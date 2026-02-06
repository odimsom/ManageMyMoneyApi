using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ManageMyMoney.Infrastructure.Persistence.Context;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ManageMyMoneyContext>
{
    public ManageMyMoneyContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ManageMyMoneyContext>();
        
        var connectionString = "Host=localhost;Database=managemymoney_dev;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new ManageMyMoneyContext(optionsBuilder.Options);
    }
}