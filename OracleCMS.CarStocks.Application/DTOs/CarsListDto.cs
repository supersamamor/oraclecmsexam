using OracleCMS.Common.Core.Base.Models;
using System.ComponentModel;

namespace OracleCMS.CarStocks.Application.DTOs;

public record CarsListDto : BaseDto
{
	public string Make { get; init; } = "";
	public string Model { get; init; } = "";
	public int Year { get; init; }
	public string YearFormatted { get { return this.Year.ToString("#,##0"); } }
	
	public string StatusBadge { get; set; } = "";
}
