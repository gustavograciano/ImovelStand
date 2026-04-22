using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Abstractions;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public static readonly Guid DemoTenantId = new("11111111-1111-1111-1111-111111111111");

    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Plano> Planos => Set<Plano>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Apartamento> Apartamentos => Set<Apartamento>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Empreendimento> Empreendimentos => Set<Empreendimento>();
    public DbSet<Torre> Torres => Set<Torre>();
    public DbSet<Tipologia> Tipologias => Set<Tipologia>();
    public DbSet<HistoricoPreco> HistoricoPrecos => Set<HistoricoPreco>();
    public DbSet<Foto> Fotos => Set<Foto>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ClienteDependente> ClienteDependentes => Set<ClienteDependente>();
    public DbSet<HistoricoInteracao> HistoricoInteracoes => Set<HistoricoInteracao>();
    public DbSet<Visita> Visitas => Set<Visita>();
    public DbSet<Proposta> Propostas => Set<Proposta>();
    public DbSet<PropostaHistoricoStatus> PropostaHistoricos => Set<PropostaHistoricoStatus>();
    public DbSet<Comissao> Comissoes => Set<Comissao>();
    public DbSet<ContratoTemplate> ContratoTemplates => Set<ContratoTemplate>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Plano>(entity =>
        {
            entity.Property(e => e.PrecoMensal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Cnpj).IsUnique().HasFilter("[Cnpj] IS NOT NULL");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Plano)
                .WithMany()
                .HasForeignKey(e => e.PlanoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Cpf }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.RendaMensal).HasPrecision(18, 2);
            entity.Property(e => e.EstadoCivil).HasConversion<int?>();
            entity.Property(e => e.RegimeBens).HasConversion<int?>();
            entity.Property(e => e.OrigemLead).HasConversion<int?>();
            entity.Property(e => e.StatusFunil).HasConversion<int>();
            entity.OwnsOne(e => e.Endereco);

            entity.HasOne(e => e.CorretorResponsavel)
                .WithMany()
                .HasForeignKey(e => e.CorretorResponsavelId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Conjuge)
                .WithMany()
                .HasForeignKey(e => e.ConjugeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ClienteDependente>(entity =>
        {
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Dependentes)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HistoricoInteracao>(entity =>
        {
            entity.HasIndex(e => new { e.ClienteId, e.DataHora });
            entity.Property(e => e.Tipo).HasConversion<int>();
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Interacoes)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Visita>(entity =>
        {
            entity.HasIndex(e => new { e.EmpreendimentoId, e.DataHora });
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Visitas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Corretor)
                .WithMany()
                .HasForeignKey(e => e.CorretorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Empreendimento)
                .WithMany()
                .HasForeignKey(e => e.EmpreendimentoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Role).HasDefaultValue("Corretor");
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.PercentualComissao).HasPrecision(9, 4);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.UsuarioId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Ignore(e => e.EstaAtivo);

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Empreendimento>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Slug }).IsUnique();
            entity.Property(e => e.VgvEstimado).HasPrecision(18, 2);
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.OwnsOne(e => e.Endereco);
        });

        modelBuilder.Entity<Torre>(entity =>
        {
            entity.HasIndex(e => new { e.EmpreendimentoId, e.Nome }).IsUnique();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Empreendimento)
                .WithMany(emp => emp.Torres)
                .HasForeignKey(e => e.EmpreendimentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tipologia>(entity =>
        {
            entity.HasIndex(e => new { e.EmpreendimentoId, e.Nome }).IsUnique();
            entity.Property(e => e.AreaPrivativa).HasPrecision(10, 2);
            entity.Property(e => e.AreaTotal).HasPrecision(10, 2);
            entity.Property(e => e.PrecoBase).HasPrecision(18, 2);
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Empreendimento)
                .WithMany(emp => emp.Tipologias)
                .HasForeignKey(e => e.EmpreendimentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Apartamento>(entity =>
        {
            entity.HasIndex(e => new { e.TorreId, e.Numero }).IsUnique();
            entity.Property(e => e.PrecoAtual).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<int>().HasDefaultValue(StatusApartamento.Disponivel);
            entity.Property(e => e.Orientacao).HasConversion<int?>();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Torre)
                .WithMany(t => t.Apartamentos)
                .HasForeignKey(e => e.TorreId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Tipologia)
                .WithMany(tp => tp.Apartamentos)
                .HasForeignKey(e => e.TipologiaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HistoricoPreco>(entity =>
        {
            entity.Property(e => e.PrecoAnterior).HasPrecision(18, 2);
            entity.Property(e => e.PrecoNovo).HasPrecision(18, 2);
            entity.Property(e => e.DataAlteracao).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Apartamento)
                .WithMany(a => a.HistoricoPrecos)
                .HasForeignKey(e => e.ApartamentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Foto>(entity =>
        {
            entity.HasIndex(e => new { e.EntidadeTipo, e.EntidadeId });
            entity.Property(e => e.EntidadeTipo).HasConversion<int>();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Venda>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Numero }).IsUnique();
            entity.Property(e => e.ValorFinal).HasPrecision(18, 2);
            entity.Property(e => e.DataFechamento).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.OwnsOne(e => e.CondicaoFinal, cb =>
            {
                cb.Property(c => c.ValorTotal).HasPrecision(18, 2);
                cb.Property(c => c.Entrada).HasPrecision(18, 2);
                cb.Property(c => c.Sinal).HasPrecision(18, 2);
                cb.Property(c => c.ValorParcelaMensal).HasPrecision(18, 2);
                cb.Property(c => c.ValorSemestral).HasPrecision(18, 2);
                cb.Property(c => c.ValorChaves).HasPrecision(18, 2);
                cb.Property(c => c.ValorPosChaves).HasPrecision(18, 2);
                cb.Property(c => c.TaxaJurosAnual).HasPrecision(9, 4);
                cb.Property(c => c.Indice).HasConversion<int>();
                cb.Ignore(c => c.ValorPagoTotal);
            });

            entity.HasOne(e => e.Proposta)
                .WithMany()
                .HasForeignKey(e => e.PropostaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Vendas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Apartamento)
                .WithMany(a => a.Vendas)
                .HasForeignKey(e => e.ApartamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Corretor)
                .WithMany()
                .HasForeignKey(e => e.CorretorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CorretorCaptacao)
                .WithMany()
                .HasForeignKey(e => e.CorretorCaptacaoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.GerenteAprovador)
                .WithMany()
                .HasForeignKey(e => e.GerenteAprovadorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Comissao>(entity =>
        {
            entity.HasIndex(e => new { e.VendaId, e.UsuarioId, e.Tipo }).IsUnique();
            entity.Property(e => e.Percentual).HasPrecision(9, 4);
            entity.Property(e => e.Valor).HasPrecision(18, 2);
            entity.Property(e => e.Tipo).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Venda)
                .WithMany(v => v.Comissoes)
                .HasForeignKey(e => e.VendaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.Property(e => e.DataReserva).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Status).HasDefaultValue("Ativa");

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Reservas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Apartamento)
                .WithMany(a => a.Reservas)
                .HasForeignKey(e => e.ApartamentoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Proposta>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Numero }).IsUnique();
            entity.HasIndex(e => e.ApartamentoId);
            entity.Property(e => e.ValorOferecido).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.OwnsOne(e => e.Condicao, cb =>
            {
                cb.Property(c => c.ValorTotal).HasPrecision(18, 2);
                cb.Property(c => c.Entrada).HasPrecision(18, 2);
                cb.Property(c => c.Sinal).HasPrecision(18, 2);
                cb.Property(c => c.ValorParcelaMensal).HasPrecision(18, 2);
                cb.Property(c => c.ValorSemestral).HasPrecision(18, 2);
                cb.Property(c => c.ValorChaves).HasPrecision(18, 2);
                cb.Property(c => c.ValorPosChaves).HasPrecision(18, 2);
                cb.Property(c => c.TaxaJurosAnual).HasPrecision(9, 4);
                cb.Property(c => c.Indice).HasConversion<int>();
                cb.Ignore(c => c.ValorPagoTotal);
            });

            entity.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Apartamento).WithMany().HasForeignKey(e => e.ApartamentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Corretor).WithMany().HasForeignKey(e => e.CorretorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PropostaOriginal).WithMany().HasForeignKey(e => e.PropostaOriginalId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PropostaHistoricoStatus>(entity =>
        {
            entity.HasIndex(e => new { e.PropostaId, e.DataAlteracao });
            entity.Property(e => e.StatusAnterior).HasConversion<int>();
            entity.Property(e => e.StatusNovo).HasConversion<int>();
            entity.Property(e => e.DataAlteracao).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Proposta)
                .WithMany(p => p.Historico)
                .HasForeignKey(e => e.PropostaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContratoTemplate>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Nome }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<WebhookSubscription>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Evento });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        ApplyTenantFilters(modelBuilder);

        SeedData(modelBuilder);
    }

    /// <summary>
    /// Filtro global multi-tenant: toda query em entidade <see cref="ITenantEntity"/> filtra
    /// por <c>TenantId == _tenantProvider.TenantId</c>. Quando o provider não tem tenant
    /// (migrations design-time, seed, jobs sem escopo), o filtro vira no-op.
    /// </summary>
    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType)) continue;

            var method = typeof(ApplicationDbContext)
                .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);
            method.Invoke(this, new object[] { modelBuilder });
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantEntity
    {
        // Quando a request não tem tenant (jobs, migrations design-time, seed), o filtro
        // vira no-op — queries enxergam todos os tenants. Em request autenticada,
        // HasTenant == true e o filtro aplica TenantId == request tenant.
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !_tenantProvider.HasTenant || e.TenantId == _tenantProvider.TenantId);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var tenantId = DemoTenantId;

        modelBuilder.Entity<Plano>().HasData(
            new Plano { Id = 1, Nome = "Starter", PrecoMensal = 299m, MaxEmpreendimentos = 1, MaxUnidades = 100, MaxUsuarios = 3, Ativo = true },
            new Plano { Id = 2, Nome = "Pro", PrecoMensal = 899m, MaxEmpreendimentos = 5, MaxUnidades = 500, MaxUsuarios = 15, Ativo = true },
            new Plano { Id = 3, Nome = "Business", PrecoMensal = 2499m, MaxEmpreendimentos = 50, MaxUnidades = 10_000, MaxUsuarios = 100, Ativo = true }
        );

        modelBuilder.Entity<Tenant>().HasData(new Tenant
        {
            Id = tenantId,
            Nome = "Imobiliária Demo",
            Slug = "demo",
            PlanoId = 2,
            Ativo = true,
            CreatedAt = seedDate
        });

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                TenantId = tenantId,
                Nome = "Administrador",
                Email = "admin@imovelstand.com",
                SenhaHash = "$2a$11$r4aHE2PnR4xi9noJxkzqe.2SIC5DqPZvinTi8EmFOHsMRIWcrPkqi",
                Role = "Admin",
                DataCadastro = seedDate,
                Ativo = true
            },
            new Usuario
            {
                Id = 2,
                TenantId = tenantId,
                Nome = "Corretor Teste",
                Email = "corretor@imovelstand.com",
                SenhaHash = "$2a$11$D8sg3FrM1EI689Z905iG2ubYw/m6LSlI3au9TWZFWd9dCFhw9rxQS",
                Role = "Corretor",
                DataCadastro = seedDate,
                Ativo = true
            }
        );

        modelBuilder.Entity<Empreendimento>().HasData(new
        {
            Id = 1,
            TenantId = tenantId,
            Nome = "Residencial Exemplo",
            Slug = "residencial-exemplo",
            Descricao = "Empreendimento demo para ambiente de desenvolvimento",
            Construtora = "Construtora Demo Ltda",
            DataLancamento = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DataEntregaPrevista = new DateTime(2027, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = StatusEmpreendimento.Lancamento,
            VgvEstimado = 38_000_000m,
            DataCadastro = seedDate
        });

        modelBuilder.Entity<Empreendimento>()
            .OwnsOne(e => e.Endereco)
            .HasData(new
            {
                EmpreendimentoId = 1,
                Logradouro = "Avenida Demo",
                Numero = "1000",
                Bairro = "Centro",
                Cidade = "Sao Paulo",
                Uf = "SP",
                Cep = "01000-000"
            });

        modelBuilder.Entity<Torre>().HasData(
            new Torre { Id = 1, TenantId = tenantId, EmpreendimentoId = 1, Nome = "Torre A", Pavimentos = 12, ApartamentosPorPavimento = 2, DataCadastro = seedDate },
            new Torre { Id = 2, TenantId = tenantId, EmpreendimentoId = 1, Nome = "Torre B", Pavimentos = 12, ApartamentosPorPavimento = 2, DataCadastro = seedDate }
        );

        modelBuilder.Entity<Tipologia>().HasData(
            new Tipologia { Id = 1, TenantId = tenantId, EmpreendimentoId = 1, Nome = "2Q Garden", AreaPrivativa = 55m, AreaTotal = 70m, Quartos = 2, Suites = 0, Banheiros = 1, Vagas = 1, PrecoBase = 350_000m, DataCadastro = seedDate },
            new Tipologia { Id = 2, TenantId = tenantId, EmpreendimentoId = 1, Nome = "3Q Suite", AreaPrivativa = 75m, AreaTotal = 95m, Quartos = 3, Suites = 1, Banheiros = 2, Vagas = 2, PrecoBase = 520_000m, DataCadastro = seedDate },
            new Tipologia { Id = 3, TenantId = tenantId, EmpreendimentoId = 1, Nome = "Cobertura Duplex", AreaPrivativa = 140m, AreaTotal = 180m, Quartos = 3, Suites = 2, Banheiros = 3, Vagas = 2, PrecoBase = 1_200_000m, DataCadastro = seedDate }
        );

        modelBuilder.Entity<Apartamento>().HasData(BuildApartamentosSeed(seedDate, tenantId));
    }

    private static Apartamento[] BuildApartamentosSeed(DateTime seedDate, Guid tenantId)
    {
        var apts = new List<Apartamento>();
        int id = 1;

        // 2 torres x 12 pavimentos x 2 apts/pav = 48 unidades
        for (int torreId = 1; torreId <= 2; torreId++)
        {
            for (int pav = 1; pav <= 12; pav++)
            {
                for (int unidade = 1; unidade <= 2; unidade++)
                {
                    var isCobertura = pav == 12;
                    var tipologiaId = isCobertura ? 3 : (unidade == 1 ? 1 : 2);
                    var precoBase = tipologiaId switch
                    {
                        1 => 350_000m,
                        2 => 520_000m,
                        _ => 1_200_000m
                    };
                    // pavimento mais alto = premio de 1% por andar sobre o base
                    var preco = precoBase * (1 + 0.01m * (pav - 1));

                    apts.Add(new Apartamento
                    {
                        Id = id++,
                        TenantId = tenantId,
                        TorreId = torreId,
                        TipologiaId = tipologiaId,
                        Numero = $"{pav:00}{unidade:00}",
                        Pavimento = pav,
                        Orientacao = unidade == 1 ? Orientacao.Norte : Orientacao.Sul,
                        PrecoAtual = Math.Round(preco, 2),
                        Status = StatusApartamento.Disponivel,
                        DataCadastro = seedDate
                    });
                }
            }
        }

        return apts.ToArray();
    }
}
