using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;
using MyPanelExpress.Utilities;
using SixLabors.ImageSharp;

namespace MyPanelExpress.Services;

/// <summary>
/// Pipeline d'images : validation, conversion en webp, envoi sur MinIO. La base ne stocke
/// que la clé de l'objet ; l'URL présignée est régénérée à chaque lecture.
/// </summary>
public class MediaService(MinioService minioService, ILogger<MediaService> logger) : IMediaService
{
    public async Task<Response<MediaDetails>> UploadImageAsync(
        IFormFile file,
        string modelName,
        string type,
        Guid id
    )
    {
        try
        {
            if (!ImageProcessing.IsAllowedImage(file))
            {
                return new Response<MediaDetails>
                {
                    Status = 400,
                    Message = "Type de fichier non autorisé. Formats acceptés : jpg, png, gif, bmp, webp",
                    Data = null,
                };
            }

            byte[] webp;
            try
            {
                await using var stream = file.OpenReadStream();
                webp = await ImageProcessing.ToWebpAsync(stream);
            }
            catch (UnknownImageFormatException)
            {
                // Extension correcte mais contenu qui n'est pas une image : c'est ici,
                // au décodage, que ça se voit — pas au contrôle du type MIME.
                return new Response<MediaDetails>
                {
                    Status = 400,
                    Message = "Le fichier n'est pas une image valide",
                    Data = null,
                };
            }

            var key = ImageProcessing.BuildObjectKey(modelName, type, id);
            await minioService.UploadBytesAsync(key, webp, "image/webp");

            return new Response<MediaDetails>
            {
                Status = 200,
                Message = "Image envoyée avec succès",
                Data = new MediaDetails { Key = key, Url = await minioService.GetFileUrlAsync(key) },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'envoi de l'image");

            return new Response<MediaDetails>
            {
                Status = 500,
                Message = $"Erreur lors de l'envoi de l'image: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<MediaDetails>> GetUrlAsync(string key)
    {
        try
        {
            if (!await minioService.DoesFileExistsAsync(key))
            {
                return new Response<MediaDetails>
                {
                    Status = 404,
                    Message = "Le fichier n'existe pas",
                    Data = null,
                };
            }

            return new Response<MediaDetails>
            {
                Status = 200,
                Message = "URL générée avec succès",
                Data = new MediaDetails { Key = key, Url = await minioService.GetFileUrlAsync(key) },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la génération de l'URL");

            return new Response<MediaDetails>
            {
                Status = 500,
                Message = $"Erreur lors de la génération de l'URL: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<Response<object>> DeleteAsync(string key)
    {
        try
        {
            await minioService.RemoveFileAsync(key);

            return new Response<object>
            {
                Status = 200,
                Message = "Image supprimée avec succès",
                Data = null,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la suppression de l'image");

            return new Response<object>
            {
                Status = 500,
                Message = $"Erreur lors de la suppression de l'image: {ex.Message}",
                Data = null,
            };
        }
    }
}
