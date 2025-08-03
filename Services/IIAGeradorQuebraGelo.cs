using CappyAI.Models;

namespace CappyAI.Services;

public interface IIAGeradorQuebraGelo
{
    Task<QuebraGelo[]> GerarIdeiasComIAAsync(ContextoUsuario contexto, int quantidade);
} 