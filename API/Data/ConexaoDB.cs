using Microsoft.EntityFrameworkCore;
using appPortoHack.API.Models;

namespace appPortoHack.API.Data;

public class ConexaoDB(DbContextOptions<ConexaoDB> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<AtributoValor> ProdutoAtributos => Set<AtributoValor>();
    public DbSet<Duimp> Duimps => Set<Duimp>();
    public DbSet<ItemDuimp> DuimpItens => Set<ItemDuimp>();
    public DbSet<OperadorEstrangeiro> OperadoresEstrangeiros => Set<OperadorEstrangeiro>();
}
