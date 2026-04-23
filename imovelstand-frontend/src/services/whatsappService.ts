import { api } from './api';

export interface WhatsAppTemplate {
  id: number;
  nome: string;
  idioma: string;
  categoria: string;
  corpo: string;
  qtdVariaveis: number;
  descricao?: string;
  ativo: boolean;
}

export interface WhatsAppMensagem {
  id: number;
  clienteId?: number;
  telefoneContato: string;
  direcao: 'Enviada' | 'Recebida';
  status: 'Pendente' | 'Aceita' | 'Entregue' | 'Lida' | 'Falhou';
  conteudo: string;
  templateId?: number;
  mensagemErro?: string;
  createdAt: string;
  enviadaEm?: string;
  entregueEm?: string;
  lidaEm?: string;
}

export const whatsappService = {
  async listarTemplates(): Promise<WhatsAppTemplate[]> {
    const r = await api.get<WhatsAppTemplate[]>('/whatsapp/templates');
    return r.data;
  },
  async listarMensagens(clienteId: number): Promise<WhatsAppMensagem[]> {
    const r = await api.get<WhatsAppMensagem[]>(`/whatsapp/clientes/${clienteId}/mensagens`);
    return r.data;
  },
  async enviarTemplate(clienteId: number, templateId: number, variaveis: string[]): Promise<WhatsAppMensagem> {
    const r = await api.post<WhatsAppMensagem>(`/whatsapp/clientes/${clienteId}/enviar-template`, {
      templateId,
      variaveis
    });
    return r.data;
  },
  async enviarTexto(clienteId: number, texto: string): Promise<WhatsAppMensagem> {
    const r = await api.post<WhatsAppMensagem>(`/whatsapp/clientes/${clienteId}/enviar-texto`, {
      texto
    });
    return r.data;
  }
};
