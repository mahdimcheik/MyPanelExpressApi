using MyPanelExpress.Models;

namespace MyPanelExpress.Services.Interfaces;

/// <summary>
/// Paramètres du site (singleton). La ligne unique existe en base dès la migration : il n'y a ni
/// création ni suppression, seulement lecture et modification.
/// </summary>
public interface IParametresService
{
    /// <summary>Les paramètres du site. Public (site vitrine) comme back-office.</summary>
    Task<Response<ParametresDetails>> GetAsync();

    /// <summary>Modifie les paramètres du site.</summary>
    Task<Response<ParametresDetails>> UpdateAsync(UpdateParametres dto);
}
