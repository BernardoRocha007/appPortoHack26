namespace appPortoHack.API.Models;

public class Produto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Ncm { get; set; } = string.Empty;
    public string CnpjRaiz { get; set; } = string.Empty;
    public string PaisOrigem { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;

    // Lista de Atributos Específicos da NCM (Relacionamento 1 para N)
    public List<AtributoValor> Atributos { get; set; } = new();
}

public class AtributoValor
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}