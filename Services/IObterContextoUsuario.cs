using CappyAI.Models;

namespace CappyAI.Services;

public interface IObterContextoUsuario
{
    Task<ContextoUsuario> ObterContextoAsync();
} 