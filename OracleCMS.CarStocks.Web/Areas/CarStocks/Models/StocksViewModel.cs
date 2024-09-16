using OracleCMS.Common.Web.Utility.Extensions;
using OracleCMS.CarStocks.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using OracleCMS.Common.Web.Utility.Annotations;

namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Models;

public record StocksViewModel : BaseViewModel
{	
	[Display(Name = "Car ID")]
	[Required]
	
	public string CarID { get; init; } = "";
	public string?  ReferenceFieldCarID { get; set; }
	[Display(Name = "Dealer ID")]
	[Required]
	
	public string DealerID { get; init; } = "";
	public string?  ReferenceFieldDealerID { get; set; }
	[Display(Name = "Quantity")]
	[Required]
	public int Quantity { get; init; } = 0;
	
	public DateTime LastModifiedDate { get; set; }
	public CarsViewModel? Cars { get; init; }
	public DealersViewModel? Dealers { get; init; }
		
	
}
