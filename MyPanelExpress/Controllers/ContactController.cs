using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Controllers;

/// <summary>Formulaire de contact du site public.</summary>
[Produces("application/json")]
[Consumes("application/json")]
[Route("[controller]")]
[ApiController]
[EnableCors]
[AllowAnonymous]
public class ContactController(IContactService contactService) : ControllerBase
{
    /// <summary>Envoie un message à l'entreprise.</summary>
    [HttpPost]
    [EnableRateLimiting("contact")]
    public async Task<ActionResult<Response<object>>> Send([FromBody] CreateContactMessage dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                new Response<object>
                {
                    Status = 400,
                    Message = "Données de validation invalides",
                    Data = ModelState,
                }
            );
        }

        var response = await contactService.SendMessageAsync(dto);
        return StatusCode(response.Status, response);
    }
}
