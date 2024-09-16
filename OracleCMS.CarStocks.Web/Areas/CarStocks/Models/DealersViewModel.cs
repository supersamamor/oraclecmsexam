using OracleCMS.Common.Web.Utility.Extensions;
using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using OracleCMS.Common.Web.Utility.Annotations;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Models;

public record DealersViewModel : BaseViewModel
{	
	[Display(Name = "Dealer Name")]
	[Required]
	[StringLength(100, ErrorMessage = "{0} length can't be more than {1}.")]
	public string DealerName { get; init; } = "";
	[Display(Name = "Dealer Website")]
	[Required]
	[StringLength(255, ErrorMessage = "{0} length can't be more than {1}.")]
	public string DealerWebsite { get; init; } = "";
	
	public DateTime LastModifiedDate { get; set; }
		
	public IList<StocksViewModel>? StocksList { get; set; }
	
}
