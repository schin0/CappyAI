namespace CappyAI.Models;

public record QuebraGelo(
    string Id,
    string Titulo,
    string Descricao,
    TipoQuebraGelo Tipo,
    string[] Tags,
    int NivelDificuldade,
    int TempoEstimado
);

public record ContextoUsuario(
    string Localizacao,
    string? ClimaAtual,
    int HoraAtual,
    string? DiaSemana,
    string? EstacaoAno,
    string[]? InteressesUsuario,
    string? CulturaLocal
);

public record SolicitacaoQuebraGelo(
    ContextoUsuario Contexto,
    int Quantidade,
    TipoQuebraGelo? TipoPreferido,
    int? NivelDificuldadeMaximo
);

public record RespostaQuebraGelo(
    QuebraGelo[] Ideias,
    string MensagemMotivacional,
    string ContextoUtilizado
);

public enum TipoQuebraGelo
{
    Pergunta,
    Jogo,
    Desafio,
    TemaConversa,
    AtividadeInterativa
} 