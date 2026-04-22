using ImovelStand.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace ImovelStand.Infrastructure.Storage;

public class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _client;
    private readonly FileStorageOptions _options;
    private readonly ILogger<MinioFileStorage> _logger;
    private bool _bucketEnsured;
    private readonly SemaphoreSlim _bucketLock = new(1, 1);

    public MinioFileStorage(IOptions<FileStorageOptions> options, ILogger<MinioFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSsl)
            .WithRegion(_options.Region)
            .Build();
    }

    public async Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureBucketAsync(cancellationToken);

        var putArgs = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(putArgs, cancellationToken);
        _logger.LogInformation("Upload {Key} ({Bytes} bytes, {ContentType})", objectKey, content.Length, contentType);
        return objectKey;
    }

    public async Task<string> GetPresignedUrlAsync(string objectKey, TimeSpan expiraEm, CancellationToken cancellationToken = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry((int)expiraEm.TotalSeconds);
        return await _client.PresignedGetObjectAsync(args);
    }

    public async Task<Stream> DownloadAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var ms = new MemoryStream();
        var args = new GetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithCallbackStream(s => s.CopyTo(ms));
        await _client.GetObjectAsync(args, cancellationToken);
        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs().WithBucket(_options.BucketName).WithObject(objectKey);
        await _client.RemoveObjectAsync(args, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new StatObjectArgs().WithBucket(_options.BucketName).WithObject(objectKey);
            await _client.StatObjectAsync(args, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        if (_bucketEnsured) return;

        await _bucketLock.WaitAsync(cancellationToken);
        try
        {
            if (_bucketEnsured) return;

            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_options.BucketName),
                cancellationToken);

            if (!exists)
            {
                await _client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_options.BucketName),
                    cancellationToken);
                _logger.LogInformation("Bucket criado: {Bucket}", _options.BucketName);
            }

            _bucketEnsured = true;
        }
        finally
        {
            _bucketLock.Release();
        }
    }
}
