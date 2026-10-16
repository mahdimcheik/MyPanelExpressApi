using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Controllers;

/// <summary>
/// Fiches des services proposés. Les lectures publiques ne renvoient que les services visibles,
/// triés par ordre d'affichage ; tout le reste est réservé à l'administrateur.
/// </summary>
[Produces("application/json")]
[Route("[controller]")]
[ApiController]
[EnableCors]
[Authorize]
public class ServicesController(IServiceService serviceService) : ControllerBase
{
    /// <summary>Les services visibles, triés par ordre d'affichage. Site public.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<Response<List<ServiceDetails>>>> GetVisibles()
    {
        var response = await serviceService.GetVisiblesAsync();
        return StatusCode(response.Status, response);
    }

    /// <summary>Un service visible. Renvoie 404 s'il est masqué. Site public.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<Response<ServiceDetails>>> GetVisible(Guid id)
    {
        var response = await serviceService.GetVisibleByIdAsync(id);
        return StatusCode(response.Status, response);
    }

    /// <summary>Tous les services, masqués compris. Back-office.</summary>
    [HttpGet("admin")]
    public async Task<ActionResult<Response<List<ServiceDetails>>>> GetAll()
    {
        var response = await serviceService.GetAllAsync();
        return StatusCode(response.Status, response);
    }

    /// <summary>Un service, masqué ou non. Back-office.</summary>
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<Response<ServiceDetails>>> GetById(Guid id)
    {
        var response = await serviceService.GetByIdAsync(id);
        return StatusCode(response.Status, response);
    }

    /// <summary>Crée un service. Les images s'ajoutent ensuite, via POST {id}/images.</summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<ServiceDetails>>> Create([FromBody] CreateService dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await serviceService.CreateAsync(dto);
        return StatusCode(response.Status, response);
    }

    /// <summary>Modifie un service. N'affecte pas ses images.</summary>
    [HttpPut]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<ServiceDetails>>> Update([FromBody] UpdateService dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await serviceService.UpdateAsync(dto);
        return StatusCode(response.Status, response);
    }

    /// <summary>Masque ou réaffiche un service, sans le supprimer.</summary>
    [HttpPatch("{id:guid}/visibilite")]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<ServiceDetails>>> SetVisibilite(
        Guid id,
        [FromBody] SetVisibilite dto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await serviceService.SetVisibiliteAsync(id, dto.Visible);
        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Supprime définitivement un service, ses images et les fichiers correspondants du stockage.
    /// Irréversible : pour retirer temporairement une fiche du site, utiliser PATCH
    /// {id}/visibilite.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Response<object>>> Delete(Guid id)
    {
        var response = await serviceService.DeleteAsync(id);
        return StatusCode(response.Status, response);
    }

    /// <summary>Ajoute une ou plusieurs images à la fin de la galerie.</summary>
    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<Response<ServiceDetails>>> AddImages(
        Guid id,
        [FromForm] IList<IFormFile> files
    )
    {
        var response = await serviceService.AddImagesAsync(id, files);
        return StatusCode(response.Status, response);
    }

    /// <summary>Supprime une image de la galerie, et le fichier du stockage.</summary>
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<ActionResult<Response<ServiceDetails>>> RemoveImage(Guid id, Guid imageId)
    {
        var response = await serviceService.RemoveImageAsync(id, imageId);
        return StatusCode(response.Status, response);
    }

    /// <summary>Réordonne la galerie. Le corps doit contenir toutes les images du service.</summary>
    [HttpPut("{id:guid}/images/ordre")]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<ServiceDetails>>> ReorderImages(
        Guid id,
        [FromBody] List<ImageOrdre> ordres
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await serviceService.ReorderImagesAsync(id, ordres);
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
