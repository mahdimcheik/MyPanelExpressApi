using MyPanelExpress.Models.Interfaces;

namespace MyPanelExpress.Models;

/// <summary>
/// Les horaires d'ouverture, une ligne par jour de la semaine. Les sept lignes sont créées
/// par la migration (HasData) : l'administrateur les modifie, il n'en crée ni n'en supprime.
/// </summary>
public class Horaire : BaseModel
{
    /// <summary>Jour de la semaine (0 = dimanche). Unique : une seule ligne par jour.</summary>
    public DayOfWeek JourSemaine { get; set; }

    /// <summary>Ignorée si Ferme est vrai.</summary>
    public TimeOnly? HeureOuverture { get; set; }

    /// <summary>Ignorée si Ferme est vrai.</summary>
    public TimeOnly? HeureFermeture { get; set; }

    /// <summary>Jour de fermeture : les heures ne sont alors pas affichées.</summary>
    public bool Ferme { get; set; }

    /// <summary>
    /// Masquer plutôt que supprimer : les endpoints publics ne renvoient que Visible == true.
    /// </summary>
    public bool Visible { get; set; } = true;
}
