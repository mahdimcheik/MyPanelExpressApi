using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPanelExpress.Models;

namespace MyPanelExpress.Contexts;

/// <summary>
/// IdentityUserContext (et non IdentityDbContext) : le site n'a pas de rôles, donc on évite
/// de créer les tables AspNetRoles / AspNetUserRoles / AspNetRoleClaims, qui ne serviraient
/// jamais. Il n'y a qu'un seul utilisateur, et il est administrateur par définition.
/// </summary>
public class MainContext(DbContextOptions<MainContext> options)
    : IdentityUserContext<UserApp, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
