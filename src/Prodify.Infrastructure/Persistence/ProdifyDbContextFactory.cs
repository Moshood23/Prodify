using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Prodify.Infrastructure.Persistence;

public class ProdifyDbContextFactory : IDesignTimeDbContextFactory<ProdifyDbContext>
{
    public ProdifyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProdifyDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=Prodify;User Id=sa;Password=Prodify_Dev_2026!;TrustServerCertificate=True;");

        return new ProdifyDbContext(optionsBuilder.Options);
    }
}