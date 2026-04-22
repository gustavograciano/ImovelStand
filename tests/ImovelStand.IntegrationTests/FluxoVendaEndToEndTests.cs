using FluentAssertions;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;

namespace ImovelStand.IntegrationTests;

[Collection("SqlServer")]
public class FluxoVendaEndToEndTests
{
    private readonly SqlServerFixture _fixture;

    public FluxoVendaEndToEndTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requer Docker para Testcontainers; roda via CI com docker-in-docker")]
    public async Task Cenario_CompleteFluxoClienteProposta_FuncionaFimAFim()
    {
        var tenantId = Guid.NewGuid();
        var provider = new TestTenantProvider(tenantId);

        // Arrange: seed de tenant + empreendimento + apartamento + cliente
        using (var ctx = _fixture.CreateContext(provider))
        {
            var plano = ctx.Planos.FirstOrDefault(p => p.Nome == "Pro")
                ?? throw new InvalidOperationException("Plano Pro não existe");

            ctx.Tenants.Add(new Tenant
            {
                Id = tenantId,
                Nome = "Demo Teste",
                Slug = $"demo-{tenantId:N}",
                PlanoId = plano.Id,
                Ativo = true,
                CreatedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();

            var emp = new Empreendimento
            {
                TenantId = tenantId,
                Nome = "Test Emp",
                Slug = $"test-emp-{tenantId:N}",
                Status = StatusEmpreendimento.Lancamento
            };
            ctx.Empreendimentos.Add(emp);
            await ctx.SaveChangesAsync();

            var torre = new Torre { TenantId = tenantId, EmpreendimentoId = emp.Id, Nome = "A", Pavimentos = 1, ApartamentosPorPavimento = 1 };
            var tipo = new Tipologia { TenantId = tenantId, EmpreendimentoId = emp.Id, Nome = "T", AreaPrivativa = 50, AreaTotal = 60, Quartos = 2, Banheiros = 1, PrecoBase = 300_000 };
            ctx.AddRange(torre, tipo);
            await ctx.SaveChangesAsync();

            var apt = new Apartamento
            {
                TenantId = tenantId, TorreId = torre.Id, TipologiaId = tipo.Id,
                Numero = "0101", Pavimento = 1, PrecoAtual = 300_000,
                Status = StatusApartamento.Disponivel
            };
            var cliente = new Cliente
            {
                TenantId = tenantId, Nome = "Fulano", Cpf = "52998224725",
                Email = $"fulano-{Guid.NewGuid():N}@test.com", Telefone = "11"
            };
            var corretor = new Usuario
            {
                TenantId = tenantId, Nome = "Corretor", Email = $"c-{Guid.NewGuid():N}@test.com",
                SenhaHash = "hash", Role = "Corretor", Ativo = true
            };
            ctx.AddRange(apt, cliente, corretor);
            await ctx.SaveChangesAsync();

            // Act: cria proposta
            var proposta = new Proposta
            {
                TenantId = tenantId,
                Numero = $"PROP-{tenantId:N}",
                ClienteId = cliente.Id,
                ApartamentoId = apt.Id,
                CorretorId = corretor.Id,
                ValorOferecido = 295_000,
                Status = StatusProposta.Enviada,
                DataEnvio = DateTime.UtcNow,
                DataValidade = DateTime.UtcNow.AddDays(7),
                Condicao = new CondicaoPagamento { ValorTotal = 295_000, Entrada = 50_000, QtdParcelasMensais = 50, ValorParcelaMensal = 4_900, Indice = IndiceReajuste.Incc },
                CreatedAt = DateTime.UtcNow
            };
            ctx.Propostas.Add(proposta);
            await ctx.SaveChangesAsync();

            // Assert: proposta foi criada com snapshot da condição
            var saved = await ctx.Propostas.FindAsync(proposta.Id);
            saved.Should().NotBeNull();
            saved!.Condicao.ValorTotal.Should().Be(295_000);
            saved.Status.Should().Be(StatusProposta.Enviada);
        }
    }

    [Fact(Skip = "Requer Docker")]
    public async Task MultiTenant_DadosIsoladosEntreTenants()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (var ctxA = _fixture.CreateContext(new TestTenantProvider(tenantA)))
        {
            ctxA.Clientes.Add(new Cliente
            {
                TenantId = tenantA, Nome = "Cliente A",
                Cpf = $"{Random.Shared.NextInt64(10_000_000_000, 99_999_999_999)}",
                Email = $"a-{Guid.NewGuid():N}@test.com", Telefone = "11"
            });
            await ctxA.SaveChangesAsync();
        }

        using (var ctxB = _fixture.CreateContext(new TestTenantProvider(tenantB)))
        {
            var visiveis = await Task.FromResult(ctxB.Clientes.ToList());
            // Filtro multi-tenant: B não vê clientes de A
            visiveis.Where(c => c.TenantId == tenantA).Should().BeEmpty();
        }
    }
}
