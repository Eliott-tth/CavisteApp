using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CavisteApp.Data;

public class CavisteDbContextFactory : IDesignTimeDbContextFactory<CavisteDbContext>
{
    public CavisteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CavisteDbContext>();
        return new CavisteDbContext();
    }
}
