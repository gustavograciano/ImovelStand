import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar o token em todas as requisições
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Serviços de Autenticação
export const authService = {
  login: async (email, senha) => {
    const response = await api.post('/auth/login', { email, senha });
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data));
    }
    return response.data;
  },
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },
  getCurrentUser: () => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },
};

// Serviços de Clientes
export const clientesService = {
  getAll: () => api.get('/clientes'),
  getById: (id) => api.get(`/clientes/${id}`),
  create: (data) => api.post('/clientes', data),
  update: (id, data) => api.put(`/clientes/${id}`, data),
  delete: (id) => api.delete(`/clientes/${id}`),
};

// Serviços de Apartamentos
export const apartamentosService = {
  getAll: (status = null) => api.get('/apartamentos', { params: { status } }),
  getById: (id) => api.get(`/apartamentos/${id}`),
  create: (data) => api.post('/apartamentos', data),
  update: (id, data) => api.put(`/apartamentos/${id}`, data),
  delete: (id) => api.delete(`/apartamentos/${id}`),
};

// Serviços de Reservas
export const reservasService = {
  getAll: () => api.get('/reservas'),
  getById: (id) => api.get(`/reservas/${id}`),
  create: (data) => api.post('/reservas', data),
  update: (id, data) => api.put(`/reservas/${id}`, data),
  delete: (id) => api.delete(`/reservas/${id}`),
};

// Serviços de Vendas
export const vendasService = {
  getAll: () => api.get('/vendas'),
  getById: (id) => api.get(`/vendas/${id}`),
  create: (data) => api.post('/vendas', data),
  update: (id, data) => api.put(`/vendas/${id}`, data),
  delete: (id) => api.delete(`/vendas/${id}`),
};

export default api;
