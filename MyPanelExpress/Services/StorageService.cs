using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using MyPanelExpress.Models;
using MyPanelExpress.Utilities;

namespace MyPanelExpress.Services;

/// <summary>
/// Accès au stockage objet compatible S3 : OVH Object Storage en production, MinIO en local.
/// Un seul compartiment ; les "dossiers" ne sont que des préfixes de clé. Tout le contenu du
/// site est public : les objets sont envoyés en public-read et servis par URL stable. Les
/// méthodes lèvent en cas d'erreur : ce sont les services appelants qui emballent le résultat
/// dans un <see cref="Response{T}"/>.
/// </summary>
public class StorageService(IAmazonS3 s3)
{
    private readonly string _bucketName = EnvironmentVariables.S3_BUCKET;

    /// <summary>
    /// Développement uniquement : crée le compartiment MinIO s'il n'existe pas et l'ouvre en
    /// lecture anonyme. MinIO ignore les ACL d'objet, sans cette policy les URL publiques
    /// répondent 403. En production, le compartiment est créé dans le manager OVH.
    /// </summary>
    public async Task EnsureBucketAsync()
    {
        var buckets = await s3.ListBucketsAsync();
        if (buckets.Buckets?.Any(b => b.BucketName == _bucketName) != true)
        {
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = _bucketName });
        }

        await s3.PutBucketPolicyAsync(
            new PutBucketPolicyRequest
            {
                BucketName = _bucketName,
                Policy = $$"""
                {
                  "Version": "2012-10-17",
                  "Statement": [{
                    "Effect": "Allow",
                    "Principal": { "AWS": ["*"] },
                    "Action": ["s3:GetObject"],
                    "Resource": ["arn:aws:s3:::{{_bucketName}}/*"]
                  }]
                }
                """,
            }
        );
    }

    /// <summary>
    /// Envoie un buffer en mémoire. C'est le seul chemin d'upload : pas de fichier temporaire,
    /// donc rien à nettoyer.
    /// </summary>
    public async Task UploadBytesAsync(string objectName, byte[] content, string contentType)
    {
        using var stream = new MemoryStream(content, writable: false);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectName,
            InputStream = stream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead,
            AutoCloseStream = false,
        };
        // La clé contient un GUID et n'est jamais réécrite : cache illimité sans risque.
        request.Headers.CacheControl = "public, max-age=31536000, immutable";
        // Pour que le navigateur affiche l'image au lieu de la télécharger.
        request.Headers.ContentDisposition = "inline";

        await s3.PutObjectAsync(request);
    }

    /// <summary>
    /// URL publique et stable de l'objet. Calculée localement, sans appel réseau : ne jamais
    /// la persister, pour pouvoir changer de domaine ou placer un CDN devant le compartiment.
    /// </summary>
    public string Url(string objectName) =>
        $"{EnvironmentVariables.S3_PUBLIC_URL.TrimEnd('/')}/{objectName}";

    public async Task<bool> DoesFileExistsAsync(string objectName)
    {
        try
        {
            await s3.GetObjectMetadataAsync(_bucketName, objectName);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task RemoveFileAsync(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return;

        await s3.DeleteObjectAsync(_bucketName, objectName);
    }

    /// <summary>Liste les clés complètes sous un préfixe.</summary>
    public async Task<List<string>> ListAllFilesAsync(string prefix)
    {
        var keys = new List<string>();
        var request = new ListObjectsV2Request { BucketName = _bucketName, Prefix = prefix };

        ListObjectsV2Response response;
        do
        {
            response = await s3.ListObjectsV2Async(request);
            keys.AddRange(response.S3Objects?.Select(o => o.Key) ?? []);
            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated == true);

        return keys;
    }

    /// <summary>
    /// Une clé doit ressembler à celles que <see cref="ImageProcessing.BuildObjectKey"/>
    /// produit : rejette traversée, chemins absolus et caractères hors [a-zA-Z0-9-_./].
    /// </summary>
    public static bool IsValidKey(string? key) =>
        !string.IsNullOrWhiteSpace(key)
        && key.Length <= 512
        && !key.StartsWith('/')
        && !key.Contains("..")
        && !key.Contains("//")
        && key.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' or '/');
}
