using OracleCMS.CarStocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.ExcelProcessor.Models;
using OracleCMS.CarStocks.ExcelProcessor.Helper;


namespace OracleCMS.CarStocks.ExcelProcessor.CustomValidation
{
    public static class StocksValidator
    {
        public static async Task<Dictionary<string, object?>>  ValidatePerRecordAsync(ApplicationContext context, Dictionary<string, object?> rowValue)
        {
			string errorValidation = "";
            var carID = rowValue[nameof(StocksState.CarID)]?.ToString();
			var cars = await context.Cars.Where(l => l.Id == carID).AsNoTracking().IgnoreQueryFilters().FirstOrDefaultAsync();
			if(cars == null) {
				errorValidation += $"Car ID is not valid.; ";
			}
			else
			{
				rowValue[nameof(StocksState.CarID)] = cars?.Id;
			}
			var dealerID = rowValue[nameof(StocksState.DealerID)]?.ToString();
			var dealers = await context.Dealers.Where(l => l.Id == dealerID).AsNoTracking().IgnoreQueryFilters().FirstOrDefaultAsync();
			if(dealers == null) {
				errorValidation += $"Dealer ID is not valid.; ";
			}
			else
			{
				rowValue[nameof(StocksState.DealerID)] = dealers?.Id;
			}
			
			
			if (!string.IsNullOrEmpty(errorValidation))
			{
				throw new Exception(errorValidation);
			}
            return rowValue;
        }
			
		public static Dictionary<string, HashSet<int>> DuplicateValidation(List<ExcelRecord> records)
		{
			List<string> listOfKeys = new()
			{
																				
			};
			return listOfKeys.Count > 0 ? DictionaryHelper.FindDuplicateRowNumbersPerKey(records, listOfKeys) : new Dictionary<string, HashSet<int>>();
		}
    }
}
