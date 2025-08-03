using CappyAI.Domain.Entities;

namespace CappyAI.Domain.Interfaces;

public interface IGeradorQuebraGelo
{
    Task<QuebraGelo[]> GerarIdeiasAsync(ContextoUsuario contexto, int quantidade);
} 