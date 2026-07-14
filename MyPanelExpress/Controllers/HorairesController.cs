using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Controllers;

/// <summary>
/// Horaires d'ouverture. Les sept jours existent en base dès la migration : il n'y a ni
/// création ni suppression, seulement des modifications.
/// </summary>
[Produces("application/json")]
[Route("[controller]")]
[ApiController]
[EnableCors]
[Authorize]
public class HorairesController(IHoraireService horaireService) : ControllerBase
{
    /// <summary>Les jours visibles, du lundi au dimanche. Site public.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<Response<List<HoraireDetails>>>> GetVisibles()
    {
        var response = await horaireService.GetVisiblesAsync();
        return StatusCode(response.Status, response);
    }

    /// <summary>Les sept jours, masqués compris. Back-office.</summary>
    [HttpGet("admin")]
    public async Task<ActionResult<Response<List<HoraireDetails>>>> GetAll()
    {
        var response = await horaireService.GetAllAsync();
        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Enregistre la grille complète en une seule transaction : un jour incohérent fait échouer
    /// tout l'enregistrement.
    /// </summary>
    [HttpPut]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<List<HoraireDetails>>>> UpdateAll(
        [FromBody] List<UpdateHoraire> dtos
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        var response = await horaireService.UpdateAllAsync(dtos);
        return StatusCode(response.Status, response);
    }

    /// <summary>Modifie un seul jour.</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<ActionResult<Response<HoraireDetails>>> Update(
        Guid id,
        [FromBody] UpdateHoraire dto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ValidationInvalide());
        }

        if (id != dto.Id)
        {
            return BadRequest(
                new Response<object>
                {
                    Status = 400,
                    Message = "L'identifiant de l'URL ne correspond pas à celui du corps de la requête",
                    Data = null,
                }
            );
        }

        var response = await horaireService.UpdateAsync(dto);
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
