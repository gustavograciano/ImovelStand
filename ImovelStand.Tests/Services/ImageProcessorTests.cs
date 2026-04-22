using ImovelStand.Application.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ImovelStand.Tests.Services;

public class ImageProcessorTests
{
    private static Stream BuildJpeg(int width = 1600, int height = 900)
    {
        using var img = new Image<Rgba32>(width, height);
        var ms = new MemoryStream();
        img.SaveAsJpeg(ms, new JpegEncoder { Quality = 85 });
        ms.Position = 0;
        return ms;
    }

    private static Stream BuildPng(int width = 100, int height = 100)
    {
        using var img = new Image<Rgba32>(width, height);
        var ms = new MemoryStream();
        img.SaveAsPng(ms, new PngEncoder());
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task ProcessAsync_GeraVariantsComTamanhosEsperados()
    {
        var sut = new ImageProcessor();
        using var input = BuildJpeg();

        await using var variants = await sut.ProcessAsync(input);

        Assert.True(variants.Original.Length > 0);
        Assert.True(variants.Thumbnail.Length > 0);
        Assert.True(variants.Medium.Length > 0);

        // Thumbnail deve ser menor que medium que deve ser menor que original (mesma imagem base, tamanhos menores).
        Assert.True(variants.Thumbnail.Length <= variants.Medium.Length);
    }

    [Fact]
    public void IsSupportedImage_ComJpeg_Aprova()
    {
        using var stream = BuildJpeg(50, 50);
        Assert.True(ImageProcessor.IsSupportedImage(stream));
    }

    [Fact]
    public void IsSupportedImage_ComPng_Aprova()
    {
        using var stream = BuildPng();
        Assert.True(ImageProcessor.IsSupportedImage(stream));
    }

    [Fact]
    public void IsSupportedImage_ComExeRenomeadoPraJpg_Rejeita()
    {
        // Header "MZ" típico de EXE/DLL PE
        using var stream = new MemoryStream(new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 });
        Assert.False(ImageProcessor.IsSupportedImage(stream));
    }

    [Fact]
    public void IsSupportedImage_ComTextoPlano_Rejeita()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("not an image"));
        Assert.False(ImageProcessor.IsSupportedImage(stream));
    }
}
