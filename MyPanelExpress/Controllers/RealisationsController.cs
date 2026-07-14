using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Controllers;

/// <summary>
/// Galerie des réalisations. Les lectures publiques ne renvoient que les réalisations visibles ;
/// tout le reste est réservé à l'administrateur.
/// </summary>
[Produces("application/json")]
[Route("[controller]")]
[ApiController]
[EnableCors]
[Authorize]
public class RealisationsController(IRealisationService realisationService) : ControllerBase
{
    /// <summary>Les réalisations visibles, éventuellement filtrées par catégorie. Site public.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<Response<List<RealisationDetails>>>> GetVisibles(
        [FromQuery] string? categorie = null
    )
    {
        var response = await realisationService.GetVisiblesAsync(categorie);
        return StatusCode(response.Status, response);
    }

    /// <summary>Les dernières réalisations visibles, pour la page d'accueil. Site public.</summary>
    [HttpGet("recentes")]
    [AllowAnonymous]
    public async Task<ActionResult<Response<List<RealisationDetails>>>> GetRecentes(
        [FromQuery] int nombre = 6
    )
    {
        var response = await realisationService.GetRecentesAsync(nombre);
        return StatusCode(response.Status, response);
    }

    /// <summary>Une réalisation visible. Renvoie 404 si elle est masquée. Site public.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<Response<RealisationDetails>>> GetVisible(Guid id)
    {
        var response = await realisationService.GetVisibleByIdAsync(id);
        return StatusCode(response.Status, response);
    }

    /// <summary>Toutes les réalisations, masquées comprises. Back-office.</summary>
    [HttpGet("admin")]
    public async Task<ActionResult<Response<List<RealisationDetails>>>> GetAll()
    {
        var response = await realisationService.GetAllAsync();
        return StatusCode(response.Status, response);
    }

    /// <summary>Une réalisation, masquée ou non. Back-office.</summary>
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<Response<RealisationDetails>>> GetById(Guid id)
    {
        var response = await realisationService.GetByIdAsync(id);
        return StatusCode(response.Status, response);
    }

    /// <summary>Crée une réalisation. Les images s'ajoutent ensuite, via POST {id}/images.</summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<RealisationDetails>>> Create(
        [FromBody] CreateRealisation dto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await realisationService.CreateAsync(dto);
        return StatusCode(response.Status, response);
    }

    /// <summary>Modifie une réalisation. N'affecte pas ses images.</summary>
    [HttpPut]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<RealisationDetails>>> Update(
        [FromBody] UpdateRealisation dto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await realisationService.UpdateAsync(dto);
        return StatusCode(response.Status, response);
    }

    /// <summary>Masque ou réaffiche une réalisation, sans la supprimer.</summary>
    [HttpPatch("{id:guid}/visibilite")]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<RealisationDetails>>> SetVisibilite(
        Guid id,
        [FromBody] SetVisibilite dto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await realisationService.SetVisibiliteAsync(id, dto.Visible);
        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Supprime définitivement une réalisation, ses images et les fichiers correspondants sur
    /// MinIO. Irréversible : pour retirer temporairement un projet du site, utiliser PATCH
    /// {id}/visibilite.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Response<object>>> Delete(Guid id)
    {
        var response = await realisationService.DeleteAsync(id);
        return StatusCode(response.Status, response);
    }

    /// <summary>Ajoute une ou plusieurs images à la fin de la galerie.</summary>
    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<Response<RealisationDetails>>> AddImages(
        Guid id,
        [FromForm] IList<IFormFile> files
    )
    {
        var response = await realisationService.AddImagesAsync(id, files);
        return StatusCode(response.Status, response);
    }

    /// <summary>Supprime une image de la galerie, et le fichier sur MinIO.</summary>
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<ActionResult<Response<RealisationDetails>>> RemoveImage(Guid id, Guid imageId)
    {
        var response = await realisationService.RemoveImageAsync(id, imageId);
        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Réordonne la galerie. Le corps doit contenir toutes les images de la réalisation.
    /// </summary>
    [HttpPut("{id:guid}/images/ordre")]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<RealisationDetails>>> ReorderImages(
        Guid id,
        [FromBody] List<ImageOrdre> ordres
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await realisationService.ReorderImagesAsync(id, ordres);
        return StatusCode(response.Status, response);
    }

    private Response<object> ValidationInvalide() =>
        new()
        {
            Status = 400,
            Message = "Données de validation invalides",
            Data = ModelState,
        };
}
