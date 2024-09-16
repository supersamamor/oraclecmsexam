namespace OracleCMS.CarStocks.Web.Models
{
    public class ListingToolbarModel(string pageTitle)
    {
        public string PageTitle { get; set; } = pageTitle;
        public List<Button> Buttons { get; set; } = [];
    }
    public class Button(string page, string area, string type, int sequence)
    {
        public Button(string type, int sequence) : this("", "", type, sequence)
        {
            this.Type = type;
            this.Sequence = sequence;
        }
        public string Page { get; set; } = page;
        public string Area { get; set; } = area;
        public int Sequence { get; set; } = sequence;
        public string Type { get; set; } = type;
        public string StyleClass
        {
            get
            {
                switch (this.Type)
                {
                    case ButtonType.Add:
                        return "btn btn-success text-light";
                    case ButtonType.Download:
                        return "btn btn-primary text-light";
                    case ButtonType.Upload:
                    case ButtonType.DownloadTemplate:
                        return "btn btn-custom-dark-blue text-light";
                    case ButtonType.Back:
                        return "btn btn-secondary text-light";
                    default:
                        return "btn btn-primary text-light";
                }
            }
        }
        public string DOMId
        {
            get
            {
                return $"{ListingToolbarConstants.ButtonPrefix}{this.Type}";
            }
        }
        public string Label
        {
            get
            {
                switch (this.Type)
                {
                    case ButtonType.Add:
                    case ButtonType.Download:
                    case ButtonType.Upload:
                        return this.Type;
                    case ButtonType.DownloadTemplate:
                        return "Download Template";
                    default:
                        return this.Type;
                }
            }
        }
    }
    public static class ButtonType
    {
        public const string Add = "Add";
        public const string Download = "Download";
        public const string Upload = "Upload";
        public const string Back = "Back";
        public const string DownloadTemplate = "DownloadTemplate";
    }
    public static class ListingToolbarConstants
    {
        public const string ButtonPrefix = "toolbarButton";
    }
}
