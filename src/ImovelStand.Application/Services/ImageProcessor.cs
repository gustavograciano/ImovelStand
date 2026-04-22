using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImovelStand.Application.Services;

public class ImageProcessor
{
    public const int ThumbnailWidth = 400;
    public const int MediumWidth = 800;
    public const long MaxBytes = 10 * 1024 * 1024; // 10 MB

    public static readonly string[] AllowedContentTypes =
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public async Task<ImageVariants> ProcessAsync(Stream input, CancellationToken cancellationToken = default)
    {
        input.Position = 0;
        using var original = await Image.LoadAsync(input, cancellationToken);

        var (origStream, _) = await EncodeAsync(original.Clone(_ => { }), quality: 90, cancellationToken);
        var thumb = original.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(ThumbnailWidth, 0),
            Mode = ResizeMode.Max
        }));
        var (thumbStream, _) = await EncodeAsync(thumb, quality: 80, cancellationToken);

        var medium = original.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(MediumWidth, 0),
            Mode = ResizeMode.Max
        }));
        var (mediumStream, _) = await EncodeAsync(medium, quality: 85, cancellationToken);

        return new ImageVariants(origStream, thumbStream, mediumStream, original.Width, original.Height);
    }

    private static async Task<(MemoryStream stream, string contentType)> EncodeAsync(
        Image image, int quality, CancellationToken cancellationToken)
    {
        var ms = new MemoryStream();
        await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = quality }, cancellationToken);
        ms.Position = 0;
        return (ms, "image/jpeg");
    }

    /// <summary>Verifica "magic bytes" para garantir que o arquivo é realmente uma imagem suportada.</summary>
    public static bool IsSupportedImage(Stream stream)
    {
        if (!stream.CanSeek) return false;
        stream.Position = 0;

        Span<byte> header = stackalloc byte[12];
        var read = stream.Read(header);
        stream.Position = 0;
        if (read < 4) return false;

        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
        // PNG: 89 50 4E 47
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
        // WEBP: "RIFF....WEBP"
        if (read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) return true;

        return false;
    }
}

public record ImageVariants(Stream Original, Stream Thumbnail, Stream Medium, int OriginalWidth, int OriginalHeight) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Original.DisposeAsync();
        await Thumbnail.DisposeAsync();
        await Medium.DisposeAsync();
    }
}
