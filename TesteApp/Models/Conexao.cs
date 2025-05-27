using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TesteApp.Models
{
    public class Conexao : IdentityDbContext<IdentityUser>
    {
        public Conexao(DbContextOptions<Conexao> options) : base(options)
        {
            
        }

        public DbSet<Produtos> Produtos { get; set; }
    }
}
