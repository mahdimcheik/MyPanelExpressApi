using Microsoft.EntityFrameworkCore;
using MyPanelExpress.Contexts;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Services;

/// <summary>
/// Paramètres du site. La ligne unique est créée par la migration : ce service ne fait que la lire
/// et la modifier, jamais la créer ni la supprimer.
/// </summary>
public class ParametresService(MainContext context, ILogger<ParametresService> logger)
    : IParametresService
{
    public async Task<Response<ParametresDetails>> GetAsync()
    {
        try
        {
            var parametres = await context.Parametres.AsNoTracking().FirstOrDefaultAsync();

            if (parametres is null)
            {
                return new Response<ParametresDetails>
                {
                    Status = 404,
                    Message = "Paramètres introuvables",
                    Data = null,
                };
            }

            return new Response<ParametresDetails>
            {
                Status = 200,
                Message = "Paramètres récupérés avec succès",
                Data = new ParametresDetails(parametres),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des paramètres");

            return new Response<ParametresDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération des paramètres: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<ParametresDetails>> UpdateAsync(UpdateParametres dto)
    {
        try
        {
            var parametres = await context.Parametres.FirstOrDefaultAsync();

            if (parametres is null)
            {
                return new Response<ParametresDetails>
                {
                    Status = 404,
                    Message = "Paramètres introuvables",
                    Data = null,
                };
            }

            dto.Appliquer(parametres);
            await context.SaveChangesAsync();

            return new Response<ParametresDetails>
            {
                Status = 200,
                Message = "Paramètres modifiés avec succès",
                Data = new ParametresDetails(parametres),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la modification des paramètres");

            return new Response<ParametresDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la modification des paramètres: {ex.Message}",
                Data = null,
            };
        }
    }
}
