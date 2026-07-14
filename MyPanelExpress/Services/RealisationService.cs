using Microsoft.EntityFrameworkCore;
using MyPanelExpress.Contexts;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Services;

/// <summary>
/// Gestion des réalisations. La base ne stocke que la clé MinIO de chaque image ; les URL
/// présignées sont régénérées à chaque lecture (elles expirent au bout de 24 h).
/// </summary>
public class RealisationService(
    MainContext context,
    IMediaService mediaService,
    MinioService minioService,
    ILogger<RealisationService> logger
) : IRealisationService
{
    private const string ModelName = "realisations";
    private const string TypeImages = "images";

    public async Task<Response<List<RealisationDetails>>> GetAllAsync()
    {
        try
        {
            var realisations = await context
                .Realisations.AsNoTracking()
                .Include(r => r.Images)
                .OrderByDescending(r => r.DateRealisation)
                .ToListAsync();

            var details = await ToDetailsAsync(realisations);

            return new Response<List<RealisationDetails>>
            {
                Status = 200,
                Message = "Réalisations récupérées avec succès",
                Data = details,
                Count = details.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des réalisations");

            return new Response<List<RealisationDetails>>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération des réalisations: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<List<RealisationDetails>>> GetVisiblesAsync(string? categorie = null)
    {
        try
        {
            var query = context
                .Realisations.AsNoTracking()
                .Include(r => r.Images)
                .Where(r => r.Visible);

            if (!string.IsNullOrWhiteSpace(categorie))
            {
                query = query.Where(r => r.Categorie == categorie);
            }

            var realisations = await query
                .OrderByDescending(r => r.DateRealisation)
                .ToListAsync();

            var details = await ToDetailsAsync(realisations);

            return new Response<List<RealisationDetails>>
            {
                Status = 200,
                Message = "Réalisations récupérées avec succès",
                Data = details,
                Count = details.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des réalisations visibles");

            return new Response<List<RealisationDetails>>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération des réalisations: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<List<RealisationDetails>>> GetRecentesAsync(int nombre = 6)
    {
        try
        {
            if (nombre <= 0)
            {
                return new Response<List<RealisationDetails>>
                {
                    Status = 400,
                    Message = "Le nombre de réalisations demandé doit être positif",
                    Data = null,
                };
            }

            var realisations = await context
                .Realisations.AsNoTracking()
                .Include(r => r.Images)
                .Where(r => r.Visible)
                .OrderByDescending(r => r.DateRealisation)
                .Take(nombre)
                .ToListAsync();

            var details = await ToDetailsAsync(realisations);

            return new Response<List<RealisationDetails>>
            {
                Status = 200,
                Message = "Réalisations récentes récupérées avec succès",
                Data = details,
                Count = details.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des réalisations récentes");

            return new Response<List<RealisationDetails>>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération des réalisations: {ex.Message}",
                Data = null,
            };
        }
    }

    public Task<Response<RealisationDetails>> GetByIdAsync(Guid id) => FindDetailsAsync(id, false);

    public Task<Response<RealisationDetails>> GetVisibleByIdAsync(Guid id) =>
        FindDetailsAsync(id, true);

    public async Task<Response<RealisationDetails>> CreateAsync(CreateRealisation dto)
    {
        try
        {
            var realisation = new Realisation
            {
                Id = Guid.NewGuid(),
                Titre = dto.Titre,
                Description = dto.Description,
                Categorie = dto.Categorie,
                DateRealisation = dto.DateRealisation,
                Visible = dto.Visible,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            context.Realisations.Add(realisation);
            await context.SaveChangesAsync();

            return new Response<RealisationDetails>
            {
                Status = 201,
                Message = "Réalisation créée avec succès",
                Data = new RealisationDetails(realisation),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la création de la réalisation");

            return new Response<RealisationDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la création de la réalisation: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<RealisationDetails>> UpdateAsync(UpdateRealisation dto)
    {
        try
        {
            var realisation = await context
                .Realisations.Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            if (realisation is null)
            {
                return NotFound();
            }

            dto.Appliquer(realisation);
            await context.SaveChangesAsync();

            return new Response<RealisationDetails>
            {
                Status = 200,
                Message = "Réalisation modifiée avec succès",
                Data = await ToDetailsAsync(realisation),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la modification de la réalisation {Id}", dto.Id);

            return new Response<RealisationDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la modification de la réalisation: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<RealisationDetails>> SetVisibiliteAsync(Guid id, bool visible)
    {
        try
        {
            var realisation = await context
                .Realisations.Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (realisation is null)
            {
                return NotFound();
            }

            realisation.Visible = visible;
            realisation.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            return new Response<RealisationDetails>
            {
                Status = 200,
                Message = visible ? "Réalisation affichée" : "Réalisation masquée",
                Data = await ToDetailsAsync(realisation),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du changement de visibilité de la réalisation {Id}", id);

            return new Response<RealisationDetails>
            {
                Status = 500,
                Message = $"Erreur lors du changement de visibilité: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<object>> DeleteAsync(Guid id)
    {
        try
        {
            var realisation = await context.Realisations.FirstOrDefaultAsync(r => r.Id == id);

            if (realisation is null)
            {
                return new Response<object>
                {
                    Status = 404,
                    Message = "Réalisation introuvable",
                    Data = null,
                };
            }

            // Suppression réelle : les lignes d'images partent en cascade (voir MainContext).
            context.Realisations.Remove(realisation);
            await context.SaveChangesAsync();

            // Les objets MinIO ensuite, et par préfixe plutôt que par clé : on ramasse aussi
            // les fichiers éventuellement orphelins. Un échec ici ne remet pas la réalisation
            // en base — elle est bel et bien supprimée — il ne laisse que des octets inutiles.
            await SupprimerObjetsAsync($"{ModelName}/{TypeImages}/{id}", id);

            return new Response<object>
            {
                Status = 200,
                Message = "Réalisation supprimée avec succès",
                Data = null,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la suppression de la réalisation {Id}", id);

            return new Response<object>
            {
                Status = 500,
                Message = $"Erreur lors de la suppression de la réalisation: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<RealisationDetails>> AddImagesAsync(Guid id, IList<IFormFile> files)
    {
        var envoyees = new List<string>();

        try
        {
            if (files is null || files.Count == 0)
            {
                return new Response<RealisationDetails>
                {
                    Status = 400,
                    Message = "Aucune image n'a été envoyée",
                    Data = null,
                };
            }

            var realisation = await context
                .Realisations.Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (realisation is null)
            {
                return NotFound();
            }

            var ordre = realisation.Images.Count == 0 ? 0 : realisation.Images.Max(i => i.Ordre) + 1;

            foreach (var file in files)
            {
                var upload = await mediaService.UploadImageAsync(file, ModelName, TypeImages, id);

                if (upload.Status != 200 || upload.Data is null)
                {
                    // Un fichier refusé annule tout le lot : on retire ce qui vient de partir
                    // sur MinIO pour ne pas laisser d'objets sans ligne en base.
                    await SupprimerObjetsAsync(envoyees, id);

                    return new Response<RealisationDetails>
                    {
                        Status = upload.Status,
                        Message = $"{file.FileName} : {upload.Message}",
                        Data = null,
                    };
                }

                envoyees.Add(upload.Data.Key);

                var image = new RealisationImage
                {
                    Id = Guid.NewGuid(),
                    ObjectKey = upload.Data.Key,
                    Ordre = ordre++,
                    RealisationId = id,
                    CreatedAt = DateTimeOffset.UtcNow,
                };

                // Add() sur le DbSet, et non realisation.Images.Add() : la clé est déjà renseignée,
                // et EF prendrait une entité découverte via la navigation pour une ligne existante
                // — il émettrait un UPDATE (0 ligne touchée) au lieu d'un INSERT. Le correctif de
                // relation d'EF rattache l'image à realisation.Images tout seul ; l'ajouter aussi
                // à la main la ferait figurer deux fois dans la galerie renvoyée.
                context.RealisationImages.Add(image);
            }

            realisation.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            return new Response<RealisationDetails>
            {
                Status = 200,
                Message = files.Count > 1 ? "Images ajoutées avec succès" : "Image ajoutée avec succès",
                Data = await ToDetailsAsync(realisation),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'ajout d'images à la réalisation {Id}", id);

            // Les lignes ne sont pas enregistrées (SaveChanges n'a pas eu lieu) : les objets
            // déjà envoyés seraient orphelins.
            await SupprimerObjetsAsync(envoyees, id);

            return new Response<RealisationDetails>
            {
                Status = 500,
                Message = $"Erreur lors de l'ajout des images: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<RealisationDetails>> RemoveImageAsync(Guid id, Guid imageId)
    {
        try
        {
            var realisation = await context
                .Realisations.Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (realisation is null)
            {
                return NotFound();
            }

            var image = realisation.Images.FirstOrDefault(i => i.Id == imageId);

            if (image is null)
            {
                return new Response<RealisationDetails>
                {
                    Status = 404,
                    Message = "Image introuvable pour cette réalisation",
                    Data = null,
                };
            }

            var key = image.ObjectKey;

            realisation.Images.Remove(image);
            context.RealisationImages.Remove(image);
            realisation.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            await SupprimerObjetsAsync([key], id);

            return new Response<RealisationDetails>
            {
                Status = 200,
                Message = "Image supprimée avec succès",
                Data = await ToDetailsAsync(realisation),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la suppression de l'image {ImageId}", imageId);

            return new Response<RealisationDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la suppression de l'image: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<RealisationDetails>> ReorderImagesAsync(
        Guid id,
        List<ImageOrdre> ordres
    )
    {
        try
        {
            var realisation = await context
                .Realisations.Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (realisation is null)
            {
                return NotFound();
            }

            // On exige la galerie complète : un ordre partiel laisserait des doublons de rang.
            var attendus = realisation.Images.Select(i => i.Id).ToHashSet();
            var recus = ordres.Select(o => o.ImageId).ToHashSet();

            if (!attendus.SetEquals(recus))
            {
                return new Response<RealisationDetails>
                {
                    Status = 400,
                    Message = "La liste doit contenir toutes les images de la réalisation, une seule fois chacune",
                    Data = null,
                };
            }

            foreach (var image in realisation.Images)
            {
                image.Ordre = ordres.First(o => o.ImageId == image.Id).Ordre;
                image.UpdatedAt = DateTimeOffset.UtcNow;
            }

            realisation.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            return new Response<RealisationDetails>
            {
                Status = 200,
                Message = "Ordre des images mis à jour",
                Data = await ToDetailsAsync(realisation),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du réordonnancement des images de la réalisation {Id}", id);

            return new Response<RealisationDetails>
            {
                Status = 500,
                Message = $"Erreur lors du réordonnancement des images: {ex.Message}",
                Data = null,
            };
        }
    }

    private async Task<Response<RealisationDetails>> FindDetailsAsync(Guid id, bool visibleUniquement)
    {
        try
        {
            var realisation = await context
                .Realisations.AsNoTracking()
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id && (!visibleUniquement || r.Visible));

            if (realisation is null)
            {
                return NotFound();
            }

            return new Response<RealisationDetails>
            {
                Status = 200,
                Message = "Réalisation récupérée avec succès",
                Data = await ToDetailsAsync(realisation),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération de la réalisation {Id}", id);

            return new Response<RealisationDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération de la réalisation: {ex.Message}",
                Data = null,
            };
        }
    }

    private static Response<RealisationDetails> NotFound() =>
        new()
        {
            Status = 404,
            Message = "Réalisation introuvable",
            Data = null,
        };

    /// <summary>Construit le DTO et signe l'URL de chaque image.</summary>
    private async Task<RealisationDetails> ToDetailsAsync(Realisation realisation)
    {
        var details = new RealisationDetails(realisation);

        foreach (var image in details.Images)
        {
            image.Url = await minioService.GetFileUrlAsync(image.ObjectKey);
        }

        return details;
    }

    private async Task<List<RealisationDetails>> ToDetailsAsync(List<Realisation> realisations)
    {
        var details = new List<RealisationDetails>(realisations.Count);

        foreach (var realisation in realisations)
        {
            details.Add(await ToDetailsAsync(realisation));
        }

        return details;
    }

    /// <summary>
    /// Retire des objets de MinIO sans faire échouer l'opération appelante : la base fait foi.
    /// Un objet resté en place n'est qu'un fichier inutile, et il est tracé dans les logs.
    /// </summary>
    private async Task SupprimerObjetsAsync(IEnumerable<string> keys, Guid id)
    {
        foreach (var key in keys)
        {
            try
            {
                await minioService.RemoveFileAsync(key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Objet MinIO {Key} non supprimé pour la réalisation {Id} : fichier orphelin",
                    key,
                    id
                );
            }
        }
    }

    /// <summary>Retire tous les objets sous un préfixe (toutes les images d'une réalisation).</summary>
    private async Task SupprimerObjetsAsync(string prefix, Guid id)
    {
        try
        {
            var keys = await minioService.ListAllFilesAsync(prefix);
            await SupprimerObjetsAsync(keys, id);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Images de la réalisation {Id} non listées sur MinIO : fichiers orphelins sous {Prefix}",
                id,
                prefix
            );
        }
    }
}
