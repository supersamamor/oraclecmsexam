using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Queries;
using OracleCMS.CarStocks.Core.CarStocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using OracleCMS.Common.API.Controllers;
using Asp.Versioning;

namespace OracleCMS.CarStocks.API.Controllers.v1;

[ApiVersion("1.0")]
public class StocksController : BaseApiController<StocksController>
{
    [Authorize(Policy = Permission.Stocks.View)]
    [HttpGet]
    public async Task<ActionResult<PagedListResponse<StocksState>>> GetAsync([FromQuery] GetStocksQuery query) =>
        Ok(await Mediator.Send(query));


    [Authorize(Policy = Permission.Stocks.View)]
    [HttpGet("{id}")]
    public async Task<ActionResult<StocksState>> GetAsync(string id) =>
        await ToActionResult(async () => await Mediator.Send(new GetStocksByIdQuery(id)));

    [Authorize(Policy = Permission.Stocks.Create)]
    [HttpPost]
    public async Task<ActionResult<StocksState>> PostAsync([FromBody] StocksViewModel request) =>
        await ToActionResult(async () => await Mediator.Send(Mapper.Map<AddStocksCommand>(request)));

    [Authorize(Policy = Permission.Stocks.Edit)]
    [HttpPut("{id}")]
    public async Task<ActionResult<StocksState>> PutAsync(string id, [FromBody] StocksViewModel request)
    {
        var command = Mapper.Map<EditStocksCommand>(request);
        return await ToActionResult(async () => await Mediator.Send(command with { Id = id }));
    }

    [Authorize(Policy = Permission.Stocks.Delete)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<StocksState>> DeleteAsync(string id) =>
        await ToActionResult(async () => await Mediator.Send(new DeleteStocksCommand { Id = id }));
}

public record StocksViewModel
{
    [Required]

    public string CarID { get; set; } = "";
    [Required]

    public string DealerID { get; set; } = "";
    [Required]
    public int Quantity { get; set; } = 0;

}
