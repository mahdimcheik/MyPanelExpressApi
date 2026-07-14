using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

/// <summary>
/// Masquer ou réafficher un contenu sans le supprimer. Partagé par les réalisations et les
/// services : l'administrateur retire une fiche du site public en la rendant invisible.
/// </summary>
public class SetVisibilite
{
    [Required]
    public bool Visible { get; set; }
}
