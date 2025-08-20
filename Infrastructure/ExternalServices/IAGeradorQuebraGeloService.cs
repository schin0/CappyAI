using CappyAI.Domain.Entities;
using CappyAI.Domain.Interfaces;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace CappyAI.Infrastructure.ExternalServices;

public class IAGeradorQuebraGeloService : IIAGeradorQuebraGelo
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IAGeradorQuebraGeloService> _logger;
    private readonly ConcurrentDictionary<string, (QuebraGelo[] Resultado, DateTime ExpiraEm)> _cache;
    private readonly TimeSpan _tempoExpiracaoCache = TimeSpan.FromMinutes(30);

    public IAGeradorQuebraGeloService(HttpClient httpClient, IConfiguration configuration, ILogger<IAGeradorQuebraGeloService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _cache = new ConcurrentDictionary<string, (QuebraGelo[], DateTime)>();
    }

    public async Task<QuebraGelo[]> GerarIdeiasComIAAsync(ContextoUsuario contexto, int quantidade, TipoQuebraGelo? tipoPreferido = null)
    {
        try
        {
            var chaveCache = GerarChaveCache(contexto, quantidade, tipoPreferido);
            
            if (ObterDoCache(chaveCache, out var resultadoCache))
            {
                _logger.LogInformation("Resultado obtido do cache para chave: {Chave}", chaveCache);
                return resultadoCache;
            }

            var prompt = GerarPrompt(contexto, quantidade, tipoPreferido);
            var resposta = await EnviarParaIAAsync(prompt);
            var resultado = ProcessarRespostaIA(resposta, quantidade);
            
            if (resultado.Length > 0)
            {
                SalvarNoCache(chaveCache, resultado);
                _logger.LogInformation("Resultado salvo no cache para chave: {Chave}", chaveCache);
            }
            
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar ideias com IA");
            return Array.Empty<QuebraGelo>();
        }
    }

    private string GerarPrompt(ContextoUsuario contexto, int quantidade, TipoQuebraGelo? tipoPreferido)
    {
        var tipoInstrucao = tipoPreferido.HasValue 
            ? $"Tipo preferido: {tipoPreferido.Value}. Gere apenas ideias deste tipo."
            : "Gere ideias variadas (Pergunta, Jogo, Desafio, TemaConversa, AtividadeInterativa).";

        const string template = @"Você é um gerador de JSON. Gere {0} ideias para quebrar o gelo. Contexto: {1}, {2}, {3}h, {4}, {5}, {6}{7}. {8}

REGRAS OBRIGATÓRIAS:
- Responda EXCLUSIVAMENTE com JSON válido
- NÃO adicione texto, explicações ou comentários
- NÃO use markdown ou formatação
- Apenas o array JSON

[{{
  ""id"": ""1"",
  ""titulo"": ""Título da ideia"",
  ""descricao"": ""Descrição detalhada"",
  ""tipo"": ""{9}"",
  ""tags"": [""tag1"", ""tag2""],
  ""nivelDificuldade"": 1,
  ""tempoEstimado"": 3
}}]";

        var interesses = contexto.InteressesUsuario?.Any() == true
            ? $", interesses: {string.Join(", ", contexto.InteressesUsuario)}"
            : string.Empty;

        var tipoExemplo = tipoPreferido?.ToString() ?? "Pergunta";

        return string.Format(template,
            quantidade,
            contexto.Localizacao,
            contexto.ClimaAtual ?? "clima não informado",
            contexto.HoraAtual,
            contexto.DiaSemana ?? "dia não informado",
            contexto.EstacaoAno ?? "estação não informada",
            contexto.CulturaLocal ?? "cultura não informada",
            interesses,
            tipoInstrucao,
            tipoExemplo);
    }

    private string GerarChaveCache(ContextoUsuario contexto, int quantidade, TipoQuebraGelo? tipoPreferido)
    {
        var interesses = contexto.InteressesUsuario?.Any() == true
            ? string.Join(",", contexto.InteressesUsuario.OrderBy(x => x))
            : string.Empty;

        return $"{quantidade}|{contexto.Localizacao}|{contexto.ClimaAtual ?? "null"}|{contexto.HoraAtual}|{contexto.DiaSemana ?? "null"}|{contexto.EstacaoAno ?? "null"}|{contexto.CulturaLocal ?? "null"}|{interesses}|{tipoPreferido?.ToString() ?? "null"}";
    }

    private bool ObterDoCache(string chave, out QuebraGelo[] resultado)
    {
        if (_cache.TryGetValue(chave, out var itemCache))
        {
            if (DateTime.UtcNow < itemCache.ExpiraEm)
            {
                resultado = itemCache.Resultado;
                return true;
            }
            
            _cache.TryRemove(chave, out _);
        }
        
        resultado = Array.Empty<QuebraGelo>();
        return false;
    }

    private void SalvarNoCache(string chave, QuebraGelo[] resultado)
    {
        var expiraEm = DateTime.UtcNow.Add(_tempoExpiracaoCache);
        _cache.AddOrUpdate(chave, (resultado, expiraEm), (_, _) => (resultado, expiraEm));
        LimparCacheExpirado();
    }

    private void LimparCacheExpirado()
    {
        var agora = DateTime.UtcNow;
        var chavesExpiradas = _cache
            .Where(kvp => kvp.Value.ExpiraEm < agora)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var chave in chavesExpiradas)
        {
            _cache.TryRemove(chave, out _);
        }

        if (chavesExpiradas.Count > 0)
        {
            _logger.LogDebug("Removidas {Quantidade} entradas expiradas do cache", chavesExpiradas.Count);
        }
    }

    private async Task<string> EnviarParaIAAsync(string prompt)
    {
        var baseUrl = _configuration["IA:Url"];
        var modelo = _configuration["IA:Modelo"];
        var apiKey = _configuration["IA:ApiKey"];

        var requestUrl = $"{baseUrl}{modelo}:generateContent";

        var request = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        },
            generationConfig = new
            {
                temperature = 0.3,
                topP = 0.8,
                topK = 40
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Enviando requisição para a IA: {Url}", requestUrl);

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = content
            };
            httpRequest.Headers.Add("x-goog-api-key", apiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Resposta bruta da IA: {ResponseContent}", responseContent);

            using var document = JsonDocument.Parse(responseContent);
            var responseField = document.RootElement
                                        .GetProperty("candidates")[0]
                                        .GetProperty("content")
                                        .GetProperty("parts")[0]
                                        .GetProperty("text")
                                        .GetString();

            if (string.IsNullOrEmpty(responseField))
            {
                _logger.LogError("Campo 'text' não encontrado ou vazio na resposta da IA");
                return string.Empty;
            }

            _logger.LogInformation("Resposta processada: {Response}", responseField);
            return responseField;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro na requisição para a API da IA. StatusCode: {StatusCode}", ex.StatusCode);
            return string.Empty;
        }
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
        if (string.IsNullOrWhiteSpace(resposta))
        {
            _logger.LogWarning("Resposta da IA está vazia");
            return string.Empty;
        }

        var jsonStart = resposta.IndexOf('[');
        var jsonEnd = resposta.LastIndexOf(']');

        if (jsonStart == -1 || jsonEnd == -1 || jsonEnd <= jsonStart)
        {
            _logger.LogWarning("JSON não encontrado na resposta da IA. Resposta: {Resposta}", resposta);
            return string.Empty;
        }

        var json = resposta.Substring(jsonStart, jsonEnd - jsonStart + 1);
        json = ProcessarCaracteresEscape(json);

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("JSON extraído não é um array válido: {Json}", json);
                return string.Empty;
            }
            
            _logger.LogInformation("JSON válido extraído com sucesso");
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("JSON extraído não é válido: {Json}. Erro: {Erro}", json, ex.Message);
            
            var jsonCorrigido = TentarCorrigirJSON(json);
            if (!string.IsNullOrEmpty(jsonCorrigido))
            {
                try
                {
                    using var document = JsonDocument.Parse(jsonCorrigido);
                    if (document.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        _logger.LogInformation("JSON corrigido é válido");
                        return jsonCorrigido;
                    }
                }
                catch (Exception exCorrigido)
                {
                    _logger.LogWarning("JSON corrigido também não é válido: {Erro}", exCorrigido.Message);
                }
            }
            
            var jsonLimpo = LimparJSON(json);
            if (!string.IsNullOrEmpty(jsonLimpo))
            {
                try
                {
                    using var document = JsonDocument.Parse(jsonLimpo);
                    if (document.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        _logger.LogInformation("JSON limpo é válido");
                        return jsonLimpo;
                    }
                }
                catch
                {
                    _logger.LogWarning("JSON limpo também não é válido");
                }
            }
            
            return string.Empty;
        }
    }

    private string TentarCorrigirJSON(string json)
    {
        try
        {
            var linhas = json.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var linhasCorrigidas = new List<string>();
            var nivelChaves = 0;
            var nivelColchetes = 0;
            var dentroDeString = false;
            var escape = false;

            foreach (var linha in linhas)
            {
                var linhaTrim = linha.Trim();
                if (string.IsNullOrEmpty(linhaTrim)) continue;

                var linhaCorrigida = linhaTrim;
                
                foreach (var caractere in linhaTrim)
                {
                    if (escape)
                    {
                        escape = false;
                        continue;
                    }

                    if (caractere == '\\')
                    {
                        escape = true;
                        continue;
                    }

                    if (caractere == '"' && !escape)
                    {
                        dentroDeString = !dentroDeString;
                        continue;
                    }

                    if (!dentroDeString)
                    {
                        if (caractere == '{') nivelChaves++;
                        if (caractere == '}') nivelChaves--;
                        if (caractere == '[') nivelColchetes++;
                        if (caractere == ']') nivelColchetes--;
                    }
                }

                linhasCorrigidas.Add(linhaCorrigida);
            }

            var jsonCorrigido = string.Join("\n", linhasCorrigidas);

            if (nivelChaves > 0)
            {
                jsonCorrigido += "\n" + new string('}', nivelChaves);
            }

            if (nivelColchetes > 0)
            {
                jsonCorrigido += "\n" + new string(']', nivelColchetes);
            }

            if (nivelChaves < 0)
            {
                var chavesFaltando = Math.Abs(nivelChaves);
                jsonCorrigido = new string('{', chavesFaltando) + jsonCorrigido;
            }

            if (nivelColchetes < 0)
            {
                var colchetesFaltando = Math.Abs(nivelColchetes);
                jsonCorrigido = new string('[', colchetesFaltando) + jsonCorrigido;
            }

            _logger.LogInformation("JSON corrigido: {JsonCorrigido}", jsonCorrigido);
            return jsonCorrigido;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Erro ao tentar corrigir JSON: {Erro}", ex.Message);
            return string.Empty;
        }
    }

    private string LimparJSON(string json)
    {
        var jsonLimpo = json
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\t", " ")
            .Trim();

        var linhas = jsonLimpo.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var linhasLimpas = new List<string>();

        foreach (var linha in linhas)
        {
            var linhaLimpa = linha.Trim();
            if (!string.IsNullOrEmpty(linhaLimpa))
            {
                linhasLimpas.Add(linhaLimpa);
            }
        }

        return string.Join("", linhasLimpas);
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