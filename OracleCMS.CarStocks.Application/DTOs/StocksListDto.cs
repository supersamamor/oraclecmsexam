using OracleCMS.Common.Core.Base.Models;
using System.ComponentModel;

namespace OracleCMS.CarStocks.Application.DTOs;

public record StocksListDto : BaseDto
{
    public string CarMake { get; init; } = "";
    public string CarModel { get; init; } = "";
	public string DealerName { get; init; } = "";
	public int Quantity { get; init; }
	public string QuantityFormatted { get { return this.Quantity.ToString("#,##0"); } }
	
	
}
