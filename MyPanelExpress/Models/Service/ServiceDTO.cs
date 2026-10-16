using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

/// <summary>
/// Une image d'un service. <see cref="Url"/> est publique et remplie à la lecture par le
/// service : la base ne contient que la clé.
/// </summary>
public class ServiceImageDetails
{
    public Guid Id { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
    public int Ordre { get; set; }
    public string Url { get; set; } = string.Empty;

    public ServiceImageDetails() { }

    public ServiceImageDetails(ServiceImage image)
    {
        Id = image.Id;
        ObjectKey = image.ObjectKey;
        Ordre = image.Ordre;
    }
}

public class ServiceDetails
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Avantages { get; set; }
    public int Ordre { get; set; }
    public bool Visible { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<ServiceImageDetails> Images { get; set; } = [];

    public ServiceDetails() { }

    public ServiceDetails(Service service)
    {
        Id = service.Id;
        Nom = service.Nom;
        Description = service.Description;
        Avantages = service.Avantages;
        Ordre = service.Ordre;
        Visible = service.Visible;
        CreatedAt = service.CreatedAt;
        UpdatedAt = service.UpdatedAt;

        // La collection peut ne pas avoir été chargée (pas d'Include) : on ne suppose rien.
        Images = service
            .Images?.OrderBy(i => i.Ordre)
            .Select(i => new ServiceImageDetails(i))
            .ToList() ?? [];
    }
}

public class CreateService
{
    [Required(ErrorMessage = "Le nom est obligatoire")]
    [MaxLength(200)]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire")]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Avantages du service, un par ligne.</summary>
    [MaxLength(2000)]
    public string? Avantages { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "L'ordre doit être un entier positif")]
    public int Ordre { get; set; }

    public bool Visible { get; set; } = true;
}

public class UpdateService
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [MaxLength(200)]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire")]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Avantages { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "L'ordre doit être un entier positif")]
    public int Ordre { get; set; }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Reporte les champs sur l'entité. Les images ne sont pas touchées ici : elles ont leurs
    /// propres endpoints.
    /// </summary>
    public void Appliquer(Service service)
    {
        service.Nom = Nom;
        service.Description = Description;
        service.Avantages = Avantages;
        service.Ordre = Ordre;
        service.Visible = Visible;
        service.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
