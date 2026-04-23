/**
 * Widget embedavel do Simulador Financeiro ImovelStand.
 *
 * Isola estilos via Shadow DOM. Chama POST /api/publico/simular no backend
 * do tenant (configurado via data-api no <script>) e captura lead opcional.
 *
 * Zero dependencias externas (vanilla TS). Bundle final ~15-25 KB gzipped.
 */

import { STYLES } from './styles';

interface WidgetConfig {
  tenantSlug: string;
  apiUrl: string;
  containerId: string;
  primaryColor: string;
  empreendimentoPrecoTabela?: number;
}

interface SimulacaoResultado {
  sfh: { primeiraParcela: number; ultimaParcela: number; jurosTotais: number; taxaAnualPct: number };
  impostos: { itbi: number; cartorio: number; total: number; uf: string };
  capacidade?: { parcelaMaxima30Pct: number; alerta?: string };
  aluguelVsCompra?: { recomendacao: string; diferencaAbsoluta: number; patrimonioFinalComprar: number; patrimonioFinalAlugar: number };
  parcelaCabe?: boolean | null;
  resumoExecutivo: string;
}

interface ApiResponse {
  resultado: SimulacaoResultado;
  leadCapturado: boolean;
  leadId?: number;
}

function brl(n: number): string {
  return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}

function brlFull(n: number): string {
  return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function getScriptConfig(): WidgetConfig | null {
  const script = document.currentScript as HTMLScriptElement | null;
  if (!script) return null;
  const tenantSlug = script.dataset.tenant;
  const apiUrl = script.dataset.api;
  if (!tenantSlug || !apiUrl) {
    console.error('[ImovelStand] data-tenant e data-api sao obrigatorios.');
    return null;
  }
  return {
    tenantSlug,
    apiUrl: apiUrl.replace(/\/$/, ''),
    containerId: script.dataset.container ?? 'imovelstand-simulador',
    primaryColor: script.dataset.color ?? '#6366f1',
    empreendimentoPrecoTabela: script.dataset.preco ? Number(script.dataset.preco) : undefined
  };
}

function criarContainer(config: WidgetConfig): ShadowRoot | null {
  const target = document.getElementById(config.containerId);
  if (!target) {
    console.error(`[ImovelStand] container #${config.containerId} nao encontrado.`);
    return null;
  }
  target.innerHTML = '';
  const shadow = target.attachShadow({ mode: 'open' });
  const style = document.createElement('style');
  style.textContent = STYLES(config.primaryColor);
  shadow.appendChild(style);
  return shadow;
}

async function simular(config: WidgetConfig, dados: {
  valorImovel: number;
  entrada: number;
  rendaMensal: number;
  uf: string;
  aluguelAtual?: number;
  nome?: string;
  email?: string;
  telefone?: string;
  consentimento?: boolean;
}): Promise<ApiResponse | null> {
  try {
    const body = {
      simulacao: {
        valorImovel: dados.valorImovel,
        entrada: dados.entrada,
        rendaMensal: dados.rendaMensal,
        uf: dados.uf,
        aluguelAtual: dados.aluguelAtual ?? 0,
        qtdParcelasDireto: 0
      },
      tenantSlug: config.tenantSlug,
      leadNome: dados.nome,
      leadEmail: dados.email,
      leadTelefone: dados.telefone,
      consentimentoLgpd: dados.consentimento ?? false
    };
    const r = await fetch(`${config.apiUrl}/api/publico/simular`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    });
    if (!r.ok) return null;
    return (await r.json()) as ApiResponse;
  } catch (e) {
    console.error('[ImovelStand] erro na simulacao:', e);
    return null;
  }
}

