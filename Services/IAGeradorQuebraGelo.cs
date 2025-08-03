using CappyAI.Models;
using System.Text;
using System.Text.Json;

namespace CappyAI.Services;

public class IAGeradorQuebraGelo : IIAGeradorQuebraGelo
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IAGeradorQuebraGelo> _logger;

    public IAGeradorQuebraGelo(HttpClient httpClient, IConfiguration configuration, ILogger<IAGeradorQuebraGelo> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<QuebraGelo[]> GerarIdeiasComIAAsync(ContextoUsuario contexto, int quantidade)
    {
        try
        {
            var prompt = GerarPrompt(contexto, quantidade);
            var resposta = await EnviarParaIAAsync(prompt);
            return ProcessarRespostaIA(resposta, quantidade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar ideias com IA");
            return Array.Empty<QuebraGelo>();
        }
    }

    private string GerarPrompt(ContextoUsuario contexto, int quantidade)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("Você é um assistente especializado em gerar ideias criativas para quebrar o gelo em conversas.");
        sb.AppendLine("Gere ideias baseadas no contexto fornecido. Cada ideia deve ter:");
        sb.AppendLine("- Um título curto e atrativo");
        sb.AppendLine("- Uma descrição clara e específica");
        sb.AppendLine("- Um tipo (Pergunta, Jogo, Desafio, TemaConversa, AtividadeInterativa)");
        sb.AppendLine("- Tags relevantes separadas por vírgula");
        sb.AppendLine("- Nível de dificuldade (1=fácil, 2=médio, 3=difícil)");
        sb.AppendLine("- Tempo estimado em minutos");
        sb.AppendLine();
        sb.AppendLine("Contexto atual:");
        sb.AppendLine($"- Localização: {contexto.Localizacao}");
        sb.AppendLine($"- Clima: {contexto.ClimaAtual ?? "não informado"}");
        sb.AppendLine($"- Horário: {contexto.HoraAtual}h");
        sb.AppendLine($"- Dia: {contexto.DiaSemana ?? "não informado"}");
        sb.AppendLine($"- Estação: {contexto.EstacaoAno ?? "não informada"}");
        sb.AppendLine($"- Cultura: {contexto.CulturaLocal ?? "não informada"}");
        
        if (contexto.InteressesUsuario?.Any() == true)
        {
            sb.AppendLine($"- Interesses: {string.Join(", ", contexto.InteressesUsuario)}");
        }
        
        sb.AppendLine();
        sb.AppendLine($"Gere {quantidade} ideias criativas e contextualizadas.");
        sb.AppendLine("Responda apenas com um JSON válido no formato:");
        sb.AppendLine("[");
        sb.AppendLine("  {");
        sb.AppendLine("    \"id\": \"1\",");
        sb.AppendLine("    \"titulo\": \"Título da Ideia\",");
        sb.AppendLine("    \"descricao\": \"Descrição detalhada\",");
        sb.AppendLine("    \"tipo\": \"Pergunta\",");
        sb.AppendLine("    \"tags\": [\"tag1\", \"tag2\"],");
        sb.AppendLine("    \"nivelDificuldade\": 1,");
        sb.AppendLine("    \"tempoEstimado\": 3");
        sb.AppendLine("  }");
        sb.AppendLine("]");

        return sb.ToString();
    }

    private async Task<string> EnviarParaIAAsync(string prompt)
    {
        var url = _configuration["IA:Url"] ?? "http://localhost:11434/api/generate";
        var modelo = _configuration["IA:Modelo"] ?? "llama2";
        
        var request = new
        {
            model = modelo,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = 0.8,
                top_p = 0.9,
                max_tokens = 1000
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<OllamaResponse>(responseContent);

        return responseObj?.Response ?? string.Empty;
    }

    private QuebraGelo[] ProcessarRespostaIA(string resposta, int quantidade)
    {
        try
        {
            // Limpar a resposta para extrair apenas o JSON
            var jsonStart = resposta.IndexOf('[');
            var jsonEnd = resposta.LastIndexOf(']') + 1;
            
            if (jsonStart == -1 || jsonEnd == 0) return Array.Empty<QuebraGelo>();
            
            var json = resposta.Substring(jsonStart, jsonEnd - jsonStart);
            var ideias = JsonSerializer.Deserialize<QuebraGeloIA[]>(json);

            if (ideias == null) return Array.Empty<QuebraGelo>();

            return ideias.Take(quantidade).Select(ConverterParaQuebraGelo).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar resposta da IA");
            return Array.Empty<QuebraGelo>();
        }
    }

    private QuebraGelo ConverterParaQuebraGelo(QuebraGeloIA ideiaIA)
    {
        var tipo = Enum.TryParse<TipoQuebraGelo>(ideiaIA.Tipo, true, out var tipoEnum) 
            ? tipoEnum 
            : TipoQuebraGelo.Pergunta;

        return new QuebraGelo(
            ideiaIA.Id,
            ideiaIA.Titulo,
            ideiaIA.Descricao,
            tipo,
            ideiaIA.Tags ?? Array.Empty<string>(),
            ideiaIA.NivelDificuldade,
            ideiaIA.TempoEstimado
        );
    }
}

public class OllamaResponse
{
    public string Response { get; set; } = string.Empty;
}

public class QuebraGeloIA
{
    public string Id { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string[]? Tags { get; set; }
    public int NivelDificuldade { get; set; }
    public int TempoEstimado { get; set; }
} 