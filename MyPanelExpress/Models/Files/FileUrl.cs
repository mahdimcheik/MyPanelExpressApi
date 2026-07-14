using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

/// <summary>
/// Une image stockée sur MinIO. La base ne conserve que <see cref="Key"/> ; l'URL est
/// (re)calculée à la lecture, car les liens présignés expirent.
/// </summary>
public class MediaDetails
{
    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;
}

public class FileInfoResponse
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset? UploadDate { get; set; }
}
