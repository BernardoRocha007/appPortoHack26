using ClosedXML.Excel;
using appPortoHack.API.Models;

namespace appPortoHack.API.Services;

public class PlanilhaService
{
    // Método principal: detecta se é CSV ou XLSX pelo nome do arquivo
    public List<Produto> LerPlanilha(Stream stream, string nomeArquivo, string cnpjRaizPadrao)
    {
        if (nomeArquivo.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return LerPlanilhaXlsx(stream, cnpjRaizPadrao);
        }
        else
        {
            using var reader = new StreamReader(stream);
            var conteudoCsv = reader.ReadToEnd();
            return LerPlanilhaCsv(conteudoCsv, cnpjRaizPadrao);
        }
    }

    // 1. LEITOR DE EXCEL (.xlsx)
    public List<Produto> LerPlanilhaXlsx(Stream stream, string cnpjRaizPadrao)
    {
        var produtos = new List<Produto>();

        try
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1); // Pega a primeira aba da planilha
            var rows = worksheet.RangeUsed()?.RowsUsed().ToList();

            if (rows.Count <= 1) return produtos;

            // Lê o cabeçalho (Linha 1)
            var cabecalho = rows[0].Cells().Select(c => c.GetString().Trim().ToUpperInvariant()).ToList();

            // Percorre as linhas de dados a partir da linha 2
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var produto = new Produto { CnpjRaiz = cnpjRaizPadrao };

                for (int j = 0; j < cabecalho.Count; j++)
                {
                    var nomeColuna = cabecalho[j];
                    var valor = row.Cell(j + 1).GetString().Trim();

                    MapearColuna(produto, nomeColuna, valor);
                }

                if (!string.IsNullOrWhiteSpace(produto.Codigo) && !string.IsNullOrWhiteSpace(produto.Ncm))
                {
                    produtos.Add(produto);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao ler arquivo XLSX: {ex.Message}");
        }

        return produtos;
    }

    // 2. LEITOR DE CSV (.csv)
    public List<Produto> LerPlanilhaCsv(string conteudoCsv, string cnpjRaizPadrao)
    {
        var produtos = new List<Produto>();

        if (string.IsNullOrWhiteSpace(conteudoCsv))
            return produtos;

        try
        {
            var linhas = conteudoCsv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (linhas.Length <= 1) return produtos;

            char delimitador = linhas[0].Contains(';') ? ';' : ',';
            var cabecalho = linhas[0].Split(delimitador).Select(c => c.Trim().ToUpperInvariant()).ToArray();

            for (int i = 1; i < linhas.Length; i++)
            {
                var colunas = linhas[i].Split(delimitador).Select(c => c.Trim()).ToArray();
                if (colunas.Length < 2) continue;

                var produto = new Produto { CnpjRaiz = cnpjRaizPadrao };

                for (int j = 0; j < cabecalho.Length && j < colunas.Length; j++)
                {
                    var nomeColuna = cabecalho[j];
                    var valor = colunas[j];

                    MapearColuna(produto, nomeColuna, valor);
                }

                if (!string.IsNullOrWhiteSpace(produto.Codigo) && !string.IsNullOrWhiteSpace(produto.Ncm))
                {
                    produtos.Add(produto);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao ler arquivo CSV: {ex.Message}");
        }

        return produtos;
    }

    // Função auxiliar que mapeia as colunas comuns
    private static void MapearColuna(Produto produto, string nomeColuna, string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return;

        switch (nomeColuna)
        {
            case "CODIGO":
            case "SKU":
            case "PARTNUMBER":
            case "PART_NUMBER":
                produto.Codigo = valor;
                break;

            case "DESCRICAO":
            case "NOME":
            case "PRODUTO":
            case "DESCRIPTION":
                produto.Descricao = valor;
                break;

            case "NCM":
            case "HSCODE":
            case "HS_CODE":
                produto.Ncm = valor;
                break;

            case "PAISORIGEM":
            case "PAIS":
            case "ORIGEM":
            case "COUNTRY":
                produto.PaisOrigem = valor;
                break;

            case "FABRICANTE":
            case "FORNECEDOR":
            case "MANUFACTURER":
                produto.Fabricante = valor;
                break;

            // === ATRIBUTOS DINÂMICOS (ANVISA / MAPA / INMETRO / ANATEL) ===
            case "REGISTROANVISA":
            case "REGISTRO_ANVISA":
            case "ANVISA":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_REG_ANVISA", Valor = valor });
                break;

            case "LOTE":
            case "NUMEROLOTE":
            case "BATCH":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_LOTE", Valor = valor });
                break;

            case "VALIDADE":
            case "DATAVALIDADE":
            case "EXPIRATION":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_VALIDADE", Valor = valor });
                break;

            case "COMPOSICAO":
            case "COMPOSICAOTEXTIL":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_COMPOSICAO", Valor = valor });
                break;

            case "CSI":
            case "CERTIFICADOSANITARIO":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_CSI", Valor = valor });
                break;

            case "SIF":
            case "ESTABELECIMENTO":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_SIF", Valor = valor });
                break;

            case "HOMOLOGACAOANATEL":
            case "ANATEL":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_HOMOLOGACAO_ANATEL", Valor = valor });
                break;

            case "MATERIAL":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_MATERIAL", Valor = valor });
                break;

            case "TAMANHO":
            case "NUMERACAO":
                produto.Atributos.Add(new AtributoValor { Codigo = "ATT_TAMANHO", Valor = valor });
                break;
        }
    }
}