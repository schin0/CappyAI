using CappyAI.Domain.Entities;
using CappyAI.Domain.Interfaces;

namespace CappyAI.Infrastructure.Repositories;

public class GeradorQuebraGeloRepository : IGeradorQuebraGelo
{
    private readonly QuebraGelo[] _ideiasBase;
    private readonly IIAGeradorQuebraGelo _iaGerador;

    public GeradorQuebraGeloRepository(IIAGeradorQuebraGelo iaGerador)
    {
        _iaGerador = iaGerador;
        _ideiasBase = InicializarIdeiasBase();
    }

    public async Task<QuebraGelo[]> GerarIdeiasAsync(ContextoUsuario contexto, int quantidade, TipoQuebraGelo? tipoPreferido = null)
    {
        var ideiasGeradas = await TentarGerarComIA(contexto, quantidade, tipoPreferido);
        
        if (!ideiasGeradas.Any())
        {
            ideiasGeradas = GerarIdeiasPreDefinidas(contexto, quantidade, tipoPreferido);
        }

        return ideiasGeradas;
    }

    private async Task<QuebraGelo[]> TentarGerarComIA(ContextoUsuario contexto, int quantidade, TipoQuebraGelo? tipoPreferido)
    {
        try
        {
            return await _iaGerador.GerarIdeiasComIAAsync(contexto, quantidade, tipoPreferido);
        }
        catch
        {
            return Array.Empty<QuebraGelo>();
        }
    }

    private QuebraGelo[] GerarIdeiasPreDefinidas(ContextoUsuario contexto, int quantidade, TipoQuebraGelo? tipoPreferido)
    {
        var ideiasComPontuacao = _ideiasBase
            .Where(ideia => tipoPreferido == null || ideia.Tipo == tipoPreferido)
            .Select(ideia => new { Ideia = ideia, Pontuacao = CalcularPontuacao(ideia, contexto) })
            .Where(item => item.Pontuacao > 0)
            .OrderByDescending(item => item.Pontuacao)
            .Select(item => item.Ideia)
            .Take(quantidade)
            .ToArray();

        return ideiasComPontuacao;
    }

    private int CalcularPontuacao(QuebraGelo ideia, ContextoUsuario contexto)
    {
        var pontuacao = 0;

        pontuacao += 1;

        if (contexto.InteressesUsuario?.Any() == true)
        {
            var matchesInteresses = ideia.Tags.Count(tag => 
                contexto.InteressesUsuario!.Any(interesse => 
                    tag.Contains(interesse, StringComparison.OrdinalIgnoreCase)));
            pontuacao += matchesInteresses * 3;
        }

        if (!string.IsNullOrEmpty(contexto.CulturaLocal))
        {
            if (ideia.Tags.Any(tag => tag.Contains(contexto.CulturaLocal!, StringComparison.OrdinalIgnoreCase)))
                pontuacao += 2;
        }

        if (!string.IsNullOrEmpty(contexto.ClimaAtual))
        {
            if (ideia.Tags.Any(tag => tag.Contains(contexto.ClimaAtual!, StringComparison.OrdinalIgnoreCase)))
                pontuacao += 2;
        }

        var periodo = contexto.HoraAtual switch
        {
            >= 6 and < 12 => "manhã",
            >= 12 and < 18 => "tarde",
            >= 18 and < 22 => "noite",
            _ => "madrugada"
        };

        if (ideia.Tags.Any(tag => tag.Contains(periodo, StringComparison.OrdinalIgnoreCase)))
            pontuacao += 2;

        return pontuacao;
    }

