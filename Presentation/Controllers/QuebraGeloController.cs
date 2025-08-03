using CappyAI.Application.DTOs;
using CappyAI.Application.UseCases;
using CappyAI.Domain.Entities;
using CappyAI.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CappyAI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuebraGeloController : ControllerBase
{
    private readonly GerarIdeiasQuebraGelo _gerarIdeiasQuebraGelo;
    private readonly IObterContextoUsuario _obterContextoUsuario;

    public QuebraGeloController(
        GerarIdeiasQuebraGelo gerarIdeiasQuebraGelo,
        IObterContextoUsuario obterContextoUsuario)
    {
        _gerarIdeiasQuebraGelo = gerarIdeiasQuebraGelo;
        _obterContextoUsuario = obterContextoUsuario;
    }

    [HttpPost("gerar")]
    public async Task<ActionResult<RespostaQuebraGelo>> GerarIdeias([FromBody] SolicitacaoQuebraGelo solicitacao)
    {
        if (solicitacao.Quantidade <= 0 || solicitacao.Quantidade > 10)
        {
            return BadRequest("A quantidade deve estar entre 1 e 10");
        }

        var resposta = await _gerarIdeiasQuebraGelo.ExecutarAsync(solicitacao);
        return Ok(resposta);
    }

    [HttpPost("gerar-automatico")]
    public async Task<ActionResult<RespostaQuebraGelo>> GerarIdeiasAutomatico([FromBody] SolicitacaoAutomatica solicitacao)
    {
        if (solicitacao.Quantidade <= 0 || solicitacao.Quantidade > 10)
        {
            return BadRequest("A quantidade deve estar entre 1 e 10");
        }

        var contextoUsuario = await _obterContextoUsuario.ObterContextoAsync();
        var solicitacaoCompleta = new SolicitacaoQuebraGelo(
            contextoUsuario,
            solicitacao.Quantidade,
            solicitacao.TipoPreferido,
            solicitacao.NivelDificuldadeMaximo
        );

        var resposta = await _gerarIdeiasQuebraGelo.ExecutarAsync(solicitacaoCompleta);
        return Ok(resposta);
    }

    [HttpGet("tipos")]
    public ActionResult<TipoQuebraGelo[]> ObterTipos()
    {
        return Ok(Enum.GetValues<TipoQuebraGelo>());
    }

    [HttpGet("contexto-atual")]
    public async Task<ActionResult<ContextoUsuario>> ObterContextoAtual()
    {
        var contexto = await _obterContextoUsuario.ObterContextoAsync();
        return Ok(contexto);
    }

    [HttpGet("exemplo")]
    public ActionResult ObterExemplo()
    {
        var solicitacaoExemplo = new SolicitacaoAutomatica(
            Quantidade: 3,
            TipoPreferido: null,
            NivelDificuldadeMaximo: 2
        );

        return Ok(new
        {
            SolicitacaoExemplo = solicitacaoExemplo,
            Descricao = "Use este exemplo para gerar ideias automaticamente baseadas no contexto atual"
        });
    }
} 