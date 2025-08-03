using CappyAI.Domain.Entities;

namespace CappyAI.Application.DTOs;

public record RespostaQuebraGelo(
    QuebraGelo[] Ideias,
    string MensagemMotivacional,
    string ContextoUtilizado
); 