using MyPanelExpress.Models;

namespace MyPanelExpress.Services.Interfaces;

/// <summary>
/// Gestion des services (les prestations proposées). Le nom redouble parce que l'entité du
/// cahier des charges s'appelle "Service" : <see cref="Models.Service"/>.
/// </summary>
public interface IServiceService
{
    /// <summary>Tous les services, visibles ou non. Back-office uniquement.</summary>
    Task<Response<List<ServiceDetails>>> GetAllAsync();

    /// <summary>Les services visibles, triés par ordre d'affichage. Public.</summary>
    Task<Response<List<ServiceDetails>>> GetVisiblesAsync();

    /// <summary>Un service, visible ou non. Back-office uniquement.</summary>
    Task<Response<ServiceDetails>> GetByIdAsync(Guid id);

    /// <summary>Un service visible. Renvoie 404 s'il est masqué. Public.</summary>
    Task<Response<ServiceDetails>> GetVisibleByIdAsync(Guid id);

    Task<Response<ServiceDetails>> CreateAsync(CreateService dto);

    Task<Response<ServiceDetails>> UpdateAsync(UpdateService dto);

    /// <summary>Masquer ou réafficher sans supprimer.</summary>
    Task<Response<ServiceDetails>> SetVisibiliteAsync(Guid id, bool visible);

    /// <summary>
    /// Suppression réelle : le service, ses lignes d'images (cascade) et les objets du stockage
    /// correspondants. Irréversible — pour retirer temporairement une fiche, utiliser
    /// <see cref="SetVisibiliteAsync"/>.
    /// </summary>
    Task<Response<object>> DeleteAsync(Guid id);

    /// <summary>Ajoute une ou plusieurs images à la fin de la galerie.</summary>
    Task<Response<ServiceDetails>> AddImagesAsync(Guid id, IList<IFormFile> files);

    /// <summary>Supprime une image : la ligne et l'objet du stockage.</summary>
    Task<Response<ServiceDetails>> RemoveImageAsync(Guid id, Guid imageId);

    /// <summary>Réordonne la galerie. La liste doit couvrir toutes les images du service.</summary>
    Task<Response<ServiceDetails>> ReorderImagesAsync(Guid id, List<ImageOrdre> ordres);
}
