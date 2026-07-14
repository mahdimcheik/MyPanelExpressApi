using MyPanelExpress.Models;

namespace MyPanelExpress.Services.Interfaces;

/// <summary>
/// Les horaires d'ouverture. Les sept jours existent en base dès la migration : il n'y a ni
/// création ni suppression, seulement des modifications.
/// </summary>
public interface IHoraireService
{
    /// <summary>Les sept jours, masqués compris. Back-office uniquement.</summary>
    Task<Response<List<HoraireDetails>>> GetAllAsync();

    /// <summary>Les jours visibles, du lundi au dimanche. Public.</summary>
    Task<Response<List<HoraireDetails>>> GetVisiblesAsync();

    /// <summary>Modifie un jour.</summary>
    Task<Response<HoraireDetails>> UpdateAsync(UpdateHoraire dto);

    /// <summary>
    /// Enregistre la grille complète en une transaction : soit tous les jours sont modifiés,
    /// soit aucun.
    /// </summary>
    Task<Response<List<HoraireDetails>>> UpdateAllAsync(List<UpdateHoraire> dtos);
}
