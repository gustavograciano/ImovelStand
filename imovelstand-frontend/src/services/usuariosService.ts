import { api } from './api';
import type { UsuarioResponse } from '@/types/api';

export interface UsuarioCreateRequest {
  nome: string;
  email: string;
  senha: string;
  role: string;
  creci?: string;
  percentualComissao?: number;
}

export interface UsuarioUpdateRequest {
  nome: string;
  role: string;
  creci?: string;
  percentualComissao?: number;
  ativo: boolean;
}

export const usuariosService = {
  async list(): Promise<UsuarioResponse[]> {
    const response = await api.get<UsuarioResponse[]>('/usuarios');
    return response.data;
  },
  async me(): Promise<UsuarioResponse> {
    const response = await api.get<UsuarioResponse>('/usuarios/me');
    return response.data;
  },
  async create(data: UsuarioCreateRequest): Promise<UsuarioResponse> {
    const response = await api.post<UsuarioResponse>('/usuarios', data);
    return response.data;
  },
  async update(id: number, data: UsuarioUpdateRequest): Promise<void> {
    await api.put(`/usuarios/${id}`, data);
  },
  async inativar(id: number): Promise<void> {
    await api.delete(`/usuarios/${id}`);
  },
  async trocarSenha(senhaAtual: string, novaSenha: string): Promise<void> {
    await api.post('/usuarios/me/trocar-senha', { senhaAtual, novaSenha });
  }
};
