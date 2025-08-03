using CappyAI.Models;
using CappyAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CappyAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuebraGeloController : ControllerBase
{
    private readonly IGeradorQuebraGelo _geradorQuebraGelo;
    private readonly IObterContextoUsuario _obterContextoUsuario;

    public QuebraGeloController(
        IGeradorQuebraGelo geradorQuebraGelo,
        IObterContextoUsuario obterContextoUsuario)
    {
        _geradorQuebraGelo = geradorQuebraGelo;
        _obterContextoUsuario = obterContextoUsuario;
    }

    [HttpPost("gerar")]
    public async Task<ActionResult<RespostaQuebraGelo>> GerarIdeias([FromBody] SolicitacaoQuebraGelo solicitacao)
    {
        if (solicitacao.Quantidade <= 0 || solicitacao.Quantidade > 10)
        {
            return BadRequest("A quantidade deve estar entre 1 e 10");
        }

        var resposta = await _geradorQuebraGelo.GerarIdeiasAsync(solicitacao);
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

        var resposta = await _geradorQuebraGelo.GerarIdeiasAsync(solicitacaoCompleta);
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

public record SolicitacaoAutomatica(
    int Quantidade,
    TipoQuebraGelo? TipoPreferido,
    int? NivelDificuldadeMaximo
); 