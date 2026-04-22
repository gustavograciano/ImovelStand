using System.Net.Http;
using System.Net.Http.Headers;
using ImovelStand.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImovelStand.Infrastructure.Conversao;

/// <summary>
/// Converte DOCX -> PDF via <a href="https://gotenberg.dev">Gotenberg</a> (container
/// headless que embrulha LibreOffice). Endpoint: POST {serviceUrl}/forms/libreoffice/convert.
/// Em dev sem ServiceUrl, lança <see cref="InvalidOperationException"/>.
/// </summary>
public class GotenbergDocxToPdfConverter : IDocxToPdfConverter
{
    private readonly DocxToPdfOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GotenbergDocxToPdfConverter> _logger;

    public GotenbergDocxToPdfConverter(
        IOptions<DocxToPdfOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<GotenbergDocxToPdfConverter> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<byte[]> ConverterAsync(byte[] docx, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ServiceUrl))
        {
            throw new InvalidOperationException(
                "Gotenberg não configurado (DocxToPdf:ServiceUrl). Configure um container LibreOffice/Gotenberg em produção.");
        }

        var http = _httpClientFactory.CreateClient();
        http.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(docx);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        content.Add(fileContent, "files", "input.docx");

        var url = $"{_options.ServiceUrl.TrimEnd('/')}/forms/libreoffice/convert";
        var response = await http.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        _logger.LogInformation("DOCX convertido para PDF: {Input} bytes -> {Output} bytes", docx.Length, bytes.Length);
        return bytes;
    }
}
