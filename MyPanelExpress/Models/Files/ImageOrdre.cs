using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

/// <summary>
/// Nouvelle position d'une image dans une galerie. Partagé par les réalisations et les services :
/// l'administrateur renvoie la liste complète des images du parent, dans l'ordre voulu.
/// </summary>
public class ImageOrdre
{
    [Required]
    public Guid ImageId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "L'ordre doit être un entier positif")]
    public int Ordre { get; set; }
}
