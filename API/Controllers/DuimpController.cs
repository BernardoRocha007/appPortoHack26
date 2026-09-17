using Microsoft.AspNetCore.Mvc;
using appPortoHack.API.Models;
using appPortoHack.API.Services;

namespace appPortoHack.API.Controllers;

[ApiController]
[Route("duimp")]
public class DuimpController(DuimpService duimpService) : ControllerBase
{
    [HttpGet]
    public IActionResult ObterTodas([FromQuery] string? cnpjRaiz)
    {
        var duimps = duimpService.ObterTodas(cnpjRaiz);
        return Ok(duimps);
    }

    [HttpGet("{numero}")]
    public IActionResult ObterPorNumero(string numero)
    {
        var duimp = duimpService.ObterPorNumero(numero);
        if (duimp == null)
            return NotFound(new { mensagem = $"DUIMP {numero} não encontrada." });

        return Ok(duimp);
    }

    [HttpGet("{numero}/situacao")]
    public IActionResult ObterSituacao(string numero)
    {
        var duimp = duimpService.ObterPorNumero(numero);
        if (duimp == null)
            return NotFound(new { mensagem = $"DUIMP {numero} não encontrada." });

        return Ok(new
        {
            numero = duimp.Numero,
            situacao = duimp.Situacao,
            canal = duimp.Canal,
            dataRegistro = duimp.DataRegistro
        });
    }

    [HttpPost]
    public IActionResult Registrar([FromBody] Duimp duimp)
    {
        var (sucesso, duimpRegistrada, erros) = duimpService.RegistrarDuimp(duimp);

        if (!sucesso)
        {
            return BadRequest(new
            {
                mensagem = "Não foi possível registrar a DUIMP devido a pendências cadastrais.",
                erros
            });
        }

        return CreatedAtAction(
            nameof(ObterPorNumero),
            new { numero = duimpRegistrada!.Numero },
            new
            {
                mensagem = "DUIMP registrada com sucesso!",
                duimp = duimpRegistrada,
                alertas = erros // se houver alertas não impeditivos
            }
        );
    }
}