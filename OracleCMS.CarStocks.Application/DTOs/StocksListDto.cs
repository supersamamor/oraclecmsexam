using OracleCMS.Common.Core.Base.Models;
using System.ComponentModel;

namespace OracleCMS.CarStocks.Application.DTOs;

public record StocksListDto : BaseDto
{
	public string CarID { get; init; } = "";
	public string DealerID { get; init; } = "";
	public int Quantity { get; init; }
	public string QuantityFormatted { get { return this.Quantity.ToString("#,##0"); } }
	
	
}
