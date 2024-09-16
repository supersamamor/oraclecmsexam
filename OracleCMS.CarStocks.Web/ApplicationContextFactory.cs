using OracleCMS.Common.Web.Utility.Identity;
using OracleCMS.CarStocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OracleCMS.CarStocks.Web;

class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var builder = new DbContextOptionsBuilder<ApplicationContext>();
        builder.UseSqlServer(configuration.GetConnectionString("ApplicationContext"));
        return new ApplicationContext(builder.Options, new DefaultAuthenticatedUser(new HttpContextAccessor()));
    }
}
