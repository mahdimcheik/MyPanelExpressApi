using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Controllers;

/// <summary>
/// Paramètres du site (singleton) : identité et coordonnées de l'entreprise. La ligne unique existe
/// en base dès la migration ; il n'y a ni création ni suppression, seulement lecture et modification.
/// </summary>
[Produces("application/json")]
[Route("[controller]")]
[ApiController]
[EnableCors]
[Authorize]
public class ParametresController(IParametresService parametresService) : ControllerBase
{
    /// <summary>Les paramètres du site. Site public.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<Response<ParametresDetails>>> Get()
    {
        var response = await parametresService.GetAsync();
        return StatusCode(response.Status, response);
    }

    /// <summary>Modifie les paramètres du site. Back-office.</summary>
    [HttpPut]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<ParametresDetails>>> Update(
        [FromBody] UpdateParametres dto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await parametresService.UpdateAsync(dto);
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
