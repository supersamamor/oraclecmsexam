using OracleCMS.Common.Core.Base.Models;
using System.ComponentModel;

namespace OracleCMS.CarStocks.Application.DTOs;

public record DealersListDto : BaseDto
{
	public string DealerName { get; init; } = "";
	public string DealerWebsite { get; init; } = "";
	
	
}
