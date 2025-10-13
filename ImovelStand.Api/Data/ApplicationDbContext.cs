using Microsoft.EntityFrameworkCore;
using ImovelStand.Api.Models;

namespace ImovelStand.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Apartamento> Apartamentos { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações da tabela Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(e => e.Cpf).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configurações da tabela Apartamento
        modelBuilder.Entity<Apartamento>(entity =>
        {
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.Property(e => e.Preco).HasPrecision(18, 2);
            entity.Property(e => e.AreaMetrosQuadrados).HasPrecision(10, 2);
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Status).HasDefaultValue("Disponível");
        });

        // Configurações da tabela Venda
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

        // Configurações da tabela Reserva
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

        // Configurações da tabela Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Role).HasDefaultValue("Corretor");
            entity.Property(e => e.Ativo).HasDefaultValue(true);
        });

        // Seed de dados iniciais
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Data fixa para seed (evita erro de modelo não-determinístico)
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed de usuário padrão
        // Admin: admin@imovelstand.com / Admin@123
        // Corretor: corretor@imovelstand.com / Corretor@123
        // Hashes BCrypt pré-gerados (workfactor 11)
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

        // Seed de apartamentos
        modelBuilder.Entity<Apartamento>().HasData(
            new Apartamento
            {
                Id = 1,
                Numero = "101",
                Andar = 1,
                Quartos = 2,
                Banheiros = 1,
                AreaMetrosQuadrados = 65.5m,
                Preco = 250000.00m,
                Status = "Disponível",
                Descricao = "Apartamento com 2 quartos, sala, cozinha e área de serviço",
                DataCadastro = seedDate
            },
            new Apartamento
            {
                Id = 2,
                Numero = "102",
                Andar = 1,
                Quartos = 3,
                Banheiros = 2,
                AreaMetrosQuadrados = 85.0m,
                Preco = 350000.00m,
                Status = "Disponível",
                Descricao = "Apartamento com 3 quartos sendo 1 suíte, sala ampla e varanda",
                DataCadastro = seedDate
            },
            new Apartamento
            {
                Id = 3,
                Numero = "201",
                Andar = 2,
                Quartos = 2,
                Banheiros = 1,
                AreaMetrosQuadrados = 65.5m,
                Preco = 260000.00m,
                Status = "Disponível",
                Descricao = "Apartamento com 2 quartos, sala, cozinha e área de serviço",
                DataCadastro = seedDate
            },
            new Apartamento
            {
                Id = 4,
                Numero = "202",
                Andar = 2,
                Quartos = 3,
                Banheiros = 2,
                AreaMetrosQuadrados = 85.0m,
                Preco = 360000.00m,
                Status = "Disponível",
                Descricao = "Apartamento com 3 quartos sendo 1 suíte, sala ampla e varanda",
                DataCadastro = seedDate
            },
            new Apartamento
            {
                Id = 5,
                Numero = "301",
                Andar = 3,
                Quartos = 4,
                Banheiros = 3,
                AreaMetrosQuadrados = 120.0m,
                Preco = 480000.00m,
                Status = "Disponível",
                Descricao = "Cobertura com 4 quartos sendo 2 suítes, sala de estar e jantar, varanda gourmet",
                DataCadastro = seedDate
            }
        );
    }
}
