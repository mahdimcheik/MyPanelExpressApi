using System.ComponentModel.DataAnnotations;
using MyPanelExpress.Models.Interfaces;

namespace MyPanelExpress.Models;

/// <summary>
/// Une prestation proposée par l'entreprise (impression numérique, enseignes, signalétique...).
/// </summary>
public class Service : BaseModel
{
    [Required]
    [MaxLength(200)]
    public string Nom { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>Avantages du service, un par ligne.</summary>
    public string? Avantages { get; set; }

    /// <summary>Ordre d'affichage sur la page Services et sur l'accueil.</summary>
    public int Ordre { get; set; }

    /// <summary>
    /// Masquer plutôt que supprimer : les endpoints publics ne renvoient que Visible == true.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Galerie du service. La première image par Ordre sert de vignette : il n'y a pas de
    /// colonne "image principale" séparée.
    /// </summary>
    public ICollection<ServiceImage> Images { get; set; } = [];
}

/// <summary>
/// Une image d'un service. Supprimée en cascade avec son service.
/// </summary>
public class ServiceImage : BaseModel
{
    /// <summary>
    /// Clé de l'objet dans le stockage, jamais l'URL : l'URL publique est calculée à la
    /// lecture par StorageService, pour pouvoir changer de domaine ou de CDN.
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>Ordre d'affichage dans la galerie du service.</summary>
    public int Ordre { get; set; }

    public Guid ServiceId { get; set; }

    public Service? Service { get; set; }
}
