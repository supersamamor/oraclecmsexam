using OracleCMS.Common.Core.Base.Models;
using System.ComponentModel;

namespace OracleCMS.CarStocks.Core.CarStocks;

public record CarsState : BaseEntity
{
	public string Make { get; init; } = "";
	public string Model { get; init; } = "";
	public int Year { get; init; }
	
	
	public IList<StocksState>? StocksList { get; set; }
	
}
