namespace ImovelStand.Application.Services.Prompts;

/// <summary>
/// Biblioteca de prompts versionada. Toda mudança cria nova versão, nunca
/// edita uma versão publicada — preserva reprodutibilidade e permite A/B.
///
/// Convenção de nomenclatura: Operacao + Versao (ex: BriefingClienteV1).
/// </summary>
public static class PromptLibrary
{
    /// <summary>
    /// System prompt base para todas as operações. Define persona e guardrails.
    /// </summary>
    public const string SystemBase =
        "Você é um assistente especializado em mercado imobiliário brasileiro, " +
        "focado em apoiar corretores e gerentes de incorporadoras. " +
        "Responda SEMPRE em português do Brasil. " +
        "Seja direto, objetivo e evite hedging desnecessário. " +
        "NUNCA invente dados — se não tiver informação suficiente, diga explicitamente. " +
        "NUNCA dê conselho financeiro definitivo — use verbos como 'sugere', 'indica', 'considere'.";

    // ========== Briefing de Cliente ==========

    public static string BriefingClienteSystem(string versao) => versao switch
    {
        "v1" => SystemBase + "\n\n" +
            "Sua tarefa: gerar um BRIEFING de 3-5 linhas sobre um cliente, preparando o corretor " +
            "para a próxima interação. Formato obrigatório:\n" +
            "Linha 1: nome, idade aproximada (se disponível), profissão, renda (se disponível).\n" +
            "Linha 2: histórico resumido (quantas visitas, última interação há quanto tempo, status no funil).\n" +
            "Linha 3: interesses detectados (tipologia preferida, faixa de preço, orientação).\n" +
            "Linha 4: objeções recorrentes ou preocupações mencionadas (se houver).\n" +
            "Linha 5: SUGESTÃO DE PRÓXIMA AÇÃO específica e acionável.",
        _ => throw new ArgumentException($"Versão {versao} não existe para briefing-cliente")
    };

    public static string BriefingClienteUser(string contextoCliente, string versao) => versao switch
    {
        "v1" => $"DADOS DO CLIENTE:\n{contextoCliente}\n\nGere o briefing conforme as instruções.",
        _ => throw new ArgumentException($"Versão {versao} não existe para briefing-cliente")
    };

    // ========== Próximas Ações (fila do corretor) ==========

    public static string ProximasAcoesSystem(string versao) => versao switch
    {
        "v1" => SystemBase + "\n\n" +
            "Sua tarefa: receber uma lista de clientes de um corretor e retornar uma fila " +
            "PRIORIZADA de no máximo 8 ações acionáveis para hoje. " +
            "Responda em JSON: array de objetos com {clienteId, prioridade, acao, justificativa}. " +
            "Prioridade: 'urgente' (risco de perder), 'alta', 'media'. " +
            "Acao: uma única frase imperativa (ex: 'Ligar para João — proposta expira amanhã'). " +
            "Justificativa: uma frase curta explicando por quê. " +
            "NÃO inclua clientes sem ação clara ou já contatados nas últimas 24h.",
        _ => throw new ArgumentException($"Versão {versao} não existe para proximas-acoes")
    };

    public static string ProximasAcoesUser(string listaClientes, string versao) => versao switch
    {
        "v1" => $"CORRETOR: clientes sob sua responsabilidade:\n\n{listaClientes}\n\n" +
                "Retorne apenas o JSON com a fila priorizada. Sem texto antes ou depois.",
        _ => throw new ArgumentException($"Versão {versao} não existe para proximas-acoes")
    };

    // ========== Extrator de Proposta (conversa → proposta estruturada) ==========

    public static string ExtrairPropostaSystem(string versao) => versao switch
    {
        "v1" => SystemBase + "\n\n" +
            "Sua tarefa: extrair de uma conversa entre corretor e cliente os dados de uma PROPOSTA " +
            "de compra de apartamento. Responda APENAS JSON válido, sem texto extra, seguindo o schema:\n" +
            "{\n" +
            "  \"valorOferecido\": number,\n" +
            "  \"observacoes\": string,\n" +
            "  \"condicao\": {\n" +
            "    \"valorTotal\": number,\n" +
            "    \"entrada\": number,\n" +
            "    \"sinal\": number,\n" +
            "    \"qtdParcelasMensais\": int,\n" +
            "    \"valorParcelaMensal\": number,\n" +
            "    \"qtdSemestrais\": int,\n" +
            "    \"valorSemestral\": number,\n" +
            "    \"valorChaves\": number,\n" +
            "    \"qtdPosChaves\": int,\n" +
            "    \"valorPosChaves\": number,\n" +
            "    \"indice\": \"SemReajuste\" | \"Incc\" | \"Ipca\" | \"Igpm\" | \"Tr\" | \"Selic\",\n" +
            "    \"taxaJurosAnual\": number\n" +
            "  },\n" +
            "  \"camposFaltantes\": string[]\n" +
            "}\n" +
            "Se algum valor não foi mencionado explicitamente, use 0 e adicione o nome do campo em " +
            "'camposFaltantes'. NUNCA invente valores.",
        _ => throw new ArgumentException($"Versão {versao} não existe para extrair-proposta")
    };

    public static string ExtrairPropostaUser(string conversa, decimal valorApartamento, string versao) => versao switch
    {
        "v1" => $"VALOR TABELA DO APARTAMENTO: R$ {valorApartamento:N2}\n\n" +
                $"CONVERSA:\n{conversa}\n\n" +
                "Extraia a proposta. Retorne apenas o JSON.",
        _ => throw new ArgumentException($"Versão {versao} não existe para extrair-proposta")
    };

    // ========== Analisar Objeções ==========

    public static string AnalisarObjecoesSystem(string versao) => versao switch
    {
        "v1" => SystemBase + "\n\n" +
            "Sua tarefa: analisar o histórico de interações de um cliente e identificar OBJEÇÕES " +
            "RECORRENTES (apareceram 2+ vezes) que estão bloqueando a venda. " +
            "Para cada objeção detectada, sugira um argumento de contorno baseado em boas práticas " +
            "de vendas imobiliárias. Responda JSON: " +
            "{\"objecoes\": [{\"tema\": string, \"ocorrencias\": int, \"ultimaMencao\": string, \"sugestaoContorno\": string}]}. " +
            "Temas típicos: 'preco', 'prazo-entrega', 'localizacao', 'financiamento', 'tamanho', " +
            "'valorizacao', 'confianca-construtora'. Se não há objeções claras, retorne lista vazia.",
        _ => throw new ArgumentException($"Versão {versao} não existe para analisar-objecoes")
    };

    public static string AnalisarObjecoesUser(string historico, string versao) => versao switch
    {
        "v1" => $"HISTÓRICO DE INTERAÇÕES:\n{historico}\n\nRetorne apenas o JSON.",
        _ => throw new ArgumentException($"Versão {versao} não existe para analisar-objecoes")
    };
}
