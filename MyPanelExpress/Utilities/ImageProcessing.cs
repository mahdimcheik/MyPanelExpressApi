using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace MyPanelExpress.Utilities;

public static class ImageProcessing
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/webp",
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".bmp",
        ".webp",
    };

    /// <summary>
    /// Pré-filtre bon marché sur le type MIME et l'extension. Ce n'est PAS la barrière de
    /// sécurité : un .txt renommé en .png passe ici. Le vrai contrôle est le décodage par
    /// ImageSharp dans <see cref="ToWebpAsync"/>, qui lève sur un contenu qui n'est pas une image.
    /// </summary>
    public static bool IsAllowedImage(IFormFile file)
    {
        if (file.Length == 0)
            return false;

        var extension = Path.GetExtension(file.FileName);

        return AllowedMimeTypes.Contains(file.ContentType)
            && !string.IsNullOrEmpty(extension)
            && AllowedExtensions.Contains(extension);
    }

    /// <summary>
    /// Décode l'image, la redimensionne pour tenir dans un carré width × height sans
    /// déformation ni recadrage (on garde la photo entière, c'est une galerie de réalisations),
    /// puis la centre sur un canevas carré via un remplissage blanc, et enfin la ré-encode
    /// en webp. Lève <see cref="UnknownImageFormatException"/> si le flux n'est pas une image.
    /// </summary>
    public static async Task<byte[]> ToWebpAsync(Stream input, int width = 800, int height = 800)
    {
        using var image = await Image.LoadAsync(input);

        image.Mutate(x =>
            x.Resize(
                new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Pad,
                    Position = AnchorPositionMode.Center,
                    PadColor = Color.White,
                }
            )
        );

        using var output = new MemoryStream();
        await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = 82 });

        return output.ToArray();
    }

    /// <summary>Clé d'objet MinIO : {modelName}/{type}/{id}/{guid}.webp</summary>
    public static string BuildObjectKey(string modelName, string type, Guid id) =>
        $"{modelName}/{type}/{id}/{Guid.NewGuid():N}.webp";
}
