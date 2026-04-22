using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize(Roles = "Admin,Gerente")]
[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ExcelImporter _importer;
    private readonly ILogger<ImportController> _logger;

    public ImportController(ApplicationDbContext context, ExcelImporter importer, ILogger<ImportController> logger)
    {
        _context = context;
        _importer = importer;
        _logger = logger;
    }

    [HttpPost("tabela-precos/preview")]
    public ActionResult<ImportResult<TabelaPrecoRow>> PreviewTabelaPrecos(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Arquivo obrigatório." });

        using var stream = file.OpenReadStream();
        var result = _importer.ParseTabelaPrecos(stream);
        return Ok(result);
    }

    [HttpPost("tabela-precos/confirmar")]
    public async Task<IActionResult> ConfirmarTabelaPrecos(IFormFile file, CancellationToken ct)
    {
        if (file is null) return BadRequest(new { message = "Arquivo obrigatório." });

        using var stream = file.OpenReadStream();
        var parsed = _importer.ParseTabelaPrecos(stream);
        if (parsed.Invalidos > 0)
            return BadRequest(new { message = "Planilha tem linhas inválidas. Rode preview primeiro.", parsed.Errors });

        var torres = await _context.Torres.ToDictionaryAsync(t => t.Nome, t => t.Id, ct);
        var atualizados = 0;
        var naoEncontrados = new List<string>();

        foreach (var row in parsed.Items)
        {
            if (!torres.TryGetValue(row.TorreNome, out var torreId))
            {
                naoEncontrados.Add($"{row.TorreNome}/{row.ApartamentoNumero}");
                continue;
            }
            var apt = await _context.Apartamentos
                .FirstOrDefaultAsync(a => a.TorreId == torreId && a.Numero == row.ApartamentoNumero, ct);
            if (apt is null)
            {
                naoEncontrados.Add($"{row.TorreNome}/{row.ApartamentoNumero}");
                continue;
            }
            apt.PrecoAtual = row.NovoPreco;
            atualizados++;
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Tabela de preços: {Atualizados} atualizados, {NaoEncontrados} não encontrados.", atualizados, naoEncontrados.Count);

        return Ok(new { atualizados, naoEncontrados });
    }

    [HttpPost("clientes/preview")]
    public ActionResult<ImportResult<ClienteCreateRequest>> PreviewClientes(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Arquivo obrigatório." });

        using var stream = file.OpenReadStream();
        return Ok(_importer.ParseClientes(stream));
    }

    [HttpPost("clientes/confirmar")]
    public async Task<IActionResult> ConfirmarClientes(IFormFile file, CancellationToken ct)
    {
        if (file is null) return BadRequest(new { message = "Arquivo obrigatório." });

        using var stream = file.OpenReadStream();
        var parsed = _importer.ParseClientes(stream);

        var cpfsExistentes = await _context.Clientes.Select(c => c.Cpf).ToHashSetAsync(ct);
        var inseridos = 0;
        var duplicados = new List<string>();

        foreach (var dto in parsed.Items)
        {
            if (cpfsExistentes.Contains(dto.Cpf))
            {
                duplicados.Add(dto.Cpf);
                continue;
            }
            _context.Clientes.Add(new Cliente
            {
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                Telefone = dto.Telefone,
                OrigemLead = dto.OrigemLead,
                DataCadastro = DateTime.UtcNow
            });
            cpfsExistentes.Add(dto.Cpf);
            inseridos++;
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { inseridos, duplicados, errosParse = parsed.Errors });
    }
}
