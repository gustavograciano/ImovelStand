using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.ValueObjects;
using Mapster;

namespace ImovelStand.Application.Mapping;

public static class MappingRegistry
{
    public static void Register(TypeAdapterConfig config)
    {
        // Cliente
        config.NewConfig<Cliente, ClienteResponse>();
        config.NewConfig<ClienteCreateRequest, Cliente>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.DataCadastro)
            .Ignore(dest => dest.Vendas)
            .Ignore(dest => dest.Reservas);
        config.NewConfig<ClienteUpdateRequest, Cliente>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.Cpf)
            .Ignore(dest => dest.DataCadastro)
            .Ignore(dest => dest.Vendas)
            .Ignore(dest => dest.Reservas);

        // Apartamento
        config.NewConfig<Apartamento, ApartamentoResponse>()
            .Map(dest => dest.TorreNome, src => src.Torre != null ? src.Torre.Nome : null)
            .Map(dest => dest.TipologiaNome, src => src.Tipologia != null ? src.Tipologia.Nome : null);
        config.NewConfig<ApartamentoCreateRequest, Apartamento>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.DataCadastro)
            .Ignore(dest => dest.Torre!)
            .Ignore(dest => dest.Tipologia!)
            .Ignore(dest => dest.Vendas)
            .Ignore(dest => dest.Reservas)
            .Ignore(dest => dest.HistoricoPrecos);
        config.NewConfig<ApartamentoUpdateRequest, Apartamento>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.TorreId)
            .Ignore(dest => dest.DataCadastro)
            .Ignore(dest => dest.Torre!)
            .Ignore(dest => dest.Tipologia!)
            .Ignore(dest => dest.Vendas)
            .Ignore(dest => dest.Reservas)
            .Ignore(dest => dest.HistoricoPrecos);

        // Empreendimento + Endereco
        config.NewConfig<Endereco, EnderecoDto>();
        config.NewConfig<EnderecoDto, Endereco>();
        config.NewConfig<Empreendimento, EmpreendimentoResponse>();
        config.NewConfig<EmpreendimentoCreateRequest, Empreendimento>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.DataCadastro)
            .Ignore(dest => dest.Torres)
            .Ignore(dest => dest.Tipologias);
        config.NewConfig<EmpreendimentoUpdateRequest, Empreendimento>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.DataCadastro)
            .Ignore(dest => dest.Torres)
            .Ignore(dest => dest.Tipologias);
    }
}
