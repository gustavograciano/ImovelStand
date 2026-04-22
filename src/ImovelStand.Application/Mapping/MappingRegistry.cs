using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.ValueObjects;
using Mapster;
// Sprint 6

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
            .Ignore(dest => dest.ConsentimentoLgpdEm)
            .Ignore(dest => dest.StatusFunil)
            .Ignore(dest => dest.ConjugeId)
            .Ignore(dest => dest.CorretorResponsavel!)
            .Ignore(dest => dest.Conjuge!)
            .Ignore(dest => dest.Dependentes)
            .Ignore(dest => dest.Vendas)
            .Ignore(dest => dest.Reservas)
            .Ignore(dest => dest.Interacoes)
            .Ignore(dest => dest.Visitas);
        config.NewConfig<ClienteUpdateRequest, Cliente>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.Cpf)
            .Ignore(dest => dest.DataCadastro)
            .Ignore(dest => dest.ConsentimentoLgpd)
            .Ignore(dest => dest.ConsentimentoLgpdEm)
            .Ignore(dest => dest.CorretorResponsavel!)
            .Ignore(dest => dest.Conjuge!)
            .Ignore(dest => dest.Dependentes)
            .Ignore(dest => dest.Vendas)
            .Ignore(dest => dest.Reservas)
            .Ignore(dest => dest.Interacoes)
            .Ignore(dest => dest.Visitas);

        // HistoricoInteracao
        config.NewConfig<HistoricoInteracao, InteracaoResponse>()
            .Map(dest => dest.UsuarioNome, src => src.Usuario != null ? src.Usuario.Nome : null);
        config.NewConfig<InteracaoCreateRequest, HistoricoInteracao>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.ClienteId)
            .Ignore(dest => dest.UsuarioId)
            .Ignore(dest => dest.DataHora)
            .Ignore(dest => dest.Cliente!)
            .Ignore(dest => dest.Usuario!);

        // Visita
        config.NewConfig<Visita, VisitaResponse>()
            .Map(dest => dest.ClienteNome, src => src.Cliente != null ? src.Cliente.Nome : null)
            .Map(dest => dest.CorretorNome, src => src.Corretor != null ? src.Corretor.Nome : null)
            .Map(dest => dest.EmpreendimentoNome, src => src.Empreendimento != null ? src.Empreendimento.Nome : null);
        config.NewConfig<VisitaCreateRequest, Visita>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.DataCadastro)
            .Ignore(dest => dest.Cliente!)
            .Ignore(dest => dest.Corretor!)
            .Ignore(dest => dest.Empreendimento!);

        // Proposta + CondicaoPagamento
        config.NewConfig<CondicaoPagamento, CondicaoPagamentoDto>();
        config.NewConfig<CondicaoPagamentoDto, CondicaoPagamento>();
        config.NewConfig<Proposta, PropostaResponse>()
            .Map(dest => dest.ClienteNome, src => src.Cliente != null ? src.Cliente.Nome : null)
            .Map(dest => dest.ApartamentoNumero, src => src.Apartamento != null ? src.Apartamento.Numero : null)
            .Map(dest => dest.CorretorNome, src => src.Corretor != null ? src.Corretor.Nome : null);

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
