using System.ComponentModel.DataAnnotations;
using MyPanelExpress.Models.Interfaces;

namespace MyPanelExpress.Models;

/// <summary>
/// Paramètres du site : les informations d'identité et de contact de l'entreprise, affichées sur
/// le site public et modifiées depuis le back-office. C'est un singleton : une seule ligne, créée
/// par la migration (HasData). L'administrateur la modifie, il ne la crée ni ne la supprime.
/// </summary>
public class Parametres : BaseModel
{
    /// <summary>Nom de l'entreprise, qui fait aussi office de nom du propriétaire.</summary>
    [MaxLength(200)]
    public string NomEntreprise { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Adresse { get; set; } = string.Empty;

    /// <summary>
    /// Plusieurs numéros possibles (mobile, fixe...). Stockés dans une colonne text[] Postgres.
    /// </summary>
    public List<string> Telephones { get; set; } = [];

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Clé de l'objet MinIO du logo. Renseigné par un flux d'upload ultérieur.</summary>
    [MaxLength(512)]
    public string? Logo { get; set; }

    [MaxLength(300)]
    public string? Facebook { get; set; }

    [MaxLength(300)]
    public string? Instagram { get; set; }

    [MaxLength(300)]
    public string? WhatsApp { get; set; }

    [MaxLength(300)]
    public string? TikTok { get; set; }
}
