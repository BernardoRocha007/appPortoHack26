using appPortoHack.API.Models;

namespace appPortoHack.API.Services;

// Recebe tanto o serviço de regras quanto o serviço do Banco!
public class ProdutoService(CadAtributosService cadAtributosService, BancoDadosService bancoDadosService)
{
    // 1. Pega os produtos direto do Banco de Dados
    public List<Produto> ObterTodos(string? cnpjRaiz = null)
    {
        try
        {
            var produtos = bancoDadosService.ObterDados().Produtos;

            if (string.IsNullOrWhiteSpace(cnpjRaiz))
                return produtos;

            return produtos.Where(p => p.CnpjRaiz == cnpjRaiz).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao obter produtos: {ex.Message}");
            return new List<Produto>();
        }
    }

    // 2. Busca produto no Banco pelo código e CNPJ
    public Produto? ObterPorCodigo(string codigo, string cnpjRaiz)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(cnpjRaiz))
                return null;

            return bancoDadosService.ObterDados().Produtos.FirstOrDefault(p => 
                p.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase) && 
                p.CnpjRaiz == cnpjRaiz);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao buscar produto: {ex.Message}");
            return null;
        }
    }

    // 3. Validador Anti-Canal Vermelho e Cadastro no Banco
    public (bool Sucesso, string Mensagem, List<string> Erros) ValidarECadastrar(Produto produto)
    {
        var erros = new List<string>();

        try
        {
            if (produto == null)
                return (false, "Dados do produto não foram fornecidos.", new List<string> { "Objeto nulo." });

            if (string.IsNullOrWhiteSpace(produto.Codigo))
                erros.Add("O código do produto é obrigatório.");

            if (string.IsNullOrWhiteSpace(produto.Ncm))
                erros.Add("O código NCM é obrigatório.");

            if (string.IsNullOrWhiteSpace(produto.CnpjRaiz))
                erros.Add("O CNPJ raiz da empresa importadora é obrigatório.");

            if (erros.Count > 0)
                return (false, "Campos básicos obrigatórios não preenchidos.", erros);

            // Validação A: Já existe no banco?
            if (ObterPorCodigo(produto.Codigo, produto.CnpjRaiz) != null)
            {
                return (false, $"O produto com código '{produto.Codigo}' já está cadastrado no catálogo deste CNPJ.", erros);
            }

            // Validação B: A NCM existe nas regras?
            var regraNcm = cadAtributosService.ObterPorNcm(produto.Ncm);
            if (regraNcm == null)
            {
                erros.Add($"A NCM '{produto.Ncm}' não foi encontrada na tabela de regras.");
                return (false, "NCM não reconhecida.", erros);
            }

            // Validação C: CORRETOR ANTI-CANAL VERMELHO
            foreach (var atributoExigido in regraNcm.AtributosObrigatorios)
            {
                var preenchido = produto.Atributos.FirstOrDefault(a => 
                    a.Codigo.Equals(atributoExigido.Codigo, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(a.Valor));

                if (preenchido == null)
                {
                    erros.Add($"🚨 [Risco Canal Vermelho]: Falta o atributo obrigatório '{atributoExigido.Nome}' ({atributoExigido.Codigo}) exigido pelo órgão {regraNcm.OrgaoAnuente}.");
                }
            }

            if (erros.Count > 0)
            {
                return (false, "O produto possui pendências técnicas impeditivas para a DUIMP.", erros);
            }

            // SALVA NO BANCO DE DADOS E GRAVA NO DISCO!
            var banco = bancoDadosService.ObterDados();
            banco.Produtos.Add(produto);
            bancoDadosService.Salvar(banco);

            return (true, "Produto cadastrado com sucesso no Catálogo da DUIMP!", erros);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao cadastrar: {ex.Message}");
            return (false, "Erro interno durante o cadastro.", new List<string> { ex.Message });
        }
    }

    // 4. Cadastro em Lote
    public (int Cadastrados, int IgnoradosDuplicados, List<string> Erros) CadastrarEmLote(List<Produto> produtos)
    {
        int cadastrados = 0;
        int duplicados = 0;
        var todosErros = new List<string>();

        try
        {
            if (produtos == null || produtos.Count == 0)
                return (0, 0, new List<string> { "Nenhum produto enviado." });

            foreach (var prod in produtos)
            {
                if (ObterPorCodigo(prod.Codigo, prod.CnpjRaiz) != null)
                {
                    duplicados++;
                    continue;
                }

                var resultado = ValidarECadastrar(prod);
                if (resultado.Sucesso)
                {
                    cadastrados++;
                }
                else
                {
                    todosErros.AddRange(resultado.Erros.Select(e => $"Item '{prod.Descricao}' ({prod.Codigo}): {e}"));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha no lote: {ex.Message}");
            todosErros.Add($"Erro crítico no lote: {ex.Message}");
        }

        return (cadastrados, duplicados, todosErros);
    }
}