function render(shadow: ShadowRoot, config: WidgetConfig) {
  const html = `
    <div class="is-widget">
      <div class="is-header">
        <h2>Simulador de Financiamento</h2>
        <p>Descubra em segundos se o imóvel cabe no seu orçamento.</p>
      </div>

      <form id="simForm" class="is-form">
        <div class="is-field">
          <label>Valor do imóvel</label>
          <input type="number" id="valorImovel" value="${config.empreendimentoPrecoTabela ?? ''}" placeholder="500000" required />
        </div>
        <div class="is-row">
          <div class="is-field">
            <label>Entrada (R$)</label>
            <input type="number" id="entrada" value="" placeholder="100000" required />
          </div>
          <div class="is-field">
            <label>Renda mensal</label>
            <input type="number" id="renda" value="" placeholder="12000" required />
          </div>
        </div>
        <div class="is-row">
          <div class="is-field">
            <label>UF</label>
            <select id="uf">
              <option value="SP">SP</option>
              <option value="RJ">RJ</option>
              <option value="MG">MG</option>
              <option value="PR">PR</option>
              <option value="RS">RS</option>
              <option value="BA">BA</option>
              <option value="SC">SC</option>
              <option value="PE">PE</option>
              <option value="DF">DF</option>
              <option value="GO">GO</option>
              <option value="CE">CE</option>
            </select>
          </div>
          <div class="is-field">
            <label>Aluguel atual (opcional)</label>
            <input type="number" id="aluguel" value="" placeholder="2500" />
          </div>
        </div>
        <button type="submit" class="is-submit" id="btnSim">Simular</button>
      </form>

      <div id="simResult" class="is-result" style="display:none"></div>
    </div>
  `;
  shadow.innerHTML += html;

  const form = shadow.getElementById('simForm') as HTMLFormElement;
  const result = shadow.getElementById('simResult') as HTMLDivElement;
  const btn = shadow.getElementById('btnSim') as HTMLButtonElement;

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    btn.disabled = true;
    btn.textContent = 'Calculando...';

    const valorImovel = Number((shadow.getElementById('valorImovel') as HTMLInputElement).value);
    const entrada = Number((shadow.getElementById('entrada') as HTMLInputElement).value);
    const rendaMensal = Number((shadow.getElementById('renda') as HTMLInputElement).value);
    const aluguelAtual = Number((shadow.getElementById('aluguel') as HTMLInputElement).value) || 0;
    const uf = (shadow.getElementById('uf') as HTMLSelectElement).value;

    const resp = await simular(config, { valorImovel, entrada, rendaMensal, uf, aluguelAtual });

    btn.disabled = false;
    btn.textContent = 'Simular novamente';

    if (!resp) {
      result.style.display = 'block';
      result.innerHTML = `<div class="is-alert is-alert-error">Erro ao calcular. Tente novamente.</div>`;
      return;
    }

    renderResultado(result, resp.resultado, config);
  });
}

