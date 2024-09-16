using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Pages.ViewAttachment
{
    [Authorize]
    public class IndexModel : BasePageModel<IndexModel>
    {
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult OnGet(string subFolder, string id, string fieldName, string fileName)
        {
            try
            {
                var uploadFilesPath = _configuration.GetValue<string>("UsersUpload:UploadFilesPath");
                // Construct the path to the requested file in your static folder.
                string filePath = Path.Combine(uploadFilesPath!, subFolder, id, fieldName,
                    fileName!);

                // Serve the file.
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return File(fileStream, Helper.ContentTypeHelper.GetContentType(fileName!), fileName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching the attachment. File Name {FileName}", fileName);
                return NotFound();
            }
        }
        
    }
}
