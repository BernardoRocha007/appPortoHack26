using Microsoft.EntityFrameworkCore;

namespace appPortoHack26.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
    }

    // O Model do banco de dados (com Id)
    public class Produto
    {
        public int Id { get; set; } // Chave primária obrigatória pro banco
        public string Sku { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Ncm { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Fabricante { get; set; } = string.Empty;
        public string Atributos { get; set; } = string.Empty;
    }
}