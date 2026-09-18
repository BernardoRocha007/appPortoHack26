namespace appPortoHack.API.Models;

public class Duimp
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string CnpjRaiz { get; set; } = string.Empty;
    public DateTime DataRegistro { get; set; } = DateTime.Now;
    public string Canal { get; set; } = string.Empty;
    public string Situacao { get; set; } = "REGISTRADA";
    
    public string Moeda { get; set; } = "USD";
    public decimal ValorTotalMercadorias { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorSeguro { get; set; }
    public string RecintoAlfandegado { get; set; } = "Porto de Santos";

    // Itens que vieram nessa viagem (Relacionamento 1 para N)
    public List<ItemDuimp> Itens { get; set; } = new();
}

public class ItemDuimp
{
    public int Id { get; set; }
    public int DuimpId { get; set; }
    public int NumeroItem { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal => Quantidade * ValorUnitario;
    public string? NumeroLpco { get; set; } // Licença Anvisa/MAPA
}