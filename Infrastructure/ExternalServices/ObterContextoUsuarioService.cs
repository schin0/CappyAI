using CappyAI.Domain.Entities;
using CappyAI.Domain.Interfaces;

namespace CappyAI.Infrastructure.ExternalServices;

public class ObterContextoUsuarioService : IObterContextoUsuario
{
    public Task<ContextoUsuario> ObterContextoAsync()
    {
        var agora = DateTime.Now;
        var diaSemana = agora.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"));
        var estacao = ObterEstacaoAno(agora.Month);
        
        var contexto = new ContextoUsuario(
            Localizacao: "São Paulo, SP",
            ClimaAtual: "Ensolarado",
            HoraAtual: agora.Hour,
            DiaSemana: diaSemana,
            EstacaoAno: estacao,
            InteressesUsuario: new[] { "tecnologia", "música", "viagem" },
            CulturaLocal: "paulista"
        );

        return Task.FromResult(contexto);
    }

    private string ObterEstacaoAno(int mes)
    {
        return mes switch
        {
            12 or 1 or 2 => "Verão",
            3 or 4 or 5 => "Outono",
            6 or 7 or 8 => "Inverno",
            9 or 10 or 11 => "Primavera",
            _ => "Desconhecida"
        };
    }
} 