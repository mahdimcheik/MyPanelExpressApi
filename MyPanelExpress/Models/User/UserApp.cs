using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using MyPanelExpress.Models.Interfaces;

namespace MyPanelExpress.Models;

/// <summary>
/// L'administrateur unique du site. Il n'y a pas d'inscription publique : le compte est
/// créé au démarrage à partir de ADMIN_EMAIL / ADMIN_PASSWORD. Pas de rôles : être
/// authentifié suffit à être administrateur.
/// </summary>
public class UserApp : IdentityUser<Guid>, ICreatable, IUpdateable
{
    [MaxLength(64)]
    public string? FirstName { get; set; }

    [MaxLength(64)]
    public string? LastName { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
