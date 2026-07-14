using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Minio.Exceptions;
using MyPanelExpress.Models;
using MyPanelExpress.Utilities;

namespace MyPanelExpress.Services;

/// <summary>
/// Accès au stockage objet MinIO. Un seul bucket ; les "dossiers" ne sont que des préfixes
/// de clé. Les méthodes renvoient les types bruts du SDK et lèvent en cas d'erreur : ce sont
/// les services appelants qui emballent le résultat dans un <see cref="Response{T}"/>.
/// </summary>
public class MinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName = EnvironmentVariables.MINIO_BUCKETNAME;

    public MinioService()
    {
        _minioClient = new MinioClient()
            .WithEndpoint(EnvironmentVariables.MINIO_ENDPOINT)
            .WithCredentials(
                EnvironmentVariables.MINIO_ACCESSKEY,
                EnvironmentVariables.MINIO_SECRETKEY
            )
            .WithSSL(EnvironmentVariables.MINIO_USE_SSL)
            .Build();
    }

    /// <summary>
    /// Crée le bucket s'il n'existe pas. Appelé au démarrage pour qu'un `docker compose up`
    /// tout neuf fonctionne sans créer le bucket à la main dans la console MinIO.
    /// </summary>
    public async Task EnsureBucketAsync()
    {
        var exists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucketName)
        );

        if (!exists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
        }
    }

    /// <summary>
    /// Envoie un buffer en mémoire. C'est le seul chemin d'upload : pas de fichier temporaire,
    /// donc rien à nettoyer.
    /// </summary>
    public async Task<PutObjectResponse> UploadBytesAsync(
        string objectName,
        byte[] content,
        string contentType
    )
    {
        var metadata = new Dictionary<string, string>
        {
            { "Content-Type", contentType },
            // Pour que le navigateur affiche l'image au lieu de la télécharger.
            { "x-amz-meta-content-disposition", "inline" },
        };

        using var stream = new MemoryStream(content);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType)
            .WithHeaders(metadata);

        return await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
    }

    /// <summary>URL présignée, valable 24 h.</summary>
    public async Task<string> GetFileUrlAsync(string objectName)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithExpiry(60 * 60 * 24);

        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public async Task<bool> DoesFileExistsAsync(string objectName)
    {
        try
        {
            var stat = await _minioClient.StatObjectAsync(
                new StatObjectArgs().WithBucket(_bucketName).WithObject(objectName)
            );

            return stat is not null;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    public async Task RemoveFileAsync(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return;

        await _minioClient
            .RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucketName).WithObject(objectName))
            .ConfigureAwait(false);
    }

    /// <summary>Liste les clés complètes sous un préfixe.</summary>
    public async Task<List<string>> ListAllFilesAsync(string prefix)
    {
        var args = new ListObjectsArgs()
            .WithBucket(_bucketName)
            .WithPrefix(prefix)
            .WithRecursive(true);

        var keys = new List<string>();

        await foreach (var item in _minioClient.ListObjectsEnumAsync(args))
        {
            keys.Add(item.Key);
        }

        return keys;
    }

    /// <summary>Tous les fichiers d'une entité, avec leur URL présignée.</summary>
    public async Task<List<FileInfoResponse>> GetAllFilesAsync(
        Guid id,
        string modelName,
        string type
    )
    {
        var keys = await ListAllFilesAsync($"{modelName}/{type}/{id}");
        var files = new List<FileInfoResponse>();

        foreach (var key in keys)
        {
            files.Add(
                new FileInfoResponse
                {
                    Name = Path.GetFileName(key),
                    Url = await GetFileUrlAsync(key),
                }
            );
        }

        return files;
    }
}
