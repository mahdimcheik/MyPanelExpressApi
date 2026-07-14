using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

public class CreateContactMessage
{
    [Required(ErrorMessage = "Le nom est obligatoire")]
    [MaxLength(128)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Societe { get; set; }

    [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
    [MaxLength(32)]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le sujet est obligatoire")]
    [MaxLength(256)]
    public string Sujet { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le message est obligatoire")]
    [MaxLength(5000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Pot de miel anti-spam : champ caché côté formulaire, qu'un humain ne remplit jamais.
    /// S'il est rempli, on renvoie un succès sans envoyer le mail — le robot ne doit pas
    /// apprendre qu'il a été détecté.
    /// </summary>
    public string? Website { get; set; }
}
