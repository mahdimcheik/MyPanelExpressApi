using System.Security.Claims;
using MyPanelExpress.Models;

namespace MyPanelExpress.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Authentifie l'administrateur et renvoie un JWT.</summary>
    Task<Response<LoginResponse>> LoginAsync(UserLogin model);

    /// <summary>Renvoie l'utilisateur porté par le token.</summary>
    Task<Response<UserDetails>> GetCurrentUserAsync(ClaimsPrincipal principal);
}
