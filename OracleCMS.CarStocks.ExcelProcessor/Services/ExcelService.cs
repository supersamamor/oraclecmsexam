using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.ExcelProcessor.Helper;
using OracleCMS.CarStocks.ExcelProcessor.Models;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.Reflection;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.CarStocks.ExcelProcessor.CustomValidation;
using OracleCMS.CarStocks.ExcelProcessor.CustomTemplate;

namespace OracleCMS.CarStocks.ExcelProcessor.Services
{
    public class ExcelService(ApplicationContext context)
	{
		public readonly ApplicationContext _context = context;
        public static string ExportTemplate<TableObject>(string fullFilePath)
		{
			if (!Directory.Exists(fullFilePath))
				Directory.CreateDirectory(fullFilePath);

			string input = typeof(TableObject).Name;
			string wordToRemove = "State";
			var fileName = $"{(input.EndsWith(wordToRemove) ? input[..^wordToRemove.Length] : input)}-BatchUploadTemplate.xlsx";
			var completeFilePath = Path.Combine(fullFilePath, fileName);

			// Check if the file already exists, and delete it if it does
			if (File.Exists(completeFilePath))
				File.Delete(completeFilePath);

			using (var package = new ExcelPackage(new FileInfo(completeFilePath)))
			{
				var workSheet = package.Workbook.Worksheets.Add("Sheet1");
				var columnIndex = 1;
				var (tableObjectFields, templateStructures) = GenerateTemplateStructures<TableObject>();              
				foreach (var item in templateStructures)
				{
					workSheet.Cells[1, columnIndex].Value = item.Name;
					if (typeDefaults.TryGetValue(item.Type!, out var defaultValue))
					{
						workSheet.Column(columnIndex).Style.Numberformat.Format = defaultValue.Format;
						workSheet.Cells[2, columnIndex].Value = defaultValue.Value;
					}
					else
					{
						workSheet.Cells[2, columnIndex].Value = "Value";
					}
					columnIndex++;
				}

				string headerRange = "A1:" + NumberHelper.NumberToExcelColumn(columnIndex - 1) + "1";
				ApplyHeaderStyles(workSheet.Cells[headerRange]);

				string sheet1modelRange = "A1:" + NumberHelper.NumberToExcelColumn(columnIndex - 1) + "2";
				ApplyBorderStyles(workSheet.Cells[sheet1modelRange]);

				workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
				package.Save();
			}
			return fileName;
		}
		public async Task<ExcelImportResultModel<TableObject>> ImportAsync<TableObject>(string fullFilePath, CancellationToken token = default) where TableObject : new()
		{
			List<ExcelRecord> recordsForUpload = [];
			var (tableObjectFields, templateStructures) = GenerateTemplateStructures<TableObject>();
			bool isSuccess = true;
			using (var stream = new MemoryStream(await File.ReadAllBytesAsync(fullFilePath, token)))
			using (var package = new ExcelPackage(stream))
			{
				ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
				var rowCount = workSheet.Dimension.Rows;
				for (int row = 2; row <= rowCount; row++)
				{
					var rowValue = new Dictionary<string, object?>();
					string errorRemarks = "";
					for (int columnIndex = 1; columnIndex <= templateStructures.Length(); columnIndex++)
					{
						var item = templateStructures[columnIndex - 1];
						try
						{
							var cellValue = workSheet?.Cells[row, columnIndex]?.Value?.ToString() ?? "";
							if (string.IsNullOrEmpty(cellValue))
							{
								rowValue.Add(item.Name, cellValue);
							}
							else
							{
								rowValue.Add(item.Name, ChangeTypeWithNullHandling(Format(item.Type!, cellValue), item.Type!));
							}
							if (columnIndex == templateStructures.Length())
							{
								rowValue = await CustomValidationPerRecordHandler(typeof(TableObject).Name, rowValue);
							}
						}
						catch (Exception ex)
						{
							isSuccess = false;
							rowValue[item.Name] = workSheet?.Cells[row, columnIndex].Value;
							if (columnIndex == templateStructures.Length())
							{
								errorRemarks += $"{ex.Message};";

							}
							else
							{
								errorRemarks += $"Field `{item.Name}` - {ex.Message};";
							}
						}
						if (columnIndex == templateStructures.Length())
						{
							if (!string.IsNullOrEmpty(errorRemarks))
							{
								rowValue.Add(Constants.ExcelUploadErrorRemarks, errorRemarks[0..^2]);
							}
						}
					}
					recordsForUpload.Add(new ExcelRecord { Data = rowValue, RowNumber = row });
				}
			}
			var listOfErrorsPerColumn = CustomBulkValidationHandler(typeof(TableObject).Name, recordsForUpload);
			if (listOfErrorsPerColumn != null)
			{
				foreach (var kvp in listOfErrorsPerColumn)
				{
					System.Collections.Generic.HashSet<int> values = kvp.Value;
					foreach (var value in values)
					{
						isSuccess = false;
						var recordToUpdate = recordsForUpload.FirstOrDefault(record => record.RowNumber == value);
						if (recordToUpdate != null)
						{
							if (recordToUpdate.Data.ContainsKey(Constants.ExcelUploadErrorRemarks))
							{
								recordToUpdate.Data[Constants.ExcelUploadErrorRemarks] = recordToUpdate.Data[Constants.ExcelUploadErrorRemarks] + $";The field `{kvp.Key}` is duplicate from the excel file, please check other rows for duplicate.";
							}
							else
							{
								recordToUpdate.Data.Add(Constants.ExcelUploadErrorRemarks, $"The field `{kvp.Key}` is duplicate from the excel file, please check other rows for duplicate.");
							}
						}
					}
				}
			}
			if (isSuccess)
			{
				List<TableObject> successfulRecords = recordsForUpload.Select(record => CreateObjectFromRow<TableObject>(record.Data, tableObjectFields)).ToList();
				return new ExcelImportResultModel<TableObject> { SuccessRecords = successfulRecords, IsSuccess = true };
			}
			else
			{
				return new ExcelImportResultModel<TableObject> { FailedRecords = recordsForUpload, IsSuccess = false };
			}
		}

