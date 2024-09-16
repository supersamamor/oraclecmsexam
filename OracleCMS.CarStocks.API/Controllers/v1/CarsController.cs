using OracleCMS.Common.Utility.Models;
using OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Queries;
using OracleCMS.CarStocks.Core.CarStocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using OracleCMS.Common.API.Controllers;
using Asp.Versioning;

namespace OracleCMS.CarStocks.API.Controllers.v1;

[ApiVersion("1.0")]
public class CarsController : BaseApiController<CarsController>
{
    [Authorize(Policy = Permission.Cars.View)]
    [HttpGet]
    public async Task<ActionResult<PagedListResponse<CarsState>>> GetAsync([FromQuery] GetCarsQuery query) =>
        Ok(await Mediator.Send(query));

    [Authorize(Policy = Permission.Cars.View)]
    [HttpGet("{id}")]
    public async Task<ActionResult<CarsState>> GetAsync(string id) =>
        await ToActionResult(async () => await Mediator.Send(new GetCarsByIdQuery(id)));

    [Authorize(Policy = Permission.Cars.Create)]
    [HttpPost]
    public async Task<ActionResult<CarsState>> PostAsync([FromBody] CarsViewModel request) =>
        await ToActionResult(async () => await Mediator.Send(Mapper.Map<AddCarsCommand>(request)));

    [Authorize(Policy = Permission.Cars.Edit)]
    [HttpPut("{id}")]
    public async Task<ActionResult<CarsState>> PutAsync(string id, [FromBody] CarsViewModel request)
    {
        var command = Mapper.Map<EditCarsCommand>(request);
        return await ToActionResult(async () => await Mediator.Send(command with { Id = id }));
    }

    [Authorize(Policy = Permission.Cars.Delete)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<CarsState>> DeleteAsync(string id) =>
        await ToActionResult(async () => await Mediator.Send(new DeleteCarsCommand { Id = id }));
}

public record CarsViewModel
{
    [Required]
	[StringLength(50, ErrorMessage = "{0} length can't be more than {1}.")]
	public string Make { get;set; } = "";
	[Required]
	[StringLength(50, ErrorMessage = "{0} length can't be more than {1}.")]
	public string Model { get;set; } = "";
	[Required]
	public int Year { get;set; } = 0;
	   
}
