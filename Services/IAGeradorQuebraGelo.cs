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
        sb.AppendLine("IMPORTANTE: Responda APENAS com o JSON, sem texto adicional, explicações ou comentários.");
        sb.AppendLine("IMPORTANTE: Use APENAS PORTUGUÊS BRASILEIRO em todos os campos.");
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
        sb.AppendLine($"Gere {quantidade} ideias criativas e contextualizadas em PORTUGUÊS BRASILEIRO.");
        sb.AppendLine("Cada ideia deve ter:");
        sb.AppendLine("- Um título curto e atrativo em português");
        sb.AppendLine("- Uma descrição clara e específica em português");
        sb.AppendLine("- Um tipo (Pergunta, Jogo, Desafio, TemaConversa, AtividadeInterativa)");
        sb.AppendLine("- Tags relevantes em português");
        sb.AppendLine("- Nível de dificuldade (1=fácil, 2=médio, 3=difícil)");
        sb.AppendLine("- Tempo estimado em minutos");
        sb.AppendLine();
        sb.AppendLine("RESPONDA APENAS COM O JSON EM PORTUGUÊS:");
        sb.AppendLine("[");
        sb.AppendLine("  {");
        sb.AppendLine("    \"id\": \"1\",");
        sb.AppendLine("    \"titulo\": \"Título da Ideia em Português\",");
        sb.AppendLine("    \"descricao\": \"Descrição detalhada em português\",");
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
                temperature = 0.3,
                top_p = 0.8,
                max_tokens = 800,
                top_k = 40,
                repeat_penalty = 1.1
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Enviando requisição para IA: {Url}", url);
        
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Resposta bruta da IA: {ResponseContent}", responseContent);
        
        using var document = JsonDocument.Parse(responseContent);
        var responseField = document.RootElement.GetProperty("response").GetString();
        
        if (string.IsNullOrEmpty(responseField))
        {
            _logger.LogError("Campo 'response' não encontrado ou vazio");
            return string.Empty;
        }

        _logger.LogInformation("Resposta processada: {Response}", responseField);
        return responseField;
    }

    private QuebraGelo[] ProcessarRespostaIA(string resposta, int quantidade)
    {
        try
        {
            _logger.LogInformation("Processando resposta da IA: {Resposta}", resposta);
            
            var json = ExtrairJSONDaResposta(resposta);
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning("JSON extraído está vazio");
                return Array.Empty<QuebraGelo>();
            }

            _logger.LogInformation("JSON extraído: {Json}", json);
            
            using var document = JsonDocument.Parse(json);
            var ideias = new List<QuebraGelo>();
            
            foreach (var element in document.RootElement.EnumerateArray())
            {
                var ideia = ConverterParaQuebraGelo(element);
                ideias.Add(ideia);
            }
            
            _logger.LogInformation("Ideias deserializadas: {Quantidade}", ideias.Count);
            
            var resultado = ideias.Take(quantidade).ToArray();
            _logger.LogInformation("Ideias convertidas: {Quantidade}", resultado.Length);
            
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar resposta da IA: {Resposta}", resposta);
            return Array.Empty<QuebraGelo>();
        }
    }

    private string ExtrairJSONDaResposta(string resposta)
    {
        var jsonStart = resposta.IndexOf('[');
        var jsonEnd = resposta.LastIndexOf(']');
        
        if (jsonStart == -1 || jsonEnd == -1 || jsonEnd <= jsonStart)
        {
            _logger.LogWarning("JSON não encontrado na resposta da IA");
            return string.Empty;
        }

        var json = resposta.Substring(jsonStart, jsonEnd - jsonStart + 1);
        
        json = ProcessarCaracteresEscape(json);
        
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("JSON extraído não é um array válido");
                return string.Empty;
            }
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("JSON extraído não é válido: {Json}. Erro: {Erro}", json, ex.Message);
            return string.Empty;
        }
    }

    private string ProcessarCaracteresEscape(string json)
    {
        return json
            .Replace("\\r\\n", "\n")
            .Replace("\\n", "\n")
            .Replace("\\t", "\t")
            .Replace("\\r", "\r")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    private QuebraGelo ConverterParaQuebraGelo(JsonElement element)
    {
        var id = element.GetProperty("id").GetString() ?? "";
        var titulo = element.GetProperty("titulo").GetString() ?? "";
        var descricao = element.GetProperty("descricao").GetString() ?? "";
        var tipo = element.GetProperty("tipo").GetString() ?? "Pergunta";
        var nivelDificuldade = element.GetProperty("nivelDificuldade").GetInt32();
        var tempoEstimado = element.GetProperty("tempoEstimado").GetInt32();
        
        var tags = new List<string>();
        if (element.TryGetProperty("tags", out var tagsElement))
        {
            foreach (var tag in tagsElement.EnumerateArray())
            {
                tags.Add(tag.GetString() ?? "");
            }
        }
        
        var tipoEnum = Enum.TryParse<TipoQuebraGelo>(tipo, true, out var tipoResult) 
            ? tipoResult 
            : TipoQuebraGelo.Pergunta;

        return new QuebraGelo(
            id,
            titulo,
            descricao,
            tipoEnum,
            tags.ToArray(),
            nivelDificuldade,
            tempoEstimado
        );
    }
} 