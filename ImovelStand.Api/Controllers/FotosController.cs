using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Abstractions;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FotosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorage _storage;
    private readonly ImageProcessor _imageProcessor;
    private readonly ILogger<FotosController> _logger;

    public FotosController(
        ApplicationDbContext context,
        IFileStorage storage,
        ImageProcessor imageProcessor,
        ILogger<FotosController> logger)
    {
        _context = context;
        _storage = storage;
        _imageProcessor = imageProcessor;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Gerente")]
    [RequestSizeLimit(ImageProcessor.MaxBytes)]
    public async Task<ActionResult<FotoUploadResponse>> Upload(
        [FromForm] TipoEntidadeFoto entidadeTipo,
        [FromForm] int entidadeId,
        [FromForm] string? legenda,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Arquivo obrigatório." });

        if (file.Length > ImageProcessor.MaxBytes)
            return BadRequest(new { message = $"Arquivo excede {ImageProcessor.MaxBytes / 1024 / 1024} MB." });

        if (!ImageProcessor.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "ContentType não suportado. Use JPEG, PNG ou WebP." });

        await using var upload = file.OpenReadStream();
        using var memory = new MemoryStream();
        await upload.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        if (!ImageProcessor.IsSupportedImage(memory))
            return BadRequest(new { message = "Arquivo não parece ser uma imagem válida (magic bytes)." });

        memory.Position = 0;
        await using var variants = await _imageProcessor.ProcessAsync(memory, cancellationToken);

        var ordem = await _context.Fotos
            .Where(f => f.EntidadeTipo == entidadeTipo && f.EntidadeId == entidadeId)
            .Select(f => (int?)f.Ordem)
            .MaxAsync(cancellationToken) ?? 0;

        var baseKey = $"{entidadeTipo.ToString().ToLowerInvariant()}/{entidadeId}/{Guid.NewGuid():N}";

        var urlOriginal = await _storage.UploadAsync($"{baseKey}.jpg", variants.Original, "image/jpeg", cancellationToken);
        var urlThumb = await _storage.UploadAsync($"{baseKey}-thumb.jpg", variants.Thumbnail, "image/jpeg", cancellationToken);
        await _storage.UploadAsync($"{baseKey}-medium.jpg", variants.Medium, "image/jpeg", cancellationToken);

        var foto = new Foto
        {
            EntidadeTipo = entidadeTipo,
            EntidadeId = entidadeId,
            Url = urlOriginal,
            ThumbnailUrl = urlThumb,
            Legenda = legenda,
            Ordem = ordem + 1,
            DataCadastro = DateTime.UtcNow
        };

        _context.Fotos.Add(foto);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new FotoUploadResponse
        {
            Id = foto.Id,
            EntidadeTipo = foto.EntidadeTipo,
            EntidadeId = foto.EntidadeId,
            Url = await _storage.GetPresignedUrlAsync(foto.Url, TimeSpan.FromHours(1), cancellationToken),
            ThumbnailUrl = foto.ThumbnailUrl is null
                ? null
                : await _storage.GetPresignedUrlAsync(foto.ThumbnailUrl, TimeSpan.FromHours(1), cancellationToken),
            Ordem = foto.Ordem,
            Legenda = foto.Legenda
        };

        return CreatedAtAction(nameof(Get), new { id = foto.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FotoUploadResponse>> Get(int id, CancellationToken cancellationToken)
    {
        var foto = await _context.Fotos.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (foto is null) return NotFound();

        return Ok(new FotoUploadResponse
        {
            Id = foto.Id,
            EntidadeTipo = foto.EntidadeTipo,
            EntidadeId = foto.EntidadeId,
            Url = await _storage.GetPresignedUrlAsync(foto.Url, TimeSpan.FromHours(1), cancellationToken),
            ThumbnailUrl = foto.ThumbnailUrl is null
                ? null
                : await _storage.GetPresignedUrlAsync(foto.ThumbnailUrl, TimeSpan.FromHours(1), cancellationToken),
            Ordem = foto.Ordem,
            Legenda = foto.Legenda
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var foto = await _context.Fotos.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (foto is null) return NotFound();

        await _storage.DeleteAsync(foto.Url, cancellationToken);
        if (foto.ThumbnailUrl is not null)
            await _storage.DeleteAsync(foto.ThumbnailUrl, cancellationToken);

        _context.Fotos.Remove(foto);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public class FotoUploadResponse
{
    public int Id { get; set; }
    public TipoEntidadeFoto EntidadeTipo { get; set; }
    public int EntidadeId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int Ordem { get; set; }
    public string? Legenda { get; set; }
}
