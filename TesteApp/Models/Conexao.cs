using Microsoft.EntityFrameworkCore;

namespace TesteApp.Models
{
    public class Conexao : DbContext 
    {
        public Conexao(DbContextOptions<Conexao> options) : base(options)
        {
            
        }

        public DbSet<Produtos> Produtos { get; set; }
    }
}
