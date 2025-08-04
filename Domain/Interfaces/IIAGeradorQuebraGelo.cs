using CappyAI.Domain.Entities;

namespace CappyAI.Domain.Interfaces;

public interface IIAGeradorQuebraGelo
{
    Task<QuebraGelo[]> GerarIdeiasComIAAsync(ContextoUsuario contexto, int quantidade, TipoQuebraGelo? tipoPreferido = null);
} 