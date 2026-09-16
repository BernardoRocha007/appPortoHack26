using System.Text.Json;
using appPortoHack.API.Models;

namespace appPortoHack.API.Services;

public class CadAtributosService
{
    private readonly List<RegraNcm> _regras = new();

    public CadAtributosService(IWebHostEnvironment env)
{
    try
    {
        var caminhoJson = Path.Combine(env.ContentRootPath, "API", "regras_ncm.json");

        if (File.Exists(caminhoJson))
        {
            var conteudo = File.ReadAllText(caminhoJson);
            var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _regras = JsonSerializer.Deserialize<List<RegraNcm>>(conteudo, opcoes) ?? new();
        }
        else
        {
            Console.WriteLine($"[AVISO] Arquivo {caminhoJson} não foi encontrado.");
        }
    }
    catch (Exception ex)
    {
        // Se o JSON estiver quebrado, não derruba o app! Apenas avisa no console
        Console.WriteLine($"[ERRO] Falha ao carregar regras_ncm.json: {ex.Message}");
    }
}

    // Retorna todas as regras que temos cadastradas
    public List<RegraNcm> ObterTodas() => _regras;

    // Busca a regra de uma NCM específica
    public RegraNcm? ObterPorNcm(string ncm)
    {
        if (string.IsNullOrWhiteSpace(ncm)) return null;

        // Limpa os pontos (ex: "3004.90.99" vira "30049099") para não falhar na busca
        var ncmLimpo = ncm.Replace(".", "").Trim();
        return _regras.FirstOrDefault(r => r.Ncm.Replace(".", "").Trim() == ncmLimpo);
    }
}