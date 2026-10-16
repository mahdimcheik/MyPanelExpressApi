using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Controllers;

/// <summary>
/// Envoi et suppression des images (réalisations, services, actualités). Réservé à
/// l'administrateur : le site public ne fait que lire des URLs déjà stockées.
/// </summary>
[Produces("application/json")]
[Route("[controller]")]
[ApiController]
[EnableCors]
[Authorize]
public class MediaController(IMediaService mediaService) : ControllerBase
{
    /// <summary>Envoie une image, la convertit en webp et renvoie sa clé + son URL.</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<Response<MediaDetails>>> Upload(
        IFormFile file,
        [FromForm] string modelName = "realisations",
        [FromForm] string type = "images",
        [FromForm] Guid? id = null
    )
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(
                new Response<object>
                {
                    Status = 400,
                    Message = "Aucun fichier n'a été envoyé",
                    Data = null,
                }
            );
        }

        var response = await mediaService.UploadImageAsync(
            file,
            modelName,
            type,
            id ?? Guid.NewGuid()
        );

        return StatusCode(response.Status, response);
    }

    /// <summary>Calcule l'URL publique à partir d'une clé stockée en base.</summary>
    [HttpGet("url")]
    public async Task<ActionResult<Response<MediaDetails>>> GetUrl([FromQuery] string key)
    {
        var response = await mediaService.GetUrlAsync(key);
        return StatusCode(response.Status, response);
    }

    [HttpDelete]
    public async Task<ActionResult<Response<object>>> Delete([FromQuery] string key)
    {
        var response = await mediaService.DeleteAsync(key);
        return StatusCode(response.Status, response);
    }
}
