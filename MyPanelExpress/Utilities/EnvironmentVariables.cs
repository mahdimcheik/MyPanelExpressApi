namespace MyPanelExpress.Utilities;

/// <summary>
/// Toute la configuration passe par les variables d'environnement. En développement,
/// elles sont chargées depuis le fichier .env (voir <see cref="DotEnv"/>) ; en production,
/// elles sont injectées par docker-compose / l'orchestrateur.
/// </summary>
public static class EnvironmentVariables
{
    private static string GetEnvVar(string envVarName, string fallbackValue)
    {
        return Environment.GetEnvironmentVariable(envVarName) ?? fallbackValue;
    }

    private static int GetEnvVarInt(string envVarName, int fallbackValue)
    {
        var envValue = Environment.GetEnvironmentVariable(envVarName);
        return int.TryParse(envValue, out var result) ? result : fallbackValue;
    }

    private static bool GetEnvVarBool(string envVarName, bool fallbackValue)
    {
        var envValue = Environment.GetEnvironmentVariable(envVarName);
        return bool.TryParse(envValue, out var result) ? result : fallbackValue;
    }

    // Valeur de repli du développement : sert aussi de sentinelle à Validate().
    public const string DEV_JWT_KEY =
        "dev-only-key-do-not-use-in-production-averyveryverylongkeyforhmacsha256-0123456789";

    // auth
    public static string JWT_KEY => GetEnvVar("JWT_KEY", DEV_JWT_KEY);
    public static string API_BACK_URL => GetEnvVar("API_BACK_URL", "http://localhost:7113");
    public static string API_FRONT_URL => GetEnvVar("API_FRONT_URL", "http://localhost:4200");
    public static int TOKEN_VALIDITY_MINUTES => GetEnvVarInt("TOKEN_VALIDITY_MINUTES", 30);

    // database
    public static string DB_HOST => GetEnvVar("DB_HOST", "localhost");
    public static string DB_PORT => GetEnvVar("DB_PORT", "5432");
    public static string DB_NAME => GetEnvVar("DB_NAME", "mypanelexpress");
    public static string DB_USER => GetEnvVar("DB_USER", "postgres");
    public static string DB_PASSWORD => GetEnvVar("DB_PASSWORD", "postgres");

    // administrateur unique, créé au démarrage
    public const string DEV_ADMIN_PASSWORD = "Password123!";
    public static string ADMIN_EMAIL => GetEnvVar("ADMIN_EMAIL", "admin@mypanelexpress.com");
    public static string ADMIN_PASSWORD => GetEnvVar("ADMIN_PASSWORD", DEV_ADMIN_PASSWORD);

    // mail (formulaire de contact)
    public static string SMTP_HOST => GetEnvVar("SMTP_HOST", "localhost");
    public static int SMTP_PORT => GetEnvVarInt("SMTP_PORT", 1025);
    public static string SMTP_LOGIN => GetEnvVar("SMTP_LOGIN", "");
    public static string SMTP_KEY => GetEnvVar("SMTP_KEY", "");

    /// <summary>Faux en local : Mailpit écoute en SMTP clair sur le port 1025.</summary>
    public static bool SMTP_SSL => GetEnvVarBool("SMTP_SSL", false);
    public static string DO_NO_REPLY_MAIL =>
        GetEnvVar("DO_NO_REPLY_MAIL", "ne-pas-repondre@mypanelexpress.com");

    /// <summary>Destinataire des messages du formulaire de contact.</summary>
    public static string CONTACT_RECIPIENT_MAIL =>
        GetEnvVar("CONTACT_RECIPIENT_MAIL", "contact@mypanelexpress.com");

    // Stockage objet : un compartiment compatible S3 (OVH Object Storage, MinIO en local).
    // S3_UPLOAD_URL est l'endpoint S3 (https://s3.<région>.io.cloud.ovh.net).
    public const string DEV_S3_SECRET_KEY = "minioadmin";
    public static string S3_UPLOAD_URL => GetEnvVar("S3_UPLOAD_URL", "http://localhost:9000");

    /// <summary>
    /// Base de lecture publique, sans slash final : l'URL publique du compartiment donnée par
    /// OVH (https://&lt;bucket&gt;.s3.&lt;région&gt;.io.cloud.ovh.net) ou un CDN devant lui.
    /// </summary>
    public static string S3_PUBLIC_URL =>
        GetEnvVar("S3_PUBLIC_URL", "http://localhost:9000/mypanelexpress");
    public static string S3_BUCKET => GetEnvVar("S3_BUCKET", "mypanelexpress");
    public static string S3_ACCESS_KEY => GetEnvVar("S3_ACCESS_KEY", "minioadmin");
    public static string S3_SECRET_KEY => GetEnvVar("S3_SECRET_KEY", DEV_S3_SECRET_KEY);

    /// <summary>
    /// Région de signature SigV4, déduite de l'endpoint OVH (s3.gra.io.cloud.ovh.net → gra) ;
    /// us-east-1 pour tout autre hôte (MinIO).
    /// </summary>
    public static string S3Region
    {
        get
        {
            var host = new Uri(S3_UPLOAD_URL).Host;
            return host.StartsWith("s3.") && host.EndsWith(".cloud.ovh.net")
                ? host.Split('.')[1]
                : "us-east-1";
        }
    }

    /// <summary>
    /// Échoue au démarrage si un secret est resté à sa valeur de développement hors Development.
    /// Sans ce garde-fou, une variable oubliée en production livre silencieusement une API
    /// signée avec une clé publiquement connue et un mot de passe admin par défaut.
    /// </summary>
    public static void Validate(IHostEnvironment env)
    {
        if (env.IsDevelopment())
            return;

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(JWT_KEY) || JWT_KEY == DEV_JWT_KEY)
            errors.Add("JWT_KEY n'est pas défini (ou utilise la valeur de développement).");

        if (string.IsNullOrWhiteSpace(ADMIN_PASSWORD) || ADMIN_PASSWORD == DEV_ADMIN_PASSWORD)
            errors.Add("ADMIN_PASSWORD n'est pas défini (ou utilise la valeur de développement).");

        if (string.IsNullOrWhiteSpace(S3_SECRET_KEY) || S3_SECRET_KEY == DEV_S3_SECRET_KEY)
            errors.Add("S3_SECRET_KEY n'est pas défini (ou utilise la valeur de développement).");

        if (errors.Count > 0)
            throw new InvalidOperationException(
                "Configuration invalide :"
                    + Environment.NewLine
                    + string.Join(Environment.NewLine, errors)
            );
    }
}
