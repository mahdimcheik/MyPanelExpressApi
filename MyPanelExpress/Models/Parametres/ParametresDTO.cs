using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

/// <summary>Vue en lecture des paramètres du site. Renvoyée au site public comme au back-office.</summary>
public class ParametresDetails
{
    public Guid Id { get; set; }
    public string NomEntreprise { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public List<string> Telephones { get; set; } = [];
    public string Email { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public string? TikTok { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ParametresDetails() { }

    public ParametresDetails(Parametres parametres)
    {
        Id = parametres.Id;
        NomEntreprise = parametres.NomEntreprise;
        Adresse = parametres.Adresse;
        Telephones = parametres.Telephones ?? [];
        Email = parametres.Email;
        Logo = parametres.Logo;
        Facebook = parametres.Facebook;
        Instagram = parametres.Instagram;
        WhatsApp = parametres.WhatsApp;
        TikTok = parametres.TikTok;
        UpdatedAt = parametres.UpdatedAt;
    }
}

/// <summary>
/// Modification des paramètres du site. Pas de CreateParametres : la ligne unique est créée par la
/// migration, l'administrateur ne fait que la modifier. Pas d'Id non plus : il n'y a qu'une ligne.
/// </summary>
public class UpdateParametres
{
    [Required]
    [MaxLength(200)]
    public string NomEntreprise { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Adresse { get; set; } = string.Empty;

    /// <summary>Remplace intégralement la liste des numéros. Les entrées vides sont écartées.</summary>
    public List<string> Telephones { get; set; } = [];

    [MaxLength(300)]
    public string? Facebook { get; set; }

    [MaxLength(300)]
    public string? Instagram { get; set; }

    [MaxLength(300)]
    public string? WhatsApp { get; set; }

    [MaxLength(300)]
    public string? TikTok { get; set; }

    public void Appliquer(Parametres parametres)
    {
        parametres.NomEntreprise = NomEntreprise;
        parametres.Adresse = Adresse;

        // On nettoie la liste : espaces superflus retirés et numéros vides écartés, pour ne pas
        // stocker de lignes fantômes qui s'afficheraient comme des numéros manquants.
        parametres.Telephones =
            Telephones?.Select(t => t.Trim()).Where(t => t.Length > 0).ToList() ?? [];

        parametres.Facebook = Facebook;
        parametres.Instagram = Instagram;
        parametres.WhatsApp = WhatsApp;
        parametres.TikTok = TikTok;
        parametres.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