		public static string ExportExcelValidationResult<TableObject>(List<ExcelRecord> list, string fullFilePath)
		{
			if (!Directory.Exists(fullFilePath))
				Directory.CreateDirectory(fullFilePath);
			var (tableObjectFields, templateStructures) = GenerateTemplateStructures<TableObject>();
			string input = typeof(TableObject).Name;
			string wordToRemove = "State";
			var fileName = $"{(input.EndsWith(wordToRemove) ? input[..^wordToRemove.Length] : input)}-ValidationResult-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
			var completeFilePath = Path.Combine(fullFilePath, fileName);
			using (var package = new ExcelPackage(new FileInfo(completeFilePath)))
			{
				var workSheet = package.Workbook.Worksheets.Add("Sheet1");
				var columnIndex = 1;
				StringComparer comparer = StringComparer.OrdinalIgnoreCase;
				foreach (var item in templateStructures)
				{
					workSheet.Cells[1, columnIndex].Value = item.Name;
					// Check if the property type is nullable (e.g., DateTime?)                  
					if (typeDefaults.TryGetValue(item.Type!, out var defaultValue))
					{
						workSheet.Column(columnIndex).Style.Numberformat.Format = defaultValue.Format;
					}
					columnIndex++;
				}
				workSheet.Cells[1, columnIndex].Value = "Error Remarks";
				workSheet.Column(columnIndex).Style.Font.Color.SetColor(Color.Red);
				int row = 2;
				foreach (var record in list.OrderBy(l => l.RowNumber))
				{
					columnIndex = 1;
					foreach (var item in templateStructures)
					{
						workSheet.Cells[row, columnIndex].Value = record.Data[item.Name];
						columnIndex++;
					}
					if (record.Data.TryGetValue(Constants.ExcelUploadErrorRemarks, out var errorRemark))
					{
						workSheet.Cells[row, columnIndex].Value = errorRemark;
					}
					row++;
				}
				string headerRange = "A1:" + NumberHelper.NumberToExcelColumn(columnIndex) + "1";
				ApplyHeaderStyles(workSheet.Cells[headerRange]);
				string sheet1modelRange = "A1:" + NumberHelper.NumberToExcelColumn(columnIndex) + (row - 1);
				ApplyBorderStyles(workSheet.Cells[sheet1modelRange]);
				workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
				package.Save();
			}
			return completeFilePath;
		}
		public static string UpdateExistingExcelValidationResult<TableObject>(List<ExcelRecord> list, string fullFilePath, string existingExcelFilePath)
		{
			if (!File.Exists(existingExcelFilePath))
			{
				throw new FileNotFoundException("The specified existing Excel file does not exist.", existingExcelFilePath);
			}
			if (!Directory.Exists(fullFilePath))
				Directory.CreateDirectory(fullFilePath);

			string input = typeof(TableObject).Name;
			string wordToRemove = "State";
			var fileName = $"{(input.EndsWith(wordToRemove) ? input[..^wordToRemove.Length] : input)}-ValidationResult-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
			var completeFilePath = Path.Combine(fullFilePath, fileName);
			using (var package = new ExcelPackage(new FileInfo(existingExcelFilePath)))
			{
				var workSheet = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("Sheet1");
				var (tableObjectFields, templateStructures) = GenerateTemplateStructures<TableObject>();
				var columnIndex = templateStructures.Length() + 1;
				workSheet.Cells[1, columnIndex].Value = "Error Remarks";
				workSheet.Column(columnIndex).Style.Font.Color.SetColor(Color.Red);
				int row = 2;
				foreach (var record in list.OrderBy(l => l.RowNumber))
				{
					if (record.Data.TryGetValue(Constants.ExcelUploadErrorRemarks, out var errorRemark))
					{
						workSheet.Cells[row, columnIndex].Value = errorRemark;
					}
					row++;
				}
				string headerRange = "A1:" + NumberHelper.NumberToExcelColumn(columnIndex) + "1";
				ApplyHeaderStyles(workSheet.Cells[headerRange]);
				string sheet1modelRange = "A1:" + NumberHelper.NumberToExcelColumn(columnIndex) + (row - 1);
				ApplyBorderStyles(workSheet.Cells[sheet1modelRange]);
				workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
				package.SaveAs(new FileInfo(completeFilePath));
			}
			return completeFilePath;
		}


