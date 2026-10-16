using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

/// <summary>
/// Une image du stockage objet. La base ne conserve que <see cref="Key"/> ; l'URL publique
/// est calculée à la lecture, pour pouvoir changer de domaine ou de CDN sans migration.
/// </summary>
public class MediaDetails
{
    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;
}
