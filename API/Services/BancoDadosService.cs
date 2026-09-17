using System.Text.Json;
using appPortoHack.API.Models;

namespace appPortoHack.API.Services;

public class BancoDadosService
{
    private readonly string _caminhoBanco;
    private static readonly JsonSerializerOptions _opcoesJson = new() 
    { 
        PropertyNameCaseInsensitive = true, 
        WriteIndented = true 
    };

    public BancoDadosService(IWebHostEnvironment env)
    {
        _caminhoBanco = Path.Combine(env.ContentRootPath, "API", "BancoDados.json");

        if (!File.Exists(_caminhoBanco))
        {
            Salvar(new BancoDados());
        }
    }

    // Lê em tempo real direto do arquivo JSON
    public BancoDados ObterDados()
    {
        try
        {
            if (File.Exists(_caminhoBanco))
            {
                var json = File.ReadAllText(_caminhoBanco);
                return JsonSerializer.Deserialize<BancoDados>(json, _opcoesJson) ?? new BancoDados();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao ler BancoDados.json: {ex.Message}");
        }

        return new BancoDados();
    }

    // Salva recebendo os dados atualizados
    public void Salvar(BancoDados? dados = null)
    {
        try
        {
            var dadosParaSalvar = dados ?? ObterDados();
            var json = JsonSerializer.Serialize(dadosParaSalvar, _opcoesJson);
            File.WriteAllText(_caminhoBanco, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao salvar no arquivo BancoDados.json: {ex.Message}");
        }
    }
}