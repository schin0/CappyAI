using CappyAI.Models;

namespace CappyAI.Services;

public class ObterContextoUsuario : IObterContextoUsuario
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ObterContextoUsuario(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ContextoUsuario> ObterContextoAsync()
    {
        var horaAtual = DateTime.Now.Hour;
        var diaSemana = ObterDiaSemana();
        var estacaoAno = ObterEstacaoAno();
        
        // Em uma implementação real, você obteria esses dados do usuário logado
        var localizacao = "São Paulo, SP";
        var interessesUsuario = new[] { "tecnologia", "música", "viagem" };
        var culturaLocal = ObterCulturaLocal(localizacao);
        
        var climaAtual = await ObterClimaAtualAsync(localizacao);

        return new ContextoUsuario(
            localizacao,
            climaAtual,
            horaAtual,
            diaSemana,
            estacaoAno,
            interessesUsuario,
            culturaLocal
        );
    }

    private string ObterDiaSemana()
    {
        return DateTime.Now.DayOfWeek switch
        {
            DayOfWeek.Monday => "segunda",
            DayOfWeek.Tuesday => "terça",
            DayOfWeek.Wednesday => "quarta",
            DayOfWeek.Thursday => "quinta",
            DayOfWeek.Friday => "sexta",
            DayOfWeek.Saturday => "sábado",
            DayOfWeek.Sunday => "domingo",
            _ => "desconhecido"
        };
    }

    private string ObterEstacaoAno()
    {
        var mes = DateTime.Now.Month;
        return mes switch
        {
            12 or 1 or 2 => "verão",
            3 or 4 or 5 => "outono",
            6 or 7 or 8 => "inverno",
            9 or 10 or 11 => "primavera",
            _ => "desconhecido"
        };
    }

    private string ObterCulturaLocal(string localizacao)
    {
        return localizacao.ToLower() switch
        {
            var loc when loc.Contains("são paulo") => "paulista",
            var loc when loc.Contains("rio de janeiro") => "carioca",
            var loc when loc.Contains("minas gerais") => "mineira",
            var loc when loc.Contains("bahia") => "baiana",
            var loc when loc.Contains("pernambuco") => "pernambucana",
            var loc when loc.Contains("ceará") => "cearense",
            var loc when loc.Contains("paraná") => "paranaense",
            var loc when loc.Contains("santa catarina") => "catarinense",
            var loc when loc.Contains("rio grande do sul") => "gaúcha",
            _ => "brasileira"
        };
    }

    private async Task<string?> ObterClimaAtualAsync(string localizacao)
    {
        try
        {
            // Em uma implementação real, você faria uma chamada para uma API de clima
            // Por exemplo: OpenWeatherMap, WeatherAPI, etc.
            // Por enquanto, retornamos um valor simulado baseado na estação
            
            var estacao = ObterEstacaoAno();
            return estacao switch
            {
                "verão" => "ensolarado",
                "outono" => "nublado",
                "inverno" => "frio",
                "primavera" => "ameno",
                _ => "moderado"
            };
        }
        catch
        {
            return null;
        }
    }
} 