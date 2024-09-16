using OracleCMS.Common.Web.Utility.Extensions;
using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using OracleCMS.Common.Web.Utility.Annotations;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Models;

public record CarsViewModel : BaseViewModel
{	
	[Display(Name = "Make")]
	[Required]
	[StringLength(50, ErrorMessage = "{0} length can't be more than {1}.")]
	public string Make { get; init; } = "";
	[Display(Name = "Model")]
	[Required]
	[StringLength(50, ErrorMessage = "{0} length can't be more than {1}.")]
	public string Model { get; init; } = "";
	[Display(Name = "Year")]
	[Required]
	public int Year { get; init; } = 0;
	
	public DateTime LastModifiedDate { get; set; }
		
	public IList<StocksViewModel>? StocksList { get; set; }
	
}
