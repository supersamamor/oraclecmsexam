using OracleCMS.CarStocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.ExcelProcessor.Models;
using OracleCMS.CarStocks.ExcelProcessor.Helper;


namespace OracleCMS.CarStocks.ExcelProcessor.CustomValidation
{
    public static class CarsValidator
    {
        public static async Task<Dictionary<string, object?>>  ValidatePerRecordAsync(ApplicationContext context, Dictionary<string, object?> rowValue)
        {
			string errorValidation = "";
            var make = rowValue[nameof(CarsState.Make)]?.ToString();
			if (!string.IsNullOrEmpty(make))
			{
				var makeMaxLength = 50;
				if (make.Length > makeMaxLength)
				{
					errorValidation += $"Make should be less than {makeMaxLength} characters.;";
				}
			}
			var model = rowValue[nameof(CarsState.Model)]?.ToString();
			if (!string.IsNullOrEmpty(model))
			{
				var modelMaxLength = 50;
				if (model.Length > modelMaxLength)
				{
					errorValidation += $"Model should be less than {modelMaxLength} characters.;";
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