    private QuebraGelo[] InicializarIdeiasBase()
    {
        return new[]
        {
            new QuebraGelo(
                "1", "Se fosse um animal...",
                "Se você fosse um animal, qual seria e por quê?",
                TipoQuebraGelo.Pergunta,
                new[] { "criativo", "divertido", "universal" },
                1, 2
            ),
            new QuebraGelo(
                "2", "Superpoder Inútil",
                "Se você pudesse ter um superpoder inútil, qual seria?",
                TipoQuebraGelo.Pergunta,
                new[] { "criativo", "divertido", "imaginativo" },
                1, 3
            ),
            new QuebraGelo(
                "3", "Clima e Conversa",
                "Como o clima de hoje afeta seu humor?",
                TipoQuebraGelo.TemaConversa,
                new[] { "clima", "humor", "pessoal" },
                1, 3
            ),
            new QuebraGelo(
                "4", "Atividade ao Ar Livre",
                "Qual seria a atividade perfeita para o clima de hoje?",
                TipoQuebraGelo.Pergunta,
                new[] { "clima", "atividade", "ar livre" },
                1, 2
            ),
            new QuebraGelo(
                "5", "Clima Frio",
                "Qual é sua comida preferida para dias frios?",
                TipoQuebraGelo.TemaConversa,
                new[] { "clima", "frio", "comida", "conforto" },
                1, 3
            ),
            new QuebraGelo(
                "6", "Domingo Frio",
                "Como você gosta de passar um domingo frio?",
                TipoQuebraGelo.TemaConversa,
                new[] { "clima", "frio", "domingo", "relaxamento" },
                1, 3
            ),
            new QuebraGelo(
                "7", "Energia da Manhã",
                "Como você gosta de começar suas manhãs?",
                TipoQuebraGelo.TemaConversa,
                new[] { "manhã", "rotina", "energia" },
                1, 3
            ),
            new QuebraGelo(
                "8", "Ritual da Tarde",
                "Qual é seu ritual preferido para recarregar as energias à tarde?",
                TipoQuebraGelo.TemaConversa,
                new[] { "tarde", "ritual", "energia" },
                1, 3
            ),
            new QuebraGelo(
                "9", "Tarde de Domingo",
                "O que você gosta de fazer nas tardes de domingo?",
                TipoQuebraGelo.TemaConversa,
                new[] { "tarde", "domingo", "relaxamento" },
                1, 3
            ),
            new QuebraGelo(
                "10", "Plano da Noite",
                "Se você pudesse fazer qualquer coisa esta noite, o que seria?",
                TipoQuebraGelo.Pergunta,
                new[] { "noite", "plano", "diversão" },
                1, 2
            ),
            new QuebraGelo(
                "11", "Sotaque Local",
                "Qual é a expressão mais típica da sua região?",
                TipoQuebraGelo.TemaConversa,
                new[] { "cultura", "regional", "expressão" },
                1, 3
            ),
            new QuebraGelo(
                "12", "Comida Regional",
                "Qual é o prato típico da sua região que você mais gosta?",
                TipoQuebraGelo.TemaConversa,
                new[] { "cultura", "comida", "regional" },
                1, 3
            ),
            new QuebraGelo(
                "13", "Metrô Paulistano",
                "Qual foi sua experiência mais interessante no metrô de São Paulo?",
                TipoQuebraGelo.TemaConversa,
                new[] { "paulista", "metrô", "transporte" },
                2, 4
            ),
            new QuebraGelo(
                "14", "Bairro Favorito",
                "Qual é seu bairro favorito em São Paulo e por quê?",
                TipoQuebraGelo.Pergunta,
                new[] { "paulista", "bairro", "cidade" },
                1, 3
            ),
            new QuebraGelo(
                "15", "São Paulo no Inverno",
                "O que você mais gosta de fazer em São Paulo durante o inverno?",
                TipoQuebraGelo.TemaConversa,
                new[] { "paulista", "inverno", "cidade", "clima" },
                1, 3
            ),
            new QuebraGelo(
                "16", "Café Paulistano",
                "Qual é seu lugar preferido para tomar café em São Paulo?",
                TipoQuebraGelo.TemaConversa,
                new[] { "paulista", "café", "cidade", "gastronomia" },
                1, 3
            ),
            new QuebraGelo(
                "17", "Praia Carioca",
                "Qual é sua praia preferida no Rio e o que você mais gosta dela?",
                TipoQuebraGelo.TemaConversa,
                new[] { "carioca", "praia", "rio" },
                1, 3
            ),
            new QuebraGelo(
                "18", "Café Mineiro",
                "Qual é seu café da manhã mineiro preferido?",
                TipoQuebraGelo.TemaConversa,
                new[] { "mineira", "café", "comida" },
                1, 3
            ),
            new QuebraGelo(
                "19", "Verdade ou Desafio",
                "Proponha um jogo de verdade ou desafio com uma pergunta leve e divertida",
                TipoQuebraGelo.Jogo,
                new[] { "jogo", "interativo", "divertido" },
                2, 5
            ),
            new QuebraGelo(
                "20", "Duas Verdades e Uma Mentira",
                "Conte três fatos sobre você - dois verdadeiros e um falso. Deixe os outros adivinharem!",
                TipoQuebraGelo.Jogo,
                new[] { "jogo", "conhecimento", "divertido" },
                2, 8
            ),
            new QuebraGelo(
                "21", "Desafio do Elogio",
                "Faça um elogio sincero para a pessoa ao seu lado",
                TipoQuebraGelo.Desafio,
                new[] { "positivo", "conexão", "gentileza" },
                1, 2
            ),
            new QuebraGelo(
                "22", "Desafio da Conexão",
                "Encontre algo em comum com a pessoa ao seu lado em 30 segundos",
                TipoQuebraGelo.Desafio,
                new[] { "conexão", "rápido", "interativo" },
                2, 1
            ),
            new QuebraGelo(
                "23", "Música da Vida",
                "Se sua vida fosse um filme, qual música tocaria na trilha sonora?",
                TipoQuebraGelo.Pergunta,
                new[] { "música", "pessoal", "criativo" },
                2, 4
            ),
            new QuebraGelo(
                "24", "Tecnologia Offline",
                "Qual foi a última vez que você ficou offline por um dia inteiro?",
                TipoQuebraGelo.TemaConversa,
                new[] { "tecnologia", "reflexão", "atual" },
                2, 4
            ),
            new QuebraGelo(
                "25", "Viagem dos Sonhos",
                "Qual é o lugar mais inesperado onde você já esteve?",
                TipoQuebraGelo.TemaConversa,
                new[] { "viagem", "aventura", "pessoal" },
                2, 4
            )
        };
    }
} 