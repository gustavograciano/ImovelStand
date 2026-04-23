# Gerador de PDF — Apresentação Comercial

Script Node.js que captura screenshots reais do sistema rodando via Puppeteer e monta um PDF A4 profissional pronto para apresentação.

## Pré-requisitos

- Node.js 20+
- API ImovelStand rodando em `http://localhost:5082`
- Vite rodando em `http://localhost:5173`
- Usuário `admin@imovelstand.com` / `Admin@123` existente no banco

## Uso

```bash
cd docs/pdf-generator
npm install
npm run gerar
```

## O que o script faz

1. **Login** via API para obter access token
2. **Popula dados demo** para que as screenshots fiquem ricas:
   - Gera 48 sugestões de Precificação Dinâmica para o tenant
   - Cria e autoriza análise de crédito Open Finance do cliente João da Silva
3. **Captura screenshots** das 12 telas principais via Puppeteer (viewport 1400×900, retina 2x)
4. **Monta HTML** com layout profissional (cover dark gradient, sumário, cards, tabelas, pricing)
5. **Renderiza PDF** A4 com cabeçalho/rodapé e numeração de páginas

## Saída

- `output/ImovelStand-Apresentacao.pdf` — PDF final (~2.5 MB, 22 páginas)
- `output/apresentacao.html` — HTML fonte (debug)
- `output/screenshots/*.png` — screenshots individuais (gitignored)

## Telas capturadas

| # | Tela | Rota |
|---|---|---|
| 01 | Login | /login |
| 02 | Home + Fila IA | / |
| 03 | Dashboard | /dashboard |
| 04 | Empreendimentos CRUD | /empreendimentos |
| 05 | Apartamentos lista | /apartamentos |
| 06 | Clientes Kanban | /clientes |
| 07 | Cliente detail + Open Finance | /clientes/1 |
| 08 | Simulador | /simulador |
| 09 | Propostas | /propostas |
| 10 | Vendas | /vendas |
| 11 | Precificação IA | /precificacao |
| 12 | Usuários | /usuarios |
