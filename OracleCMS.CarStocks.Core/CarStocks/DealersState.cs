using OracleCMS.Common.Core.Base.Models;
using System.ComponentModel;

namespace OracleCMS.CarStocks.Core.CarStocks;

public record DealersState : BaseEntity
{
	public string DealerName { get; init; } = "";
	public string DealerWebsite { get; init; } = "";
	
	
	public IList<StocksState>? StocksList { get; set; }
	
}
