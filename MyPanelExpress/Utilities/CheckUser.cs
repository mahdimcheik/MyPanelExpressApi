using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyPanelExpress.Contexts;
using MyPanelExpress.Models;

namespace MyPanelExpress.Utilities;

public static class CheckUser
{
    /// <summary>
    /// Résout l'utilisateur à partir du claim NameIdentifier (l'id), et non de l'email :
    /// le token porte l'id, donc une seule lecture par clé primaire suffit.
    /// </summary>
    public static async Task<UserApp?> GetUserFromClaimAsync(
        ClaimsPrincipal principal,
        MainContext context
    )
    {
        var rawId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(rawId, out var userId))
            return null;

        return await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}
