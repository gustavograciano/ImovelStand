import { useState, useEffect } from 'react';
import { authService, apartamentosService, clientesService, reservasService, vendasService } from './services/api';
import './App.css';

function App() {
  const [user, setUser] = useState(null);
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [activeTab, setActiveTab] = useState('apartamentos');
  const [apartamentos, setApartamentos] = useState([]);
  const [clientes, setClientes] = useState([]);
  const [selectedApartamento, setSelectedApartamento] = useState(null);
  const [clienteForm, setClienteForm] = useState({ nome: '', cpf: '', email: '', telefone: '' });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    const currentUser = authService.getCurrentUser();
    if (currentUser) {
      setUser(currentUser);
      loadData();
    }
  }, []);

  const loadData = async () => {
    try {
      const [apResponse, clResponse] = await Promise.all([
        apartamentosService.getAll(),
        clientesService.getAll()
      ]);
      setApartamentos(apResponse.data);
      setClientes(clResponse.data);
    } catch (err) {
      console.error('Erro ao carregar dados:', err);
    }
  };

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const userData = await authService.login(email, senha);
      setUser(userData);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Erro ao fazer login');
    }
  };

  const handleLogout = () => {
    authService.logout();
    setUser(null);
    setApartamentos([]);
    setClientes([]);
  };

  const handleCreateCliente = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      await clientesService.create(clienteForm);
      setSuccess('Cliente cadastrado com sucesso!');
      setClienteForm({ nome: '', cpf: '', email: '', telefone: '' });
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Erro ao cadastrar cliente');
    }
  };

  const handleCreateReserva = async (apartamentoId, clienteId) => {
    setError('');
    setSuccess('');
    try {
      await reservasService.create({
        apartamentoId,
        clienteId,
        observacoes: 'Reserva criada via sistema web'
      });
      setSuccess('Reserva criada com sucesso!');
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Erro ao criar reserva');
    }
  };

  const handleCreateVenda = async (apartamentoId, clienteId, valorVenda, valorEntrada) => {
    setError('');
    setSuccess('');
    try {
      await vendasService.create({
        apartamentoId,
        clienteId,
        valorVenda,
        valorEntrada,
        formaPagamento: 'Financiamento',
        observacoes: 'Venda realizada via sistema web'
      });
      setSuccess('Venda realizada com sucesso!');
      loadData();
      setSelectedApartamento(null);
    } catch (err) {
      setError(err.response?.data?.message || 'Erro ao realizar venda');
    }
  };

  if (!user) {
    return (
      <div className="login-container">
        <div className="login-box">
          <h1>ImovelStand</h1>
          <h2>Sistema de Vendas de Apartamentos</h2>
          {error && <div className="error">{error}</div>}
          <form onSubmit={handleLogin}>
            <input
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
            <input
              type="password"
              placeholder="Senha"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              required
            />
            <button type="submit">Entrar</button>
          </form>
          <div className="login-help">
            <p>Usuários de teste:</p>
            <p>Admin: admin@imovelstand.com / Admin@123</p>
            <p>Corretor: corretor@imovelstand.com / Corretor@123</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <header>
        <h1>ImovelStand</h1>
        <div className="user-info">
          <span>{user.nome} ({user.role})</span>
          <button onClick={handleLogout}>Sair</button>
        </div>
      </header>

      <nav>
        <button onClick={() => setActiveTab('apartamentos')} className={activeTab === 'apartamentos' ? 'active' : ''}>
          Apartamentos
        </button>
        <button onClick={() => setActiveTab('clientes')} className={activeTab === 'clientes' ? 'active' : ''}>
          Clientes
        </button>
      </nav>

      {error && <div className="error">{error}</div>}
      {success && <div className="success">{success}</div>}

      {activeTab === 'apartamentos' && (
        <div className="content">
          <h2>Apartamentos Disponíveis</h2>
          <div className="apartamentos-grid">
            {apartamentos.filter(ap => ap.status === 'Disponível').map(ap => (
              <div key={ap.id} className="apartamento-card">
                <h3>Apartamento {ap.numero}</h3>
                <p>Andar: {ap.andar}</p>
                <p>Quartos: {ap.quartos} | Banheiros: {ap.banheiros}</p>
                <p>Área: {ap.areaMetrosQuadrados}m²</p>
                <p className="preco">R$ {ap.preco.toLocaleString('pt-BR')}</p>
                <p className="descricao">{ap.descricao}</p>
                <div className="actions">
                  <button onClick={() => setSelectedApartamento(ap)}>Vender</button>
                  {clientes.length > 0 && (
                    <button onClick={() => handleCreateReserva(ap.id, clientes[0].id)}>
                      Reservar
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'clientes' && (
        <div className="content">
          <h2>Cadastrar Cliente</h2>
          <form onSubmit={handleCreateCliente} className="cliente-form">
            <input
              type="text"
              placeholder="Nome Completo"
              value={clienteForm.nome}
              onChange={(e) => setClienteForm({ ...clienteForm, nome: e.target.value })}
              required
            />
            <input
              type="text"
              placeholder="CPF"
              value={clienteForm.cpf}
              onChange={(e) => setClienteForm({ ...clienteForm, cpf: e.target.value })}
              required
            />
            <input
              type="email"
              placeholder="Email"
              value={clienteForm.email}
              onChange={(e) => setClienteForm({ ...clienteForm, email: e.target.value })}
              required
            />
            <input
              type="text"
              placeholder="Telefone"
              value={clienteForm.telefone}
              onChange={(e) => setClienteForm({ ...clienteForm, telefone: e.target.value })}
              required
            />
            <button type="submit">Cadastrar Cliente</button>
          </form>

          <h2>Clientes Cadastrados</h2>
          <div className="clientes-list">
            {clientes.map(cliente => (
              <div key={cliente.id} className="cliente-card">
                <h3>{cliente.nome}</h3>
                <p>CPF: {cliente.cpf}</p>
                <p>Email: {cliente.email}</p>
                <p>Telefone: {cliente.telefone}</p>
              </div>
            ))}
          </div>
        </div>
      )}

      {selectedApartamento && (
        <div className="modal">
          <div className="modal-content">
            <h2>Vender Apartamento {selectedApartamento.numero}</h2>
            <p>Preço: R$ {selectedApartamento.preco.toLocaleString('pt-BR')}</p>
            <form onSubmit={(e) => {
              e.preventDefault();
              const clienteId = e.target.clienteId.value;
              const valorEntrada = parseFloat(e.target.valorEntrada.value);
              handleCreateVenda(selectedApartamento.id, parseInt(clienteId), selectedApartamento.preco, valorEntrada);
            }}>
              <select name="clienteId" required>
                <option value="">Selecione o Cliente</option>
                {clientes.map(c => (
                  <option key={c.id} value={c.id}>{c.nome} - {c.cpf}</option>
                ))}
              </select>
              <input type="number" name="valorEntrada" placeholder="Valor da Entrada" step="0.01" required />
              <div className="modal-actions">
                <button type="submit">Confirmar Venda</button>
                <button type="button" onClick={() => setSelectedApartamento(null)}>Cancelar</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
