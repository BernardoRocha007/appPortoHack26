using Microsoft.AspNetCore.Mvc;
using appPortoHack.API.Services;

namespace appPortoHack.API.Controllers;

[ApiController]
[Route("cadatributos/api/ext")]
// O serviço entra direto aqui no cabeçalho da classe!
public class CadAtributosController(CadAtributosService service) : ControllerBase
{
    [HttpGet("ncm/{ncm}/atributos")]
    public IActionResult ObterAtributos(string ncm)
    {
        try
        {
        var regra = service.ObterPorNcm(ncm);
        
        if (regra == null)
        {
            return NotFound(new { mensagem = $"NCM {ncm} não encontrada." });
        }
        return Ok(regra);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                mensagem = "Erro interno no servidor ao consultar a NCM.",
                detalhe = ex.Message 
            });
        }
}
}