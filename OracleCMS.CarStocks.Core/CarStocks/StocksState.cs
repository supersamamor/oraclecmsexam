using OracleCMS.Common.Core.Base.Models;
using System.ComponentModel;

namespace OracleCMS.CarStocks.Core.CarStocks;

public record StocksState : BaseEntity
{
	public string CarID { get; init; } = "";
	public string DealerID { get; init; } = "";
	public int Quantity { get; init; }
	
	public CarsState? Cars { get; init; }
	public DealersState? Dealers { get; init; }
	
	
}
