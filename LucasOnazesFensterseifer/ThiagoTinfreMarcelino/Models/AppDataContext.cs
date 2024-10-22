using Microsoft.EntityFrameworkCore;

namespace api.Models;

public class AppDataContext : DbContext
{
    public DbSet<Funcionario> Funcionarios { get; set; }

    public DbSet<FolhaPagamento> Folhas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=lucas_thiago.db");
    }
}
