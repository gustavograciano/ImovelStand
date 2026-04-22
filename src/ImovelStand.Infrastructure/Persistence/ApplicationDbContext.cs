using Microsoft.EntityFrameworkCore;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(e => e.Cpf).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Role).HasDefaultValue("Corretor");
            entity.Property(e => e.Ativo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Empreendimento>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
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
            entity.Property(e => e.ValorVenda).HasPrecision(18, 2);
            entity.Property(e => e.ValorEntrada).HasPrecision(18, 2);
            entity.Property(e => e.DataVenda).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Status).HasDefaultValue("Concluída");

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Vendas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Apartamento)
                .WithMany(a => a.Vendas)
                .HasForeignKey(e => e.ApartamentoId)
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

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
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
            new Torre { Id = 1, EmpreendimentoId = 1, Nome = "Torre A", Pavimentos = 12, ApartamentosPorPavimento = 2, DataCadastro = seedDate },
            new Torre { Id = 2, EmpreendimentoId = 1, Nome = "Torre B", Pavimentos = 12, ApartamentosPorPavimento = 2, DataCadastro = seedDate }
        );

        modelBuilder.Entity<Tipologia>().HasData(
            new Tipologia { Id = 1, EmpreendimentoId = 1, Nome = "2Q Garden", AreaPrivativa = 55m, AreaTotal = 70m, Quartos = 2, Suites = 0, Banheiros = 1, Vagas = 1, PrecoBase = 350_000m, DataCadastro = seedDate },
            new Tipologia { Id = 2, EmpreendimentoId = 1, Nome = "3Q Suite", AreaPrivativa = 75m, AreaTotal = 95m, Quartos = 3, Suites = 1, Banheiros = 2, Vagas = 2, PrecoBase = 520_000m, DataCadastro = seedDate },
            new Tipologia { Id = 3, EmpreendimentoId = 1, Nome = "Cobertura Duplex", AreaPrivativa = 140m, AreaTotal = 180m, Quartos = 3, Suites = 2, Banheiros = 3, Vagas = 2, PrecoBase = 1_200_000m, DataCadastro = seedDate }
        );

        modelBuilder.Entity<Apartamento>().HasData(BuildApartamentosSeed(seedDate));
    }

    private static Apartamento[] BuildApartamentosSeed(DateTime seedDate)
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
