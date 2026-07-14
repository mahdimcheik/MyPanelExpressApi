using MyPanelExpress.Models;

namespace MyPanelExpress.Services.Interfaces;

public interface IRealisationService
{
    /// <summary>Toutes les réalisations, visibles ou non. Back-office uniquement.</summary>
    Task<Response<List<RealisationDetails>>> GetAllAsync();

    /// <summary>Les réalisations visibles, éventuellement filtrées par catégorie. Public.</summary>
    Task<Response<List<RealisationDetails>>> GetVisiblesAsync(string? categorie = null);

    /// <summary>Les dernières réalisations visibles, pour la page d'accueil. Public.</summary>
    Task<Response<List<RealisationDetails>>> GetRecentesAsync(int nombre = 6);

    /// <summary>Une réalisation, visible ou non. Back-office uniquement.</summary>
    Task<Response<RealisationDetails>> GetByIdAsync(Guid id);

    /// <summary>Une réalisation visible. Renvoie 404 si elle est masquée. Public.</summary>
    Task<Response<RealisationDetails>> GetVisibleByIdAsync(Guid id);

    Task<Response<RealisationDetails>> CreateAsync(CreateRealisation dto);

    Task<Response<RealisationDetails>> UpdateAsync(UpdateRealisation dto);

    /// <summary>Masquer ou réafficher sans supprimer.</summary>
    Task<Response<RealisationDetails>> SetVisibiliteAsync(Guid id, bool visible);

    /// <summary>
    /// Suppression réelle : la réalisation, ses lignes d'images (cascade) et les objets MinIO
    /// correspondants. Irréversible — pour retirer temporairement un projet, utiliser
    /// <see cref="SetVisibiliteAsync"/>.
    /// </summary>
    Task<Response<object>> DeleteAsync(Guid id);

    /// <summary>Ajoute une ou plusieurs images à la fin de la galerie.</summary>
    Task<Response<RealisationDetails>> AddImagesAsync(Guid id, IList<IFormFile> files);

    /// <summary>Supprime une image : la ligne et l'objet MinIO.</summary>
    Task<Response<RealisationDetails>> RemoveImageAsync(Guid id, Guid imageId);

    /// <summary>Réordonne la galerie. La liste doit couvrir toutes les images de la réalisation.</summary>
    Task<Response<RealisationDetails>> ReorderImagesAsync(Guid id, List<ImageOrdre> ordres);
}
