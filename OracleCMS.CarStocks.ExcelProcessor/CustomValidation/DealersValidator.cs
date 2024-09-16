using OracleCMS.CarStocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.ExcelProcessor.Models;
using OracleCMS.CarStocks.ExcelProcessor.Helper;


namespace OracleCMS.CarStocks.ExcelProcessor.CustomValidation
{
    public static class DealersValidator
    {
        public static async Task<Dictionary<string, object?>>  ValidatePerRecordAsync(ApplicationContext context, Dictionary<string, object?> rowValue)
        {
			string errorValidation = "";
            var dealerName = rowValue[nameof(DealersState.DealerName)]?.ToString();
			if (!string.IsNullOrEmpty(dealerName))
			{
				var dealerNameMaxLength = 100;
				if (dealerName.Length > dealerNameMaxLength)
				{
					errorValidation += $"Dealer Name should be less than {dealerNameMaxLength} characters.;";
				}
			}
			var dealerWebsite = rowValue[nameof(DealersState.DealerWebsite)]?.ToString();
			if (!string.IsNullOrEmpty(dealerWebsite))
			{
				var dealerWebsiteMaxLength = 255;
				if (dealerWebsite.Length > dealerWebsiteMaxLength)
				{
					errorValidation += $"Dealer Website should be less than {dealerWebsiteMaxLength} characters.;";
				}
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
