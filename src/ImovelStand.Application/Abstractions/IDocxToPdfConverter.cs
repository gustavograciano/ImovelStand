namespace ImovelStand.Application.Abstractions;

public interface IDocxToPdfConverter
{
    Task<byte[]> ConverterAsync(byte[] docx, CancellationToken cancellationToken = default);
}

public class DocxToPdfOptions
{
    /// <summary>URL do container LibreOffice headless (gotenberg ou similar).</summary>
    public string? ServiceUrl { get; set; }
    public int TimeoutSeconds { get; set; } = 60;
}
