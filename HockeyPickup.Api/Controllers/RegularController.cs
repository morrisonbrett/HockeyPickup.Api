using System.ComponentModel;
using HockeyPickup.Api.Helpers;
using HockeyPickup.Api.Models.Requests;
using HockeyPickup.Api.Models.Responses;
using HockeyPickup.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HockeyPickup.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public class RegularController : ControllerBase
{
    private readonly IRegularService _regularService;
    private readonly ILogger<AuthController> _logger;

    public RegularController(ILogger<AuthController> logger, IRegularService regularService)
    {
        _logger = logger;
        _regularService = regularService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("duplicate-regular-set")]
    [Description("Duplicates a Regular Set")]
    [Produces(typeof(ApiDataResponse<RegularSetDetailedResponse>))]
    [ProducesResponseType(typeof(ApiDataResponse<RegularSetDetailedResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiDataResponse<RegularSetDetailedResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiDataResponse<RegularSetDetailedResponse>>> DuplicateRegularSet([FromBody] DuplicateRegularSetRequest duplicateRegularSetRequest)
    {
        var result = await _regularService.DuplicateRegularSet(duplicateRegularSetRequest);
        var response = ApiDataResponse<RegularSetDetailedResponse>.FromServiceResult(result);
        return result.IsSuccess ? CreatedAtAction(nameof(DuplicateRegularSet), new { id = result.Data.RegularSetId }, response) : BadRequest(response);
    }
}
