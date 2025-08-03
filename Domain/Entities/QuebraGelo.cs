namespace CappyAI.Domain.Entities;

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

public enum TipoQuebraGelo
{
    Pergunta,
    Jogo,
    Desafio,
    TemaConversa,
    AtividadeInterativa
} 