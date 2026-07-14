using MyPanelExpress.Models;

namespace MyPanelExpress.Services.Interfaces;

public interface IMediaService
{
    /// <summary>Valide, convertit en webp et envoie l'image sur MinIO.</summary>
    Task<Response<MediaDetails>> UploadImageAsync(IFormFile file, string modelName, string type, Guid id);

    /// <summary>Renvoie une URL présignée fraîche pour une clé stockée en base.</summary>
    Task<Response<MediaDetails>> GetUrlAsync(string key);

    Task<Response<object>> DeleteAsync(string key);
}
