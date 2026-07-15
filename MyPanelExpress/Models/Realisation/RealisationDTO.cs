using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

/// <summary>
/// Une image d'une réalisation, telle qu'elle est renvoyée au client. <see cref="Url"/> est
/// présignée et n'est pas stockée en base : c'est le service qui la remplit à la lecture.
/// </summary>
public class RealisationImageDetails
{
    public Guid Id { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
    public int Ordre { get; set; }
    public string Url { get; set; } = string.Empty;

    public RealisationImageDetails() { }

    public RealisationImageDetails(RealisationImage image)
    {
        Id = image.Id;
        ObjectKey = image.ObjectKey;
        Ordre = image.Ordre;
    }
}

public class RealisationDetails
{
    public Guid Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;
    public DateTimeOffset DateRealisation { get; set; }
    public bool Visible { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<RealisationImageDetails> Images { get; set; } = [];

    public RealisationDetails() { }

    public RealisationDetails(Realisation realisation)
    {
        Id = realisation.Id;
        Titre = realisation.Titre;
        Description = realisation.Description;
        Categorie = realisation.Categorie;
        DateRealisation = realisation.DateRealisation;
        Visible = realisation.Visible;
        CreatedAt = realisation.CreatedAt;
        UpdatedAt = realisation.UpdatedAt;

        // La collection peut ne pas avoir été chargée (pas d'Include) : on ne suppose rien.
        Images = realisation
            .Images?.OrderBy(i => i.Ordre)
            .Select(i => new RealisationImageDetails(i))
            .ToList() ?? [];
    }
}

public class CreateRealisation
{
    [Required(ErrorMessage = "Le titre est obligatoire")]
    [MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire")]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "La catégorie est obligatoire")]
    [MaxLength(100)]
    public string Categorie { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date de réalisation est obligatoire")]
    public DateTimeOffset DateRealisation { get; set; }

    public bool Visible { get; set; } = true;
}

public class UpdateRealisation
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Le titre est obligatoire")]
    [MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire")]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "La catégorie est obligatoire")]
    [MaxLength(100)]
    public string Categorie { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date de réalisation est obligatoire")]
    public DateTimeOffset DateRealisation { get; set; }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Reporte les champs sur l'entité. Les images ne sont pas touchées ici : elles ont leurs
    /// propres endpoints.
    /// </summary>
    public void Appliquer(Realisation realisation)
    {
        realisation.Titre = Titre;
        realisation.Description = Description;
        realisation.Categorie = Categorie;
        // Normalisée en UTC : Npgsql refuse un offset != 0 pour "timestamp with time zone".
        realisation.DateRealisation = DateRealisation.ToUniversalTime();
        realisation.Visible = Visible;
        realisation.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
