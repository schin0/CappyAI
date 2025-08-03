using CappyAI.Domain.Entities;

namespace CappyAI.Application.DTOs;

public record SolicitacaoQuebraGelo(
    ContextoUsuario Contexto,
    int Quantidade,
    TipoQuebraGelo? TipoPreferido,
    int? NivelDificuldadeMaximo
);

public record SolicitacaoAutomatica(
    int Quantidade,
    TipoQuebraGelo? TipoPreferido,
    int? NivelDificuldadeMaximo
); 