using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Controllers;

[Produces("application/json")]
[Consumes("application/json")]
[Route("[controller]")]
[ApiController]
[EnableCors]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Connexion de l'administrateur.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<Response<LoginResponse>>> Login([FromBody] UserLogin model)
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

        var response = await authService.LoginAsync(model);
        return StatusCode(response.Status, response);
    }

    /// <summary>Renvoie l'utilisateur authentifié par le token.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<Response<UserDetails>>> Me()
    {
        var response = await authService.GetCurrentUserAsync(User);
        return StatusCode(response.Status, response);
    }
}
