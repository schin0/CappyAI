using CappyAI.Models;

namespace CappyAI.Services;

public class GeradorQuebraGelo : IGeradorQuebraGelo
{
    private readonly QuebraGelo[] _ideiasBase;
    private readonly Random _random;

    public GeradorQuebraGelo()
    {
        _random = new Random();
        _ideiasBase = InicializarIdeiasBase();
    }

    public async Task<RespostaQuebraGelo> GerarIdeiasAsync(SolicitacaoQuebraGelo solicitacao)
    {
        var ideiasFiltradas = FiltrarIdeias(solicitacao);
        var ideiasSelecionadas = SelecionarIdeias(ideiasFiltradas, solicitacao.Quantidade);
        var mensagemMotivacional = GerarMensagemMotivacional(solicitacao.Contexto);
        var contextoUtilizado = GerarContextoUtilizado(solicitacao.Contexto);

        return new RespostaQuebraGelo(ideiasSelecionadas, mensagemMotivacional, contextoUtilizado);
    }

    private QuebraGelo[] FiltrarIdeias(SolicitacaoQuebraGelo solicitacao)
    {
        var ideiasComPontuacao = _ideiasBase
            .Where(ideia => VerificarTipoPreferido(ideia, solicitacao.TipoPreferido) &&
                           VerificarNivelDificuldade(ideia, solicitacao.NivelDificuldadeMaximo))
            .Select(ideia => new { Ideia = ideia, Pontuacao = CalcularPontuacao(ideia, solicitacao.Contexto) })
            .Where(item => item.Pontuacao > 0)
            .OrderByDescending(item => item.Pontuacao)
            .Select(item => item.Ideia)
            .ToArray();

        return ideiasComPontuacao;
    }

    private int CalcularPontuacao(QuebraGelo ideia, ContextoUsuario contexto)
    {
        var pontuacao = 0;

        // Pontuação base para todas as ideias
        pontuacao += 1;

        // Pontuação por interesses (mais importante)
        if (contexto.InteressesUsuario?.Any() == true)
        {
            var matchesInteresses = ideia.Tags.Count(tag => 
                contexto.InteressesUsuario!.Any(interesse => 
                    tag.Contains(interesse, StringComparison.OrdinalIgnoreCase)));
            pontuacao += matchesInteresses * 3;
        }

        // Pontuação por cultura local
        if (!string.IsNullOrEmpty(contexto.CulturaLocal))
        {
            if (ideia.Tags.Any(tag => tag.Contains(contexto.CulturaLocal!, StringComparison.OrdinalIgnoreCase)))
                pontuacao += 2;
        }

        // Pontuação por clima
        if (!string.IsNullOrEmpty(contexto.ClimaAtual))
        {
            if (ideia.Tags.Any(tag => tag.Contains(contexto.ClimaAtual!, StringComparison.OrdinalIgnoreCase)))
                pontuacao += 2;
        }

        // Pontuação por horário
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

    private bool VerificarTipoPreferido(QuebraGelo ideia, TipoQuebraGelo? tipoPreferido)
    {
        if (tipoPreferido == null) return true;
        return ideia.Tipo == tipoPreferido;
    }

    private bool VerificarNivelDificuldade(QuebraGelo ideia, int? nivelMaximo)
    {
        if (nivelMaximo == null) return true;
        return ideia.NivelDificuldade <= nivelMaximo;
    }

    private QuebraGelo[] SelecionarIdeias(QuebraGelo[] ideias, int quantidade)
    {
        if (ideias.Length <= quantidade) return ideias;
        
        return ideias.OrderBy(x => _random.Next()).Take(quantidade).ToArray();
    }

    private string GerarMensagemMotivacional(ContextoUsuario contexto)
    {
        var mensagens = new[]
        {
            "Que tal começar uma conversa incrível? Essas ideias vão te ajudar a conectar de verdade!",
            "Momentos especiais começam com uma simples pergunta. Use essas ideias para criar conexões autênticas!",
            "Desconecte do virtual e conecte-se ao real! Essas ideias são seu passaporte para conversas memoráveis.",
            "A magia acontece quando pessoas reais se encontram. Deixe essas ideias guiarem suas conversas!",
            "Cada 'oi' pode ser o início de uma amizade incrível. Use essas ideias para quebrar o gelo!"
        };

        return mensagens[_random.Next(mensagens.Length)];
    }

    private string GerarContextoUtilizado(ContextoUsuario contexto)
    {
        var elementos = new List<string>();
        
        if (!string.IsNullOrEmpty(contexto.ClimaAtual))
            elementos.Add($"clima: {contexto.ClimaAtual}");
        
        if (!string.IsNullOrEmpty(contexto.DiaSemana))
            elementos.Add($"dia: {contexto.DiaSemana}");
        
        if (!string.IsNullOrEmpty(contexto.CulturaLocal))
            elementos.Add($"cultura: {contexto.CulturaLocal}");
        
        if (contexto.InteressesUsuario?.Any() == true)
            elementos.Add($"interesses: {string.Join(", ", contexto.InteressesUsuario)}");

        return elementos.Any() 
            ? $"Contexto: {string.Join(" | ", elementos)}"
            : "Contexto: geral";
    }

    private QuebraGelo[] InicializarIdeiasBase()
    {
        return new[]
        {
            // Ideias universais
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
            
            // Ideias baseadas no clima
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
            
            // Ideias baseadas no horário
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
            
            // Ideias culturais regionais
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
            
            // Ideias específicas para São Paulo
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
            
            // Ideias para cariocas
            new QuebraGelo(
                "17", "Praia Carioca",
                "Qual é sua praia preferida no Rio e o que você mais gosta dela?",
                TipoQuebraGelo.TemaConversa,
                new[] { "carioca", "praia", "rio" },
                1, 3
            ),
            
            // Ideias para mineiros
            new QuebraGelo(
                "18", "Café Mineiro",
                "Qual é seu café da manhã mineiro preferido?",
                TipoQuebraGelo.TemaConversa,
                new[] { "mineira", "café", "comida" },
                1, 3
            ),
            
            // Jogos e atividades
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
            
            // Ideias baseadas em interesses
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