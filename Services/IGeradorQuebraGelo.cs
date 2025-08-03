using CappyAI.Models;

namespace CappyAI.Services;

public interface IGeradorQuebraGelo
{
    Task<RespostaQuebraGelo> GerarIdeiasAsync(SolicitacaoQuebraGelo solicitacao);
} 