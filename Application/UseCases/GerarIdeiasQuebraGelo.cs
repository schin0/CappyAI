using CappyAI.Application.DTOs;
using CappyAI.Domain.Entities;
using CappyAI.Domain.Interfaces;

namespace CappyAI.Application.UseCases;

public class GerarIdeiasQuebraGelo
{
    private readonly IGeradorQuebraGelo _geradorQuebraGelo;
    private readonly Random _random;

    public GerarIdeiasQuebraGelo(IGeradorQuebraGelo geradorQuebraGelo)
    {
        _geradorQuebraGelo = geradorQuebraGelo;
        _random = new Random();
    }

    public async Task<RespostaQuebraGelo> ExecutarAsync(SolicitacaoQuebraGelo solicitacao)
    {
        var ideiasGeradas = await _geradorQuebraGelo.GerarIdeiasAsync(solicitacao.Contexto, solicitacao.Quantidade);
        
        var ideiasSelecionadas = SelecionarIdeias(ideiasGeradas, solicitacao);
        var mensagemMotivacional = GerarMensagemMotivacional();
        var contextoUtilizado = GerarContextoUtilizado(solicitacao.Contexto);

        return new RespostaQuebraGelo(ideiasSelecionadas, mensagemMotivacional, contextoUtilizado);
    }

    private QuebraGelo[] SelecionarIdeias(QuebraGelo[] ideias, SolicitacaoQuebraGelo solicitacao)
    {
        var ideiasFiltradas = ideias
            .Where(ideia => VerificarTipoPreferido(ideia, solicitacao.TipoPreferido))
            .Where(ideia => VerificarNivelDificuldade(ideia, solicitacao.NivelDificuldadeMaximo))
            .ToArray();

        if (ideiasFiltradas.Length <= solicitacao.Quantidade) 
            return ideiasFiltradas;
        
        return ideiasFiltradas.OrderBy(x => _random.Next()).Take(solicitacao.Quantidade).ToArray();
    }

    private bool VerificarTipoPreferido(QuebraGelo ideia, TipoQuebraGelo? tipoPreferido)
    {
        if (tipoPreferido == null) return true;
        return ideia.Tipo == tipoPreferido;
    }

    private bool VerificarNivelDificuldade(QuebraGelo ideia, int? nivelMaximo)
    {
        if (nivelMaximo == null) return true;
        return ideia.NivelDificuldade <= nivelMaximo;
    }

    private string GerarMensagemMotivacional()
    {
        var mensagens = new[]
        {
            "Que tal começar uma conversa incrível? Essas ideias vão te ajudar a conectar de verdade!",
            "Momentos especiais começam com uma simples pergunta. Use essas ideias para criar conexões autênticas!",
            "Desconecte do virtual e conecte-se ao real! Essas ideias são seu passaporte para conversas memoráveis.",
            "A magia acontece quando pessoas reais se encontram. Deixe essas ideias guiarem suas conversas!",
            "Cada 'oi' pode ser o início de uma amizade incrível. Use essas ideias para quebrar o gelo!"
        };

        return mensagens[_random.Next(mensagens.Length)];
    }

    private string GerarContextoUtilizado(ContextoUsuario contexto)
    {
        var elementos = new List<string>();
        
        if (!string.IsNullOrEmpty(contexto.ClimaAtual))
            elementos.Add($"clima: {contexto.ClimaAtual}");
        
        if (!string.IsNullOrEmpty(contexto.DiaSemana))
            elementos.Add($"dia: {contexto.DiaSemana}");
        
        if (!string.IsNullOrEmpty(contexto.CulturaLocal))
            elementos.Add($"cultura: {contexto.CulturaLocal}");
        
        if (contexto.InteressesUsuario?.Any() == true)
            elementos.Add($"interesses: {string.Join(", ", contexto.InteressesUsuario)}");

        return elementos.Any() 
            ? $"Contexto: {string.Join(" | ", elementos)}"
            : "Contexto: geral";
    }
} 