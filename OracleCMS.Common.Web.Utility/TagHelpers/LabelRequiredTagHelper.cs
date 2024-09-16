using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
namespace OracleCMS.Common.Web.Utility.TagHelpers
{
    [HtmlTargetElement("label", Attributes = "asp-for")]
    public class LabelRequiredTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression? For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (For!.Metadata.IsRequired)
            {
                string? existingClasses = output.Attributes.FirstOrDefault(a => a.Name == "class")?.Value.ToString();
                string requiredClass = "fieldrequired";
                output.Attributes.SetAttribute("class", string.IsNullOrEmpty(existingClasses) ? requiredClass : $"{existingClasses} {requiredClass}");
            }
        }
    }
}
