using Microsoft.AspNetCore.Mvc;
using appPortoHack.API.Services;
using appPortoHack.API.Models;

namespace appPortoHack.API.Controllers;

[ApiController]
[Route("produtos")]

public class ProdutoController(ProdutoService service) : ControllerBase
{
    [HttpGet]
    public IActionResult ObterTodos([FromQuery] string? cnpjRaiz = null)
    {
        try
        {
            var produtos = service.ObterTodos(cnpjRaiz);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                mensagem = "Erro interno no servidor ao obter produtos.",
                detalhe = ex.Message 
            });
        }
    }

    [HttpGet("{codigo}")]
    public IActionResult ObterPorCodigo(string codigo, [FromQuery] string cnpjRaiz)
    {
        try
        {
            var produto = service.ObterPorCodigo(codigo, cnpjRaiz);
            if (produto == null)
                return NotFound(new { mensagem = $"Produto com código {codigo} e CNPJ {cnpjRaiz} não encontrado." });

            return Ok(produto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                mensagem = "Erro interno no servidor ao buscar produto.",
                detalhe = ex.Message 
            });
        }
    }

    [HttpPost]
    public IActionResult ValidarECadastrar([FromBody] Produto produto)
    {
        try
        {
            var (sucesso, mensagem, erros) = service.ValidarECadastrar(produto);

            if (!sucesso)
                return BadRequest(new { mensagem, erros });

            return Ok(new { mensagem });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                mensagem = "Erro interno no servidor ao validar e cadastrar produto.",
                detalhe = ex.Message 
            });
        }
    }
}