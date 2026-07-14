using Microsoft.EntityFrameworkCore;
using MyPanelExpress.Contexts;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Services;

/// <summary>
/// Gestion des horaires d'ouverture. Les sept jours sont créés par la migration : ce service
/// ne fait que les lire et les modifier, jamais les créer ni les supprimer.
/// </summary>
public class HoraireService(MainContext context, ILogger<HoraireService> logger) : IHoraireService
{
    public async Task<Response<List<HoraireDetails>>> GetAllAsync()
    {
        try
        {
            var horaires = await context.Horaires.AsNoTracking().ToListAsync();
            var details = Trier(horaires);

            return new Response<List<HoraireDetails>>
            {
                Status = 200,
                Message = "Horaires récupérés avec succès",
                Data = details,
                Count = details.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des horaires");

            return new Response<List<HoraireDetails>>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération des horaires: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<List<HoraireDetails>>> GetVisiblesAsync()
    {
        try
        {
            var horaires = await context
                .Horaires.AsNoTracking()
                .Where(h => h.Visible)
                .ToListAsync();

            var details = Trier(horaires);

            return new Response<List<HoraireDetails>>
            {
                Status = 200,
                Message = "Horaires récupérés avec succès",
                Data = details,
                Count = details.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des horaires visibles");

            return new Response<List<HoraireDetails>>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération des horaires: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<HoraireDetails>> UpdateAsync(UpdateHoraire dto)
    {
        try
        {
            var erreur = dto.Valider();

            if (erreur is not null)
            {
                return new Response<HoraireDetails>
                {
                    Status = 400,
                    Message = erreur,
                    Data = null,
                };
            }

            var horaire = await context.Horaires.FirstOrDefaultAsync(h => h.Id == dto.Id);

            if (horaire is null)
            {
                return new Response<HoraireDetails>
                {
                    Status = 404,
                    Message = "Horaire introuvable",
                    Data = null,
                };
            }

            dto.Appliquer(horaire);
            await context.SaveChangesAsync();

            return new Response<HoraireDetails>
            {
                Status = 200,
                Message = "Horaire modifié avec succès",
                Data = new HoraireDetails(horaire),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la modification de l'horaire {Id}", dto.Id);

            return new Response<HoraireDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la modification de l'horaire: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<List<HoraireDetails>>> UpdateAllAsync(List<UpdateHoraire> dtos)
    {
        try
        {
            if (dtos is null || dtos.Count == 0)
            {
                return new Response<List<HoraireDetails>>
                {
                    Status = 400,
                    Message = "Aucun horaire à modifier",
                    Data = null,
                };
            }

            // Toute la grille est validée avant d'écrire quoi que ce soit : un seul jour
            // incohérent doit faire échouer l'enregistrement en entier, pas à moitié.
            foreach (var dto in dtos)
            {
                var erreur = dto.Valider();

                if (erreur is not null)
                {
                    return new Response<List<HoraireDetails>>
                    {
                        Status = 400,
                        Message = erreur,
                        Data = null,
                    };
                }
            }

            var ids = dtos.Select(d => d.Id).ToList();

            if (ids.Distinct().Count() != ids.Count)
            {
                return new Response<List<HoraireDetails>>
                {
                    Status = 400,
                    Message = "Un même jour est présent plusieurs fois dans la liste",
                    Data = null,
                };
            }

            var horaires = await context.Horaires.Where(h => ids.Contains(h.Id)).ToListAsync();

            if (horaires.Count != dtos.Count)
            {
                return new Response<List<HoraireDetails>>
                {
                    Status = 404,
                    Message = "Un ou plusieurs horaires sont introuvables",
                    Data = null,
                };
            }

            foreach (var horaire in horaires)
            {
                dtos.First(d => d.Id == horaire.Id).Appliquer(horaire);
            }

            // Un seul SaveChanges : EF envoie le tout dans une transaction, donc soit la grille
            // entière est enregistrée, soit rien.
            await context.SaveChangesAsync();

            var details = Trier(horaires);

            return new Response<List<HoraireDetails>>
            {
                Status = 200,
                Message = "Horaires modifiés avec succès",
                Data = details,
                Count = details.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la modification des horaires");

            return new Response<List<HoraireDetails>>
            {
                Status = 500,
                Message = $"Erreur lors de la modification des horaires: {ex.Message}",
                Data = null,
            };
        }
    }

    /// <summary>
    /// Du lundi au dimanche, et non dans l'ordre de l'énumération DayOfWeek, qui commence le
    /// dimanche. Le tri se fait en mémoire : il n'y a que sept lignes.
    /// </summary>
    private static List<HoraireDetails> Trier(List<Horaire> horaires) =>
        [.. horaires
            .OrderBy(h => ((int)h.JourSemaine + 6) % 7)
            .Select(h => new HoraireDetails(h))];
}
