using Microsoft.EntityFrameworkCore;
using MyPanelExpress.Contexts;
using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Services;

/// <summary>
/// Gestion des services (les prestations). Même fonctionnement que les réalisations : la base
/// ne stocke que la clé de chaque image, l'URL publique est calculée à la lecture.
/// </summary>
public class ServiceService(
    MainContext context,
    IMediaService mediaService,
    StorageService storageService,
    ILogger<ServiceService> logger
) : IServiceService
{
    private const string ModelName = "services";
    private const string TypeImages = "images";

    public async Task<Response<List<ServiceDetails>>> GetAllAsync()
    {
        try
        {
            var services = await context
                .Services.AsNoTracking()
                .Include(s => s.Images)
                .OrderBy(s => s.Ordre)
                .ThenBy(s => s.Nom)
                .ToListAsync();

            var details = ToDetails(services);

            return new Response<List<ServiceDetails>>
            {
                Status = 200,
                Message = "Services récupérés avec succès",
                Data = details,
                Count = details.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des services");

            return new Response<List<ServiceDetails>>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération des services: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<List<ServiceDetails>>> GetVisiblesAsync()
    {
        try
        {
            var services = await context
                .Services.AsNoTracking()
                .Include(s => s.Images)
                .Where(s => s.Visible)
                .OrderBy(s => s.Ordre)
                .ThenBy(s => s.Nom)
                .ToListAsync();

            var details = ToDetails(services);

            return new Response<List<ServiceDetails>>
            {
                Status = 200,
                Message = "Services récupérés avec succès",
                Data = details,
                Count = details.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des services visibles");

            return new Response<List<ServiceDetails>>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération des services: {ex.Message}",
                Data = null,
            };
        }
    }

    public Task<Response<ServiceDetails>> GetByIdAsync(Guid id) => FindDetailsAsync(id, false);

    public Task<Response<ServiceDetails>> GetVisibleByIdAsync(Guid id) => FindDetailsAsync(id, true);

    public async Task<Response<ServiceDetails>> CreateAsync(CreateService dto)
    {
        try
        {
            var service = new Service
            {
                Id = Guid.NewGuid(),
                Nom = dto.Nom,
                Description = dto.Description,
                Avantages = dto.Avantages,
                Ordre = dto.Ordre,
                Visible = dto.Visible,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            context.Services.Add(service);
            await context.SaveChangesAsync();

            return new Response<ServiceDetails>
            {
                Status = 201,
                Message = "Service créé avec succès",
                Data = new ServiceDetails(service),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la création du service");

            return new Response<ServiceDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la création du service: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<ServiceDetails>> UpdateAsync(UpdateService dto)
    {
        try
        {
            var service = await context
                .Services.Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (service is null)
            {
                return NotFound();
            }

            dto.Appliquer(service);
            await context.SaveChangesAsync();

            return new Response<ServiceDetails>
            {
                Status = 200,
                Message = "Service modifié avec succès",
                Data = ToDetails(service),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la modification du service {Id}", dto.Id);

            return new Response<ServiceDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la modification du service: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<ServiceDetails>> SetVisibiliteAsync(Guid id, bool visible)
    {
        try
        {
            var service = await context
                .Services.Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service is null)
            {
                return NotFound();
            }

            service.Visible = visible;
            service.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            return new Response<ServiceDetails>
            {
                Status = 200,
                Message = visible ? "Service affiché" : "Service masqué",
                Data = ToDetails(service),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du changement de visibilité du service {Id}", id);

            return new Response<ServiceDetails>
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
            var service = await context.Services.FirstOrDefaultAsync(s => s.Id == id);

            if (service is null)
            {
                return new Response<object>
                {
                    Status = 404,
                    Message = "Service introuvable",
                    Data = null,
                };
            }

            // Suppression réelle : les lignes d'images partent en cascade (voir MainContext).
            context.Services.Remove(service);
            await context.SaveChangesAsync();

            // Les objets du stockage ensuite, par préfixe : on ramasse aussi les fichiers orphelins.
            await SupprimerObjetsAsync($"{ModelName}/{TypeImages}/{id}", id);

            return new Response<object>
            {
                Status = 200,
                Message = "Service supprimé avec succès",
                Data = null,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la suppression du service {Id}", id);

            return new Response<object>
            {
                Status = 500,
                Message = $"Erreur lors de la suppression du service: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<ServiceDetails>> AddImagesAsync(Guid id, IList<IFormFile> files)
    {
        var envoyees = new List<string>();

        try
        {
            if (files is null || files.Count == 0)
            {
                return new Response<ServiceDetails>
                {
                    Status = 400,
                    Message = "Aucune image n'a été envoyée",
                    Data = null,
                };
            }

            var service = await context
                .Services.Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service is null)
            {
                return NotFound();
            }

            var ordre = service.Images.Count == 0 ? 0 : service.Images.Max(i => i.Ordre) + 1;

            foreach (var file in files)
            {
                var upload = await mediaService.UploadImageAsync(file, ModelName, TypeImages, id);

                if (upload.Status != 200 || upload.Data is null)
                {
                    // Un fichier refusé annule tout le lot.
                    await SupprimerObjetsAsync(envoyees, id);

                    return new Response<ServiceDetails>
                    {
                        Status = upload.Status,
                        Message = $"{file.FileName} : {upload.Message}",
                        Data = null,
                    };
                }

                envoyees.Add(upload.Data.Key);

                var image = new ServiceImage
                {
                    Id = Guid.NewGuid(),
                    ObjectKey = upload.Data.Key,
                    Ordre = ordre++,
                    ServiceId = id,
                    CreatedAt = DateTimeOffset.UtcNow,
                };

                // Add() sur le DbSet, et non service.Images.Add() : la clé est déjà renseignée, et
                // EF prendrait une entité découverte via la navigation pour une ligne existante —
                // il émettrait un UPDATE (0 ligne touchée) au lieu d'un INSERT. Le correctif de
                // relation d'EF rattache l'image à service.Images tout seul ; l'ajouter aussi à la
                // main la ferait figurer deux fois dans la galerie renvoyée.
                context.ServiceImages.Add(image);
            }

            service.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            return new Response<ServiceDetails>
            {
                Status = 200,
                Message = files.Count > 1 ? "Images ajoutées avec succès" : "Image ajoutée avec succès",
                Data = ToDetails(service),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'ajout d'images au service {Id}", id);

            // Les lignes ne sont pas enregistrées : les objets déjà envoyés seraient orphelins.
            await SupprimerObjetsAsync(envoyees, id);

            return new Response<ServiceDetails>
            {
                Status = 500,
                Message = $"Erreur lors de l'ajout des images: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<ServiceDetails>> RemoveImageAsync(Guid id, Guid imageId)
    {
        try
        {
            var service = await context
                .Services.Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service is null)
            {
                return NotFound();
            }

            var image = service.Images.FirstOrDefault(i => i.Id == imageId);

            if (image is null)
            {
                return new Response<ServiceDetails>
                {
                    Status = 404,
                    Message = "Image introuvable pour ce service",
                    Data = null,
                };
            }

            var key = image.ObjectKey;

            service.Images.Remove(image);
            context.ServiceImages.Remove(image);
            service.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            await SupprimerObjetsAsync([key], id);

            return new Response<ServiceDetails>
            {
                Status = 200,
                Message = "Image supprimée avec succès",
                Data = ToDetails(service),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la suppression de l'image {ImageId}", imageId);

            return new Response<ServiceDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la suppression de l'image: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<ServiceDetails>> ReorderImagesAsync(Guid id, List<ImageOrdre> ordres)
    {
        try
        {
            var service = await context
                .Services.Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service is null)
            {
                return NotFound();
            }

            // On exige la galerie complète : un ordre partiel laisserait des doublons de rang.
            var attendus = service.Images.Select(i => i.Id).ToHashSet();
            var recus = ordres.Select(o => o.ImageId).ToHashSet();

            if (!attendus.SetEquals(recus))
            {
                return new Response<ServiceDetails>
                {
                    Status = 400,
                    Message = "La liste doit contenir toutes les images du service, une seule fois chacune",
                    Data = null,
                };
            }

            foreach (var image in service.Images)
            {
                image.Ordre = ordres.First(o => o.ImageId == image.Id).Ordre;
                image.UpdatedAt = DateTimeOffset.UtcNow;
            }

            service.UpdatedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();

            return new Response<ServiceDetails>
            {
                Status = 200,
                Message = "Ordre des images mis à jour",
                Data = ToDetails(service),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du réordonnancement des images du service {Id}", id);

            return new Response<ServiceDetails>
            {
                Status = 500,
                Message = $"Erreur lors du réordonnancement des images: {ex.Message}",
                Data = null,
            };
        }
    }

    private async Task<Response<ServiceDetails>> FindDetailsAsync(Guid id, bool visibleUniquement)
    {
        try
        {
            var service = await context
                .Services.AsNoTracking()
                .Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == id && (!visibleUniquement || s.Visible));

            if (service is null)
            {
                return NotFound();
            }

            return new Response<ServiceDetails>
            {
                Status = 200,
                Message = "Service récupéré avec succès",
                Data = ToDetails(service),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération du service {Id}", id);

            return new Response<ServiceDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la récupération du service: {ex.Message}",
                Data = null,
            };
        }
    }

    private static Response<ServiceDetails> NotFound() =>
        new()
        {
            Status = 404,
            Message = "Service introuvable",
            Data = null,
        };

    /// <summary>Construit le DTO et calcule l'URL publique de chaque image.</summary>
    private ServiceDetails ToDetails(Service service)
    {
        var details = new ServiceDetails(service);

        foreach (var image in details.Images)
        {
            image.Url = storageService.Url(image.ObjectKey);
        }

        return details;
    }

    private List<ServiceDetails> ToDetails(List<Service> services)
    {
        var details = new List<ServiceDetails>(services.Count);

        foreach (var service in services)
        {
            details.Add(ToDetails(service));
        }

        return details;
    }

    /// <summary>
    /// Retire des objets du stockage sans faire échouer l'opération appelante : la base fait foi.
    /// Un objet resté en place n'est qu'un fichier inutile, et il est tracé dans les logs.
    /// </summary>
    private async Task SupprimerObjetsAsync(IEnumerable<string> keys, Guid id)
    {
        foreach (var key in keys)
        {
            try
            {
                await storageService.RemoveFileAsync(key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Objet {Key} non supprimé pour le service {Id} : fichier orphelin",
                    key,
                    id
                );
            }
        }
    }

    /// <summary>Retire tous les objets sous un préfixe (toutes les images d'un service).</summary>
    private async Task SupprimerObjetsAsync(string prefix, Guid id)
    {
        try
        {
            var keys = await storageService.ListAllFilesAsync(prefix);
            await SupprimerObjetsAsync(keys, id);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Images du service {Id} non listées sur le stockage : fichiers orphelins sous {Prefix}",
                id,
                prefix
            );
        }
    }
}
