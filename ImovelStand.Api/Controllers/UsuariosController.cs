using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Api.Authorization;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(ApplicationDbContext context, ILogger<UsuariosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<ActionResult<List<UsuarioResponse>>> Listar(CancellationToken ct)
    {
        var usuarios = await _context.Usuarios.AsNoTracking()
            .OrderBy(u => u.Nome)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Role = u.Role,
                Creci = u.Creci,
                PercentualComissao = u.PercentualComissao,
                Ativo = u.Ativo,
                UltimoLoginEm = u.UltimoLoginEm,
                DataCadastro = u.DataCadastro
            })
            .ToListAsync(ct);

        return Ok(usuarios);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UsuarioResponse>> Atual(CancellationToken ct)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(raw, out var id)) return Unauthorized();

        var u = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u is null) return NotFound();

        return Ok(new UsuarioResponse
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Role = u.Role,
            Creci = u.Creci,
            PercentualComissao = u.PercentualComissao,
            Ativo = u.Ativo,
            UltimoLoginEm = u.UltimoLoginEm,
            DataCadastro = u.DataCadastro
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RequiresPlan(limit: "usuarios")]
    public async Task<ActionResult<UsuarioResponse>> Criar([FromBody] UsuarioCreateRequest request, CancellationToken ct)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email, ct))
            return Conflict(new { message = "Email já cadastrado" });

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = PasswordPolicy.Hash(request.Senha),
            Role = request.Role,
            Creci = request.Creci,
            PercentualComissao = request.PercentualComissao,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Usuário {Email} criado com role {Role}", usuario.Email, usuario.Role);

        return CreatedAtAction(nameof(Listar), new { id = usuario.Id }, new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role,
            Creci = usuario.Creci,
            PercentualComissao = usuario.PercentualComissao,
            Ativo = usuario.Ativo,
            UltimoLoginEm = usuario.UltimoLoginEm,
            DataCadastro = usuario.DataCadastro
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] UsuarioUpdateRequest request, CancellationToken ct)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null) return NotFound();

        usuario.Nome = request.Nome;
        usuario.Role = request.Role;
        usuario.Creci = request.Creci;
        usuario.PercentualComissao = request.PercentualComissao;
        usuario.Ativo = request.Ativo;

        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("me/trocar-senha")]
    public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaRequest request, CancellationToken ct)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(raw, out var id)) return Unauthorized();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null) return NotFound();

        if (!PasswordPolicy.Verify(request.SenhaAtual, usuario.SenhaHash))
            return BadRequest(new { message = "Senha atual incorreta" });

        usuario.SenhaHash = PasswordPolicy.Hash(request.NovaSenha);

        // Invalida todas as sessões: revoga todos os refresh tokens do usuário
        var tokens = await _context.RefreshTokens.IgnoreQueryFilters()
            .Where(t => t.UsuarioId == id && t.RevogadoEm == null)
            .ToListAsync(ct);
        foreach (var t in tokens)
        {
            t.RevogadoEm = DateTime.UtcNow;
            t.MotivoRevogacao = "password_changed";
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Senha trocada para usuário {Id}; {Tokens} refresh tokens revogados.", id, tokens.Count);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null) return NotFound();

        // Soft: só inativa, não apaga (preserva FK em Vendas/Propostas)
        usuario.Ativo = false;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
