using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Fluxo de onboarding: cria Tenant + primeiro usuário Admin + empreendimento
/// demo com 48 unidades. Objetivo do plano: tempo até primeiro valor &lt; 5min.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class OnboardingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(ApplicationDbContext context, ILogger<OnboardingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<ActionResult<OnboardingResponse>> Start([FromBody] OnboardingRequest request, CancellationToken ct)
    {
        using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var slugNormalizado = Slugify(request.NomeEmpresa);
            if (await _context.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Slug == slugNormalizado, ct))
                slugNormalizado = $"{slugNormalizado}-{Guid.NewGuid().ToString("N")[..6]}";

            var plano = await _context.Planos.FirstOrDefaultAsync(p => p.Nome == "Pro", ct)
                ?? throw new InvalidOperationException("Plano Pro não encontrado (migration BillingIugu aplicada?).");

            var tenantId = Guid.NewGuid();
            var tenant = new Tenant
            {
                Id = tenantId,
                Nome = request.NomeEmpresa,
                Cnpj = request.Cnpj,
                Slug = slugNormalizado,
                PlanoId = plano.Id,
                TrialAte = DateTime.UtcNow.AddDays(14),
                Ativo = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);

            if (await _context.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == request.AdminEmail, ct))
                return Conflict(new { message = "Email admin já em uso." });

            var admin = new Usuario
            {
                TenantId = tenantId,
                Nome = request.AdminNome,
                Email = request.AdminEmail,
                SenhaHash = PasswordPolicy.Hash(request.AdminSenha),
                Role = "Admin",
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            _context.Usuarios.Add(admin);

            int? empreendimentoId = null;
            if (request.CriarEmpreendimentoDemo)
            {
                var emp = new Empreendimento
                {
                    TenantId = tenantId,
                    Nome = $"{request.NomeEmpresa} - Empreendimento Demo",
                    Slug = $"{slugNormalizado}-demo",
                    Descricao = "Empreendimento criado automaticamente no onboarding",
                    Status = StatusEmpreendimento.Lancamento,
                    DataCadastro = DateTime.UtcNow
                };
                _context.Empreendimentos.Add(emp);
                await _context.SaveChangesAsync(ct);

                var torre = new Torre
                {
                    TenantId = tenantId,
                    EmpreendimentoId = emp.Id,
                    Nome = "Torre Demo",
                    Pavimentos = 12,
                    ApartamentosPorPavimento = 4,
                    DataCadastro = DateTime.UtcNow
                };
                _context.Torres.Add(torre);

                var tipologia = new Tipologia
                {
                    TenantId = tenantId,
                    EmpreendimentoId = emp.Id,
                    Nome = "Padrão 2Q",
                    AreaPrivativa = 55,
                    AreaTotal = 70,
                    Quartos = 2,
                    Suites = 0,
                    Banheiros = 1,
                    Vagas = 1,
                    PrecoBase = 350_000m,
                    DataCadastro = DateTime.UtcNow
                };
                _context.Tipologias.Add(tipologia);
                await _context.SaveChangesAsync(ct);

                for (var pav = 1; pav <= 12; pav++)
                {
                    for (var uni = 1; uni <= 4; uni++)
                    {
                        var preco = 350_000m * (1 + 0.01m * (pav - 1));
                        _context.Apartamentos.Add(new Apartamento
                        {
                            TenantId = tenantId,
                            TorreId = torre.Id,
                            TipologiaId = tipologia.Id,
                            Numero = $"{pav:00}{uni:00}",
                            Pavimento = pav,
                            PrecoAtual = Math.Round(preco, 2),
                            Status = StatusApartamento.Disponivel,
                            DataCadastro = DateTime.UtcNow
                        });
                    }
                }
                empreendimentoId = emp.Id;
            }

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            _logger.LogInformation("Onboarding concluído: tenant={Slug} admin={Email} demo={Demo}",
                slugNormalizado, request.AdminEmail, request.CriarEmpreendimentoDemo);

            return Ok(new OnboardingResponse
            {
                TenantId = tenantId,
                Slug = slugNormalizado,
                EmpreendimentoDemoId = empreendimentoId,
                TrialAte = tenant.TrialAte!.Value
            });
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static string Slugify(string input) =>
        new string(input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ã", "a").Replace("á", "a").Replace("à", "a").Replace("â", "a")
            .Replace("é", "e").Replace("ê", "e")
            .Replace("í", "i")
            .Replace("ó", "o").Replace("ô", "o").Replace("õ", "o")
            .Replace("ú", "u")
            .Replace("ç", "c")
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());
}

public class OnboardingRequest
{
    public string NomeEmpresa { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string AdminNome { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminSenha { get; set; } = string.Empty;
    public bool CriarEmpreendimentoDemo { get; set; } = true;
}

public class OnboardingResponse
{
    public Guid TenantId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int? EmpreendimentoDemoId { get; set; }
    public DateTime TrialAte { get; set; }
}
