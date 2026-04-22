namespace ImovelStand.Application.Abstractions;

public interface IFileStorage
{
    /// <summary>Faz upload de um arquivo e retorna a chave (path) no storage.</summary>
    Task<string> UploadAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>Gera URL pré-assinada para leitura (expira em <paramref name="expiraEm"/>).</summary>
    Task<string> GetPresignedUrlAsync(
        string objectKey,
        TimeSpan expiraEm,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}

public class FileStorageOptions
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public bool UseSsl { get; set; } = false;
    public string BucketName { get; set; } = "imovelstand";
    public string Region { get; set; } = "us-east-1";
}
