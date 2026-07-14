using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPanelExpress.Models.Interfaces;

namespace MyPanelExpress.Models;

/// <summary>
/// Un projet réalisé, présenté dans la galerie des réalisations.
/// </summary>
public class Realisation : BaseModel
{
    [Required]
    [MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Categorie { get; set; } = string.Empty;

    /// <summary>
    /// Date à laquelle le projet a été réalisé. Distincte de CreatedAt : l'administrateur
    /// peut publier aujourd'hui une réalisation datant d'il y a deux ans.
    /// </summary>
    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset DateRealisation { get; set; }

    /// <summary>
    /// Masquer plutôt que supprimer : les endpoints publics ne renvoient que Visible == true.
    /// </summary>
    public bool Visible { get; set; } = true;

    public ICollection<RealisationImage> Images { get; set; } = [];
}

/// <summary>
/// Une image d'une réalisation. Supprimée en cascade avec sa réalisation.
/// </summary>
public class RealisationImage : BaseModel
{
    /// <summary>
    /// Clé de l'objet MinIO, jamais l'URL : les liens présignés expirent au bout de 24 h et
    /// sont régénérés à la lecture par MediaService.
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>Ordre d'affichage dans la galerie de la réalisation.</summary>
    public int Ordre { get; set; }

    public Guid RealisationId { get; set; }

    public Realisation? Realisation { get; set; }
}
