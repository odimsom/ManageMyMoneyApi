using System.Security.Claims;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Presentation.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace ManageMyMoney.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
    protected string? CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email);
    protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    protected IActionResult HandleResult<T>(OperationResult<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value!, successMessage));

        return BadRequest(ApiResponse<T>.Fail(result.Error));
    }

    protected IActionResult HandleResult(OperationResult result, string? successMessage = null)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse.Ok(successMessage));

        return BadRequest(ApiResponse.Fail(result.Error));
    }

    protected IActionResult HandleCreated<T>(OperationResult<T> result, string actionName, object routeValues)
    {
        if (result.IsSuccess)
            return CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(result.Value!));

        return BadRequest(ApiResponse<T>.Fail(result.Error));
    }

    protected IActionResult HandleNotFound<T>(OperationResult<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value!));

        return NotFound(ApiResponse<T>.Fail(result.Error));
    }
}
