using AspNetCoreHero.ToastNotification.Extensions;
using OracleCMS.Common.Web.Utility.Logging;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.CarStocks.Web;
using OracleCMS.CarStocks.Web.Areas.Identity;
using OracleCMS.CarStocks.Web.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OracleCMS.CarStocks.EmailSending;
using OracleCMS.CarStocks.ExcelProcessor;
using OracleCMS.Common.Services.Shared;
using OracleCMS.CarStocks.ChatGPT;
using Serilog;
using OracleCMS.CarStocks.Scheduler;
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

// Add services to the container.
var configuration = builder.Configuration;
var services = builder.Services;

services.ConfigureIdentityServices(configuration);
services.ConfigureDefaultServices(configuration);

if (configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    services.AddDbContext<ApplicationContext>(options =>
        options.UseInMemoryDatabase("ApplicationContext"));
}
else
{
    services.AddDbContext<ApplicationContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("ApplicationContext"),
                             o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)));
}

services.AddHealthChecks()
        .AddDbContextCheck<ApplicationContext>()
        .AddDbContextCheck<IdentityContext>();
services.AddEmailSendingAService(configuration);
services.AddExcelProcessor();
services.AddSharedServices(configuration);
services.AddChatGPTApiService(configuration);
services.AddScheduler(configuration);
var app = builder.Build();
using (var serviceScope = app.Services.CreateScope())
{
    var serviceProvider = serviceScope.ServiceProvider;
    try
    {
        var appContext = serviceProvider.GetRequiredService<ApplicationContext>();
        appContext.Database.EnsureCreated();
        appContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred ensuriong DB was created.");
    }
}
// Static Files
var uploadFilesPath = configuration.GetValue<string>("UsersUpload:UploadFilesPath");
if (uploadFilesPath != null)
{
    bool uploadFilesPathExists = System.IO.Directory.Exists(uploadFilesPath);
    if (!uploadFilesPathExists)
        System.IO.Directory.CreateDirectory(uploadFilesPath);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
var baseUrl = configuration.GetValue<string>("BaseUrl");
app.UseHttpsRedirection();
app.UseSecurityHeaders(policies =>
    policies.AddDefaultSecurityHeaders()
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddUpgradeInsecureRequests();
				builder.AddBlockAllMixedContent();
				builder.AddDefaultSrc().None().OverHttps();
				builder.AddScriptSrc()
						.Self()
						.StrictDynamic()
						.WithNonce()
						.OverHttps();
				builder.AddStyleSrc()
						.Self()
						.UnsafeEval()
						.StrictDynamic()
						.WithNonce()
						.OverHttps();
				builder.AddImgSrc().OverHttps().Data();
				builder.AddObjectSrc().None();
				builder.AddBaseUri().None();
				builder.AddFrameAncestors().Self();
				builder.AddFormAction().From(baseUrl!).Self().OverHttps();
				builder.AddConnectSrc().From(baseUrl!).OverHttps();
				builder.AddFontSrc().From(new string[] { "https://fonts.gstatic.com",
					baseUrl!,"https://stackpath.bootstrapcdn.com" }).OverHttps();
            }));
app.UseWebOptimizer();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 365;
        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
            "public,max-age=" + durationInSeconds;
    }
});
app.UseCookiePolicy();
app.UseSerilogRequestLogging();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.EnrichDiagnosticContext();
app.MapControllers();
app.MapRazorPages();
app.MapHealthChecks("/health").AllowAnonymous();
app.UseNotyf();

// Seed the database
if (configuration.GetValue<bool>("EnableDatabaseSeed"))
{ 
    Log.Information("Seeding database");
	var scope = app.Services.CreateScope();
	await DefaultEntity.Seed(scope.ServiceProvider);
	await DefaultRole.Seed(scope.ServiceProvider);
	await DefaultUser.Seed(scope.ServiceProvider);
	await DefaultClient.Seed(scope.ServiceProvider);
	await UserRole.Seed(scope.ServiceProvider);
	await DefaultDashboard.Seed(scope.ServiceProvider);
	Log.Information("Finished seeding database");
}
app.Run();