function renderResultado(container: HTMLDivElement, r: SimulacaoResultado, config: WidgetConfig) {
  const cabeClass = r.parcelaCabe === true ? 'is-success' : r.parcelaCabe === false ? 'is-warning' : 'is-info';
  const cabeTexto = r.parcelaCabe === true ? 'Cabe no orçamento' : r.parcelaCabe === false ? 'Ultrapassa 30% da renda' : '';

  container.style.display = 'block';
  container.innerHTML = `
    <div class="is-alert ${cabeClass}">
      ${r.resumoExecutivo}
      ${cabeTexto ? `<strong style="display:block;margin-top:6px;">${cabeTexto}</strong>` : ''}
    </div>

    <div class="is-cards">
      <div class="is-card">
        <div class="is-card-label">Primeira parcela (SFH)</div>
        <div class="is-card-value">${brlFull(r.sfh.primeiraParcela)}</div>
        <div class="is-card-hint">Taxa ${r.sfh.taxaAnualPct}% a.a. — Caixa SAC</div>
      </div>
      <div class="is-card">
        <div class="is-card-label">Impostos de compra</div>
        <div class="is-card-value">${brlFull(r.impostos.total)}</div>
        <div class="is-card-hint">ITBI ${brl(r.impostos.itbi)} + cartório ${brl(r.impostos.cartorio)}</div>
      </div>
      ${r.capacidade ? `
      <div class="is-card">
        <div class="is-card-label">Parcela máxima (regra 30%)</div>
        <div class="is-card-value">${brlFull(r.capacidade.parcelaMaxima30Pct)}</div>
        ${r.capacidade.alerta ? `<div class="is-card-hint is-warning-text">${r.capacidade.alerta}</div>` : ''}
      </div>` : ''}
    </div>

    ${r.aluguelVsCompra ? `
      <div class="is-comparison">
        <h3>Aluguel vs Compra em 30 anos</h3>
        <div class="is-comparison-grid">
          <div>
            <strong>Comprar</strong>
            <span>${brlFull(r.aluguelVsCompra.patrimonioFinalComprar)}</span>
            <small>patrimônio final</small>
          </div>
          <div>
            <strong>Alugar + investir</strong>
            <span>${brlFull(r.aluguelVsCompra.patrimonioFinalAlugar)}</span>
            <small>patrimônio final</small>
          </div>
        </div>
        <p class="is-reco">${r.aluguelVsCompra.recomendacao}</p>
      </div>
    ` : ''}

    <div class="is-lead-section">
      <h3>Quer receber uma proposta personalizada?</h3>
      <form id="leadForm" class="is-lead-form">
        <input type="text" id="leadNome" placeholder="Seu nome completo" required />
        <input type="email" id="leadEmail" placeholder="Seu e-mail" required />
        <input type="tel" id="leadTelefone" placeholder="WhatsApp (com DDD)" required />
        <label class="is-check">
          <input type="checkbox" id="leadConsent" required />
          <span>Autorizo o contato e o tratamento dos dados pessoais conforme LGPD.</span>
        </label>
        <button type="submit" class="is-submit" id="btnLead">Receber proposta no WhatsApp</button>
        <div id="leadMsg" class="is-lead-msg"></div>
      </form>
    </div>
  `;

  const leadForm = container.querySelector('#leadForm') as HTMLFormElement;
  const leadMsg = container.querySelector('#leadMsg') as HTMLDivElement;
  const btnLead = container.querySelector('#btnLead') as HTMLButtonElement;

  leadForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const nome = (container.querySelector('#leadNome') as HTMLInputElement).value;
    const email = (container.querySelector('#leadEmail') as HTMLInputElement).value;
    const telefone = (container.querySelector('#leadTelefone') as HTMLInputElement).value;
    const consentimento = (container.querySelector('#leadConsent') as HTMLInputElement).checked;

    if (!consentimento) {
      leadMsg.textContent = 'Por favor, marque a autorização LGPD.';
      return;
    }

    btnLead.disabled = true;
    btnLead.textContent = 'Enviando...';

    // Pega os valores originais do form de simulação (primeiro form)
    const shadow = container.getRootNode() as ShadowRoot;
    const valorImovelVal = Number((shadow.getElementById('valorImovel') as HTMLInputElement).value);
    const entradaVal = Number((shadow.getElementById('entrada') as HTMLInputElement).value);
    const rendaVal = Number((shadow.getElementById('renda') as HTMLInputElement).value);
    const ufVal = (shadow.getElementById('uf') as HTMLSelectElement).value;

    const resp = await simular(config, {
      valorImovel: valorImovelVal,
      entrada: entradaVal,
      rendaMensal: rendaVal,
      uf: ufVal,
      nome, email, telefone,
      consentimento: true
    });

    btnLead.disabled = false;
    btnLead.textContent = 'Receber proposta no WhatsApp';

    if (resp?.leadCapturado) {
      leadMsg.innerHTML = '<span class="is-success-text">Recebemos seu contato! Um corretor retornará em breve.</span>';
      leadForm.reset();
    } else {
      leadMsg.innerHTML = '<span class="is-warning-text">Nao foi possivel registrar agora. Tente novamente mais tarde.</span>';
    }
  });
}

// ========== Entry point ==========

function init() {
  const config = getScriptConfig();
  if (!config) return;
  const shadow = criarContainer(config);
  if (!shadow) return;
  render(shadow, config);
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', init);
} else {
  init();
}

// Expose para integracoes avancadas
(window as unknown as { ImovelStandSimulador: { init: typeof init } }).ImovelStandSimulador = { init };
