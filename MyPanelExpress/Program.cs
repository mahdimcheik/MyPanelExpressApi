using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyPanelExpress.Contexts;
using MyPanelExpress.Models;
using MyPanelExpress.Services;
using MyPanelExpress.Services.Interfaces;
using MyPanelExpress.Utilities;

// Doit rester la toute première instruction : `dotnet ef` exécute le code de haut niveau
// jusqu'à builder.Build() et a besoin de la chaîne de connexion issue du .env.
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

EnvironmentVariables.Validate(builder.Environment);

ConfigureServices(builder.Services);

var app = builder.Build();

app.UseForwardedHeaders(
    new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    }
);

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<MainContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<MinioService>().EnsureBucketAsync();
    await SeedAdminAsync(scope.ServiceProvider);
}

ConfigureMiddlewarePipeline(app);

app.Run();

#region Services

static void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IAuthService, AuthService>();
    services.AddTransient<IMediaService, MediaService>();
    services.AddTransient<IContactService, ContactService>();
    services.AddTransient<MinioService>();
    services.AddTransient<MailService>();

    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
        // Masque les requêtes SQL loguées en Information par EF Core, garde warnings/erreurs.
        loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    });

    services.AddRouting(opt => opt.LowercaseUrls = true);

    var connectionString =
        $"Host={EnvironmentVariables.DB_HOST};"
        + $"Port={EnvironmentVariables.DB_PORT};"
        + $"Database={EnvironmentVariables.DB_NAME};"
        + $"Username={EnvironmentVariables.DB_USER};"
        + $"Password={EnvironmentVariables.DB_PASSWORD};";

    services.AddDbContext<MainContext>(options => options.UseNpgsql(connectionString));

    // Les visuels grand format sont lourds : la limite Kestrel par défaut (30 Mo) est
    // relevée, sinon l'upload meurt en 413 avant même d'atteindre le contrôleur.
    services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 50_000_000);

    ConfigureCors(services);
    ConfigureControllers(services);
    ConfigureRateLimiting(services);
    ConfigureSwagger(services);
    ConfigureIdentity(services);
    ConfigureAuthentication(services);
}

static void ConfigureCors(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy
                .SetIsOriginAllowed(CorsHelper.IsOriginAllowed)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}

static void ConfigureControllers(IServiceCollection services)
{
    services
        .AddControllers()
        .AddJsonOptions(options =>
            // Les enums en string dans le JSON, sinon le générateur de client Angular les sort en int.
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
        );

    services.AddEndpointsApiExplorer();
}

static void ConfigureRateLimiting(IServiceCollection services)
{
    // Le formulaire de contact est anonyme : sans limite, c'est un relais à spam.
    services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy(
            "contact",
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(10),
                    }
                )
        );
    });
}

static void ConfigureIdentity(IServiceCollection services)
{
    // AddIdentityCore, pas AddIdentity : on ne veut ni RoleManager (pas de rôles) ni
    // SignInManager, et surtout pas le schéma d'authentification par cookie qu'AddIdentity
    // enregistre — il entrerait en concurrence avec JWT sur le schéma de challenge par défaut.
    services.AddIdentityCore<UserApp>().AddEntityFrameworkStores<MainContext>();

    services.Configure<IdentityOptions>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;

        // Pas de flux de confirmation : l'admin est créé au démarrage avec EmailConfirmed = true.
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
    });
}

static void ConfigureAuthentication(IServiceCollection services)
{
    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = EnvironmentVariables.API_BACK_URL,
                ValidAudience = EnvironmentVariables.API_BACK_URL,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)
                ),
            };
        });

    services.AddAuthorization();
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "MyPanelExpress API",
                Version = "v1",
                Description = "API du site vitrine MyPanelExpress",
            }
        );

        var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        c.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Saisir 'Bearer' [espace] puis le token. Exemple : \"Bearer 12345abcdef\"",
            }
        );

        c.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            }
        );
    });
}

#endregion

#region Middleware

static void ConfigureMiddlewarePipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyPanelExpress API v1");
            c.RoutePrefix = string.Empty;
            c.DocumentTitle = "MyPanelExpress API";
        });
    }

    app.UseStaticFiles();

    app.UseRouting();
    app.UseCors();

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}

#endregion

#region Seed

// Crée l'administrateur unique au premier démarrage. La garde porte sur "y a-t-il déjà un
// utilisateur" et non sur l'email : changer ADMIN_EMAIL après coup ne doit pas créer un
// second compte, le site n'en a qu'un.
static async Task SeedAdminAsync(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<UserApp>>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    if (await userManager.Users.AnyAsync())
        return;

    var admin = new UserApp
    {
        UserName = EnvironmentVariables.ADMIN_EMAIL,
        Email = EnvironmentVariables.ADMIN_EMAIL,
        EmailConfirmed = true,
        FirstName = "Admin",
        LastName = "MyPanelExpress",
    };

    var result = await userManager.CreateAsync(admin, EnvironmentVariables.ADMIN_PASSWORD);

    if (!result.Succeeded)
    {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));

        // Sans admin, personne ne peut se connecter : autant échouer bruyamment au démarrage.
        logger.LogError("Échec de la création de l'administrateur : {Errors}", errors);
        throw new InvalidOperationException($"Impossible de créer l'administrateur : {errors}");
    }

    logger.LogInformation("Administrateur créé : {Email}", admin.Email);
}

#endregion

public partial class Program { }
