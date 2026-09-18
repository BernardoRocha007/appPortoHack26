using Microsoft.EntityFrameworkCore;
using appPortoHack.API.Data;
using appPortoHack.API.Models;

namespace appPortoHack.API.Services;

public class DuimpService(ConexaoDB db, CadAtributosService cadAtributosService)
{
    public List<Duimp> ObterTodas(string? cnpjRaiz = null)
    {
        try
        {
            var query = db.Duimps.Include(d => d.Itens).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(cnpjRaiz))
                query = query.Where(d => d.CnpjRaiz == cnpjRaiz);

            return query.OrderByDescending(d => d.DataRegistro).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO SQL] Falha ao obter DUIMPs: {ex.Message}");
            return new List<Duimp>();
        }
    }

    public Duimp? ObterPorNumero(string numero)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numero)) return null;

            return db.Duimps
                .Include(d => d.Itens)
                .AsNoTracking()
                .FirstOrDefault(d => d.Numero.Equals(numero, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO SQL] Falha ao buscar DUIMP {numero}: {ex.Message}");
            return null;
        }
    }

    // Registrar DUIMP com Simulação de Parametrização
    public (bool Sucesso, Duimp? DuimpRegistrada, List<string> Erros) RegistrarDuimp(Duimp duimp)
    {
        var erros = new List<string>();

        try
        {
            if (duimp == null || duimp.Itens == null || duimp.Itens.Count == 0)
            {
                return (false, null, new List<string> { "A DUIMP precisa ter pelo menos 1 item." });
            }

            if (string.IsNullOrWhiteSpace(duimp.CnpjRaiz))
            {
                return (false, null, new List<string> { "O CNPJ raiz do importador é obrigatório." });
            }

            bool temRiscoCanalVermelho = false;

            // Valida cada item da DUIMP contra os produtos cadastrados no bd
            foreach (var item in duimp.Itens)
            {
                var produtoNoCatalogo = db.Produtos.FirstOrDefault(p => 
                    p.Codigo == item.CodigoProduto && 
                    p.CnpjRaiz == duimp.CnpjRaiz);

                if (produtoNoCatalogo == null)
                {
                    erros.Add($"O item {item.NumeroItem} (Código '{item.CodigoProduto}') NÃO está cadastrado no Catálogo de Produtos do importador!");
                    temRiscoCanalVermelho = true;
                    continue;
                }

                // Checa se a NCM exige LPCO (Licença da Anvisa/MAPA)
                var regraNcm = cadAtributosService.ObterPorNcm(produtoNoCatalogo.Ncm);
                if (regraNcm != null && regraNcm.RequerLpco && string.IsNullOrWhiteSpace(item.NumeroLpco))
                {
                    erros.Add($"O item {item.NumeroItem} ({produtoNoCatalogo.Descricao}) exige Licença de Importação ({regraNcm.OrgaoAnuente}), mas nenhum LPCO foi vinculado.");
                    temRiscoCanalVermelho = true;
                }
            }

            // Se tiver itens que nem existem no catálogo, impede o registro
            if (erros.Count > 0 && !duimp.Itens.Any(i => db.Produtos.Any(p => p.Codigo == i.CodigoProduto && p.CnpjRaiz == duimp.CnpjRaiz)))
            {
                return (false, null, erros);
            }

            // Gera o Número Oficial da DUIMP (Ex: 26BR0001234567-0)
            duimp.Numero = $"26BR{Random.Shared.NextInt64(1000000000, 9999999999)}-0";
            duimp.DataRegistro = DateTime.Now;
            duimp.ValorTotalMercadorias = duimp.Itens.Sum(i => i.Quantidade * i.ValorUnitario);

            // Parametrização da Receita Federal
            if (temRiscoCanalVermelho)
            {
                duimp.Canal = "RISCO DE CANAL VERMELHO POR DADOS TÉCNICOS";
                duimp.Situacao = "EM_CONFERENCIA_FISICA";
            }
            else
            {
                duimp.Canal = "RISCO MÍNIMO DE CANAL VERDE POR DADOS TÉCNICOS";
                duimp.Situacao = "DESEMBARACADA";
            }

            // Grava no SQL Server!
            db.Duimps.Add(duimp);
            db.SaveChanges();

            return (true, duimp, erros);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO SQL] Falha ao registrar DUIMP: {ex.Message}");
            return (false, null, new List<string> { ex.Message });
        }
    }
}