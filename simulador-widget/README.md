# ImovelStand Simulador — Widget Embedável

Widget standalone para embedar o Simulador Financeiro em sites de
lançamento. Zero dependências externas (React, jQuery etc).

## Integração no site da incorporadora

```html
<!-- Container onde o widget vai renderizar -->
<div id="imovelstand-simulador"></div>

<!-- Loader -->
<script
  src="https://cdn.imovelstand.com.br/widget/widget.js"
  data-tenant="construtora-xyz"
  data-api="https://api.imovelstand.com.br"
  data-color="#6366f1"
  data-preco="550000"
></script>
```

### Atributos

| Atributo | Obrigatório | Descrição |
|---|---|---|
| `data-tenant` | ✅ | Slug do tenant (incorporadora) para roteamento do lead |
| `data-api` | ✅ | URL base da API ImovelStand |
| `data-color` | — | Cor primária (hex). Default: `#6366f1` |
| `data-container` | — | ID do container. Default: `imovelstand-simulador` |
| `data-preco` | — | Pré-preenche o campo "Valor do imóvel" (útil por lançamento) |

### Isolamento

- Estilos renderizam em Shadow DOM — não vazam nem sofrem interferência do CSS da página hospedeira
- Não pollui `window` exceto por `window.ImovelStandSimulador.init()`

## Desenvolvimento

```bash
cd simulador-widget
npm install
npm run build        # gera dist/widget.js
npm run dev          # modo dev (preview)
```

Para testar localmente:
1. Rode a API em `http://localhost:5000`
2. `npm run build` no widget
3. Abra `samples/embed.html` no navegador

## Captura de lead

Quando o usuário preenche nome+email+telefone e aceita LGPD, o widget
chama `POST /api/publico/simular` com os dados. O backend cria um
`Cliente` novo com `OrigemLead=Site` + primeira `HistoricoInteracao`
documentando os parâmetros da simulação.

O corretor vê o lead novo no CRM com contexto rico para retorno.

## Rate limit

O endpoint público tem limite de 20 req/min por IP. Em pico de tráfego,
o widget exibe mensagem de erro amigável.
