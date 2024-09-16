using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Queries;
using OracleCMS.CarStocks.Core.CarStocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using OracleCMS.Common.API.Controllers;
using Asp.Versioning;

namespace OracleCMS.CarStocks.API.Controllers.v1;

[ApiVersion("1.0")]
public class DealersController : BaseApiController<DealersController>
{
    [Authorize(Policy = Permission.Dealers.View)]
    [HttpGet]
    public async Task<ActionResult<PagedListResponse<DealersState>>> GetAsync([FromQuery] GetDealersQuery query) =>
        Ok(await Mediator.Send(query));

    [Authorize(Policy = Permission.Dealers.View)]
    [HttpGet("{id}")]
    public async Task<ActionResult<DealersState>> GetAsync(string id) =>
        await ToActionResult(async () => await Mediator.Send(new GetDealersByIdQuery(id)));

    [Authorize(Policy = Permission.Dealers.Create)]
    [HttpPost]
    public async Task<ActionResult<DealersState>> PostAsync([FromBody] DealersViewModel request) =>
        await ToActionResult(async () => await Mediator.Send(Mapper.Map<AddDealersCommand>(request)));

    [Authorize(Policy = Permission.Dealers.Edit)]
    [HttpPut("{id}")]
    public async Task<ActionResult<DealersState>> PutAsync(string id, [FromBody] DealersViewModel request)
    {
        var command = Mapper.Map<EditDealersCommand>(request);
        return await ToActionResult(async () => await Mediator.Send(command with { Id = id }));
    }

    [Authorize(Policy = Permission.Dealers.Delete)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<DealersState>> DeleteAsync(string id) =>
        await ToActionResult(async () => await Mediator.Send(new DeleteDealersCommand { Id = id }));
}

public record DealersViewModel
{
    [Required]
	[StringLength(100, ErrorMessage = "{0} length can't be more than {1}.")]
	public string DealerName { get;set; } = "";
	[Required]
	[StringLength(255, ErrorMessage = "{0} length can't be more than {1}.")]
	public string DealerWebsite { get;set; } = "";
	   
}
