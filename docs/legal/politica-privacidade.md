---
⚠️ TEMPLATE — NÃO É CÓPIA FINAL. Precisa ser revisado por um advogado
antes de publicação. A empresa que operar o ImovelStand é a
controladora LGPD. Substitua os placeholders `[...]`.
---

# Política de Privacidade — ImovelStand

**Última atualização:** [DATA]

## 1. Quem somos

[NOME DA EMPRESA], inscrita no CNPJ [...], com sede em [...], é a controladora dos dados pessoais coletados e tratados através do software ImovelStand (o "Serviço").

Contato do encarregado de dados (DPO): [EMAIL DPO]

## 2. Quais dados coletamos

Coletamos os seguintes dados, fornecidos diretamente por você ou por nossos clientes (incorporadoras contratantes):

- **Identificação**: nome, CPF, RG, data de nascimento, estado civil
- **Contato**: email, telefone, WhatsApp, endereço
- **Profissionais/financeiros**: profissão, empresa, renda mensal declarada
- **Relacionamento**: origem do lead, interações registradas, visitas, propostas, vendas
- **Dados de sessão**: IP, user agent, tokens de acesso (para segurança)

## 3. Base legal (LGPD art. 7º)

O tratamento ocorre sob as seguintes bases:

- **Consentimento** (art. 7º, I) — marcado expressamente no cadastro
- **Execução de contrato** (art. 7º, V) — necessário para processar sua proposta/venda
- **Legítimo interesse** (art. 7º, IX) — para segurança, prevenção de fraude e análise de crédito

## 4. Com quem compartilhamos

- **Incorporadora contratante** (tenant) — dona do relacionamento comercial com você
- **Gateways de pagamento** (ex: Iugu) — se você optar por pagar via plataforma
- **Serviços técnicos essenciais**: provedor de email (SMTP), SMS/WhatsApp (Z-API), hospedagem em nuvem (Microsoft Azure)
- **Autoridades** quando requisitado por ordem judicial

Não vendemos seus dados.

## 5. Seus direitos (LGPD art. 18)

Você pode a qualquer momento:

1. **Confirmar** se tratamos seus dados
2. **Acessar** os dados que temos sobre você
3. **Corrigir** dados incompletos/incorretos
4. **Anonimizar, bloquear ou eliminar** dados desnecessários
5. **Portabilidade** — pelo endpoint `GET /api/clientes/{id}/export` você baixa seus dados em JSON estruturado
6. **Eliminação** dos dados tratados com base em consentimento
7. **Revogar consentimento**

Para exercer qualquer direito, escreva para [EMAIL DPO].

## 6. Retenção

Mantemos seus dados enquanto houver relacionamento comercial com a incorporadora contratante ou obrigação legal (ex: 5 anos após a última transação para fins fiscais, conforme Código Tributário).

Após isso, os dados são **anonimizados** — mantemos apenas informações estatísticas sem identificação pessoal.

## 7. Segurança

- Senhas armazenadas com BCrypt (workfactor 12)
- Todas as comunicações via HTTPS/TLS 1.2+
- Isolamento multi-tenant no banco de dados
- Logs auditáveis de acesso
- Backup diário automático

## 8. Cookies

Usamos cookies apenas para:
- **Essenciais**: sessão autenticada (refresh token)
- **Funcionais**: preferências de UI (modo claro/escuro no futuro)
- **Analytics** (apenas com consentimento separado): para entender uso agregado

## 9. Menores de idade

O ImovelStand não é destinado a menores de 18 anos. Se identificarmos dados de menor, eles serão removidos.

## 10. Alterações

Mudanças nesta política serão comunicadas com 30 dias de antecedência pelo email cadastrado.

## 11. Contato

- **DPO**: [NOME] — [EMAIL DPO]
- **Autoridade Nacional**: ANPD — <https://www.gov.br/anpd>
