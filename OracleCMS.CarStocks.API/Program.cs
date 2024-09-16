using OracleCMS.Common.API;
using OracleCMS.Common.Web.Utility.Logging;
using OracleCMS.CarStocks.Application;
using OracleCMS.CarStocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using OracleCMS.CarStocks.Infrastructure;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).ReadFrom
                          .Services(services).Enrich
                          .FromLogContext());


// Reading connection strings from environment variables
builder.Configuration.AddEnvironmentVariables();

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDefaultApiServices(configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationContext>();
builder.Services.AddDbContext<IdentityContext>(options
       => options.UseSqlServer(configuration.GetConnectionString("ApplicationContext")));
if (configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    builder.Services.AddDbContext<ApplicationContext>(options
        => options.UseInMemoryDatabase("ApplicationContext"));
}
else
{
    builder.Services.AddDbContext<ApplicationContext>(options
        => options.UseSqlServer(configuration.GetConnectionString("ApplicationContext")));
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.EnableSwagger();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseRouting();
var enableAuthentication = configuration.GetValue<bool>("Authentication:Enabled");
if (enableAuthentication)
{
    app.UseAuthentication();  
}
app.UseAuthorization();
app.EnrichDiagnosticContext();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();


// Seed the database after the app is built
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var appContext = services.GetRequiredService<ApplicationContext>();
        appContext.Database.EnsureCreated();  // Make sure the database is created
        SeedData.Seed(appContext);            // Seed the data
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database.");
    }
}

app.Run();
