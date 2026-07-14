using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyPanelExpress.Contexts;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;
using MyPanelExpress.Utilities;

namespace MyPanelExpress.Services;

/// <summary>
/// Authentification de l'administrateur unique : connexion, émission du JWT, lecture du
/// compte courant. Pas d'inscription, pas de rôles, pas de refresh token.
/// </summary>
public class AuthService(
    MainContext context,
    UserManager<UserApp> userManager,
    ILogger<AuthService> logger
) : IAuthService
{
    public async Task<Response<LoginResponse>> LoginAsync(UserLogin model)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return new Response<LoginResponse>
                {
                    Status = 404,
                    Message = "L'utilisateur n'existe pas",
                    Data = null,
                };
            }

            // Le verrouillage doit être piloté à la main : on utilise UserManager sans
            // SignInManager, or CheckPasswordAsync ne lit ni n'incrémente le compteur d'échecs.
            // Sans ces trois appels, les options de Lockout configurées dans Program.cs seraient
            // purement décoratives — et cette route est une cible de force brute évidente.
            if (await userManager.IsLockedOutAsync(user))
            {
                return new Response<LoginResponse>
                {
                    Status = 423,
                    Message = "Compte temporairement verrouillé suite à trop de tentatives",
                    Data = null,
                };
            }

            if (!await userManager.CheckPasswordAsync(user, model.Password))
            {
                await userManager.AccessFailedAsync(user);

                return new Response<LoginResponse>
                {
                    Status = 401,
                    Message = "Connexion échouée",
                    Data = null,
                };
            }

            await userManager.ResetAccessFailedCountAsync(user);

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(
                EnvironmentVariables.TOKEN_VALIDITY_MINUTES
            );

            return new Response<LoginResponse>
            {
                Status = 200,
                Message = "Connexion réussie",
                Data = new LoginResponse
                {
                    Token = GenerateAccessToken(user),
                    ExpiresAt = expiresAt,
                    User = new UserDetails(user),
                },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la connexion");

            return new Response<LoginResponse>
            {
                Status = 500,
                Message = $"Erreur lors de la connexion: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<UserDetails>> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        try
        {
            var user = await CheckUser.GetUserFromClaimAsync(principal, context);
            if (user is null)
            {
                return new Response<UserDetails>
                {
                    Status = 404,
                    Message = "L'utilisateur n'existe pas",
                    Data = null,
                };
            }

            return new Response<UserDetails>
            {
                Status = 200,
                Message = "Utilisateur récupéré avec succès",
                Data = new UserDetails(user),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur");

            return new Response<UserDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération de l'utilisateur: {ex.Message}",
                Data = null,
            };
        }
    }

    private static string GenerateAccessToken(UserApp user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)
        );
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // L'id, pour que CheckUser résolve l'utilisateur sans requête par email.
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
        };

        var token = new JwtSecurityToken(
            issuer: EnvironmentVariables.API_BACK_URL,
            audience: EnvironmentVariables.API_BACK_URL,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(EnvironmentVariables.TOKEN_VALIDITY_MINUTES),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
