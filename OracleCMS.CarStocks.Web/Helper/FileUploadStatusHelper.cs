using OracleCMS.CarStocks.Core.Constants;
namespace OracleCMS.CarStocks.Web.Helper
{
    public static class FileUploadStatusHelper
    {
        public static string GetBadge(string status)
        {
            switch (status)
            {
                case FileUploadStatus.Pending:
                    return @"<span class=""badge bg-info"">" + status + @"</span>";
                case FileUploadStatus.Done:
                    return @"<span class=""badge bg-success"">" + status + @"</span>";
                case FileUploadStatus.Failed:
                    return @"<span class=""badge bg-danger"">" + status + @"</span>";
                default:
                    return "";
            }
        }
    }
}
