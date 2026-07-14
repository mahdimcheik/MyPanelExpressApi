using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

public class HoraireDetails
{
    public Guid Id { get; set; }

    /// <summary>Sérialisé en chaîne ("Monday"...) grâce au JsonStringEnumConverter global.</summary>
    public DayOfWeek JourSemaine { get; set; }

    public TimeOnly? HeureOuverture { get; set; }
    public TimeOnly? HeureFermeture { get; set; }
    public bool Ferme { get; set; }
    public bool Visible { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public HoraireDetails() { }

    public HoraireDetails(Horaire horaire)
    {
        Id = horaire.Id;
        JourSemaine = horaire.JourSemaine;
        HeureOuverture = horaire.HeureOuverture;
        HeureFermeture = horaire.HeureFermeture;
        Ferme = horaire.Ferme;
        Visible = horaire.Visible;
        UpdatedAt = horaire.UpdatedAt;
    }
}

/// <summary>
/// Modification d'un jour. Il n'y a pas de CreateHoraire : les sept jours sont créés par la
/// migration, l'administrateur ne fait que les modifier.
/// </summary>
public class UpdateHoraire
{
    [Required]
    public Guid Id { get; set; }

    public TimeOnly? HeureOuverture { get; set; }
    public TimeOnly? HeureFermeture { get; set; }

    /// <summary>Jour de fermeture : les heures sont alors ignorées et remises à null.</summary>
    public bool Ferme { get; set; }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Un jour ouvert doit avoir ses deux heures, et l'ouverture doit précéder la fermeture.
    /// Renvoie le message d'erreur, ou null si le jour est valide.
    /// </summary>
    public string? Valider()
    {
        if (Ferme)
            return null;

        if (HeureOuverture is null || HeureFermeture is null)
            return "Un jour ouvert doit avoir une heure d'ouverture et une heure de fermeture";

        if (HeureOuverture >= HeureFermeture)
            return "L'heure d'ouverture doit précéder l'heure de fermeture";

        return null;
    }

    public void Appliquer(Horaire horaire)
    {
        horaire.Ferme = Ferme;
        horaire.Visible = Visible;

        // Un jour fermé ne garde pas d'heures résiduelles : elles ne seraient jamais affichées
        // et prêteraient à confusion dans le back-office.
        horaire.HeureOuverture = Ferme ? null : HeureOuverture;
        horaire.HeureFermeture = Ferme ? null : HeureFermeture;
        horaire.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
