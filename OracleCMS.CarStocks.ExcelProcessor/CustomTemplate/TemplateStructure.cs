using System.Reflection;
namespace OracleCMS.CarStocks.ExcelProcessor.CustomTemplate
{
    public class TemplateStructure
    {
        public Type? Type { get; set; }
        public string Name { get; set; } = "";
        public static IList<TemplateStructure> ToTemplateStructure(PropertyInfo[] properties)
        {
            List<TemplateStructure> templateStructures = [];
            foreach (var property in properties)
            {
                templateStructures.Add(new TemplateStructure
                {
                    Name = property.Name,
                    Type = property.PropertyType
                });
            }
            return templateStructures;
        }
    }
}
