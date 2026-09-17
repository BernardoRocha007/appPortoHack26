using appPortoHack.API.Models;

namespace appPortoHack.API.Models;
public class BancoDados
{
    public List<Produto> Produtos { get; set; } = new();
    public List<Duimp> Duimps { get; set; } = new();
    public List<OperadorEstrangeiro> OperadoresEstrangeiros { get; set; } = new();
}