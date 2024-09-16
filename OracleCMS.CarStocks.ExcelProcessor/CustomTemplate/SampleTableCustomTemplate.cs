using OracleCMS.CarStocks.ExcelProcessor.CustomTemplate;
using System.Reflection;

namespace OracleCMS.CarStocks.ExcelProcessor.CustomValidation
{
    /// <summary>
    /// This is a sample only for implementing batch upload with custom template
    /// </summary>
    public static class SampleTableCustomTemplate
    {
        public const string SampleField1 = "Sample Field 1";
        public const string SampleField2 = "Sample Field 2";
        public const string SampleField3 = "Sample Field 3";
        public const string SampleField4 = "Sample Field 4";
        public static IList<TemplateStructure> Generate()
        {
            IList<TemplateStructure> templateStructure = new List<TemplateStructure>
            {
                new() { Name = SampleField1, Type = typeof(string)},
                new() { Name = SampleField2, Type = typeof(DateTime)},
                new() { Name = SampleField3, Type = typeof(decimal)},
                new() { Name = SampleField4, Type = typeof(int)},
            };
            return templateStructure;
        }

        public static TableObject CreateRecord<TableObject>(Dictionary<string, object?> rowValue) where TableObject : new()
        {
            TableObject successRecord = new();
            Type unitStateType = typeof(SampleTableState);    
            foreach (var propertyName in Generate())
            {
                switch (propertyName.Name)
                {
                    case SampleField1:
                        PropertyInfo? sampleField1PropertyInfo = unitStateType.GetProperty(nameof(SampleTableState.SampleField1));
                        sampleField1PropertyInfo!.SetValue(successRecord, "Sample Field 1 Value");
                        break;
                    case SampleField2:
                        PropertyInfo? sampleField2PropertyInfo = unitStateType.GetProperty(nameof(SampleTableState.SampleField2));
                        sampleField2PropertyInfo!.SetValue(successRecord, DateTime.Now);
                        break;
                    case SampleField3:
                        PropertyInfo? sampleField3PropertyInfo = unitStateType.GetProperty(nameof(SampleTableState.SampleField3));
                        sampleField3PropertyInfo!.SetValue(successRecord, 12345.67);
                        break;
                    case SampleField4:
                        PropertyInfo? sampleField4PropertyInfo = unitStateType.GetProperty(nameof(SampleTableState.SampleField4));
                        sampleField4PropertyInfo!.SetValue(successRecord, 12345);
                        break;
                    default:
                        break;
                }
            }          
            // Custom logic to map/convert record to result
            return successRecord;
        }
    }
}
