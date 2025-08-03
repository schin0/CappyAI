using CappyAI.Domain.Entities;

namespace CappyAI.Domain.Interfaces;

public interface IObterContextoUsuario
{
    Task<ContextoUsuario> ObterContextoAsync();
} 