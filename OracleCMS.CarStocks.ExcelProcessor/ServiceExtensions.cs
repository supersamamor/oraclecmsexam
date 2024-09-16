using OracleCMS.CarStocks.ExcelProcessor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace OracleCMS.CarStocks.ExcelProcessor
{
    public static class ServiceExtensions
    {
        public static void AddExcelProcessor(this IServiceCollection services)
        {
            services.AddTransient<ExcelService>();
        }
    }
}