		private static readonly Dictionary<Type, (object? Value, string? Format)> typeDefaults = new()
		{
			{ typeof(DateTime), (DateTime.Now, "MM/dd/yyyy") },
			{ typeof(int), (0, "#,##0") },
			{ typeof(decimal), (0, "#,##0.00") },
			{ typeof(double), (0.0, "#,##0.00") },
			{ typeof(float), (0.0f, "#,##0.00") },
			{ typeof(short), ((short)0, "#,##0") },
			{ typeof(byte), ((byte)0, "#,##0") },
			{ typeof(char), ('A', null) },
			{ typeof(bool), ("false", null) },
		};
		private static string? Format(Type dataType, object value, string? dateFormat = null)
		{
			if (typeDefaults.TryGetValue(dataType, out var formatInfo))
			{
				string? formatString = formatInfo.Format;

				if (dataType == typeof(DateTime) || dataType == typeof(DateTime?))
				{
					if (double.TryParse(value.ToString(), out double oleAutomationDate))
					{
						DateTime convertedDateTime = DateTime.FromOADate(oleAutomationDate);
						return string.Format("{0:" + (dateFormat ?? formatString) + "}", convertedDateTime);
					}
					else if (DateTime.TryParse(value.ToString(), out DateTime dateTime))
					{
						return string.Format("{0:" + (dateFormat ?? formatString) + "}", dateTime);
					}
					else
					{
						return value == null ? string.Empty : string.Format("{0:" + formatString + "}", value);
					}
				}
				else
				{
					return string.Format("{0:" + formatString + "}", value);
				}
			}
			else
			{
				return value?.ToString() ?? string.Empty;
			}
		}
        private static object? ChangeTypeWithNullHandling(string? value, Type? conversionType)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException(nameof(conversionType));
            }
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                conversionType = Nullable.GetUnderlyingType(conversionType);
            }
            return Convert.ChangeType(value, conversionType!);
        }
        private static PropertyInfo[] GetTableObjectFields<TableObject>()
		{
			Type tableObjectType = typeof(TableObject);
			PropertyInfo[] properties = tableObjectType.GetProperties();

			Type baseEntityType = typeof(Common.Core.Base.Models.BaseEntity);
			// Get all the fields of the BaseEntity class          
			PropertyInfo[] baseEntityFields = baseEntityType.GetProperties();
			// Include only properties with primitive data types
			properties = properties.Where(prop => prop.PropertyType.IsPrimitive
				|| prop.PropertyType.IsEnum || prop.PropertyType == typeof(string)
				|| prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?) || prop.PropertyType == typeof(int?)
				|| prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?)).ToArray();
			properties = properties.Where(prop => !baseEntityFields.Any(baseProp => baseProp.Name == prop.Name)).ToArray();
			return properties;
		}
		private static void ApplyHeaderStyles(ExcelRangeBase headerRange)
		{
			headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
			headerRange.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
			headerRange.Style.Font.Bold = true;
			headerRange.Style.Font.Color.SetColor(Color.White);
		}

		private static void ApplyBorderStyles(ExcelRangeBase range)
		{
			range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
			range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
			range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
			range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
		}

		private static TableObject CreateObjectFromRow<TableObject>(Dictionary<string, object?> rowValue, PropertyInfo[] tableObjectFields) where TableObject : new()
		{
			var successRecord = new TableObject();
			if (typeof(SampleTableState).IsAssignableFrom(typeof(TableObject))) //Note : this part is for batch upload module with custom template
			{
				successRecord = SampleTableCustomTemplate.CreateRecord<TableObject>(rowValue);
			}
			else
			{
				foreach (var propertyName in tableObjectFields)
				{

					if (Nullable.GetUnderlyingType(propertyName.PropertyType) != null && string.IsNullOrEmpty(rowValue[propertyName.Name]?.ToString()))
					{
						propertyName.SetValue(successRecord, null);
					}
					else
					{
						propertyName.SetValue(successRecord, rowValue[propertyName.Name]);
					}
				}
			}
			return successRecord;
		}
		private static (PropertyInfo[] TableObjectFields, IList<TemplateStructure> TemplateStructures) GenerateTemplateStructures<TableObject>()            
		{
			PropertyInfo[] tableObjectFields = GetTableObjectFields<TableObject>();
			IList<TemplateStructure>? templateStructures;
			if (typeof(SampleTableState).IsAssignableFrom(typeof(TableObject))) //Note : this part is for batch upload module with custom template
			{
				templateStructures = SampleTableCustomTemplate.Generate();
			}
			else
			{
				templateStructures = TemplateStructure.ToTemplateStructure(tableObjectFields);
			}
			return (tableObjectFields, templateStructures);
		}
        private async Task<Dictionary<string, object?>> CustomValidationPerRecordHandler(string module, Dictionary<string, object?> rowValue)
        {
            //Implement Custom Validation Here Depending on Model/Table Name
            switch (module)
            {
                case nameof(DealersState):
					return await DealersValidator.ValidatePerRecordAsync(_context, rowValue);
				case nameof(CarsState):
					return await CarsValidator.ValidatePerRecordAsync(_context, rowValue);
				case nameof(StocksState):
					return await StocksValidator.ValidatePerRecordAsync(_context, rowValue);
				
                default: break;
            }
            return rowValue;
        }
        private Dictionary<string, HashSet<int>>? CustomBulkValidationHandler(string module, List<ExcelRecord> records)
        {
            //Implement Custom Validation Here Depending on Model/Table Name
            switch (module)
            {
                												
                default: break;
            }
            return null;
        }
    }
}
