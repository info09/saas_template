using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaS.API.Extensions;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Features.Auth.Login;
using SaaS.Application.Features.Auth.Logout;
using SaaS.Application.Features.Auth.LogoutSession;
using SaaS.Application.Features.Auth.Profile;
using SaaS.Application.Features.Auth.RefreshToken;
using SaaS.Application.Features.Auth.Sessions;
using SaaS.Application.Interfaces;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IMediator mediator, ITenantService tenantService, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing tenant context.",
                Detail = "Include the 'X-Tenant-Id' header when calling this endpoint."
            });
        }

        var command = new LoginCommand(request.Email, request.Password, tenantId);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(Result<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<UserProfileResponse>>> GetProfile()
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId) || string.IsNullOrWhiteSpace(_currentUserService.TenantId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing user context.",
                Detail = "The authenticated user context could not be resolved from the current request."
            });
        }

        var result = await _mediator.Send(new GetProfileQuery(_currentUserService.UserId, _currentUserService.TenantId));
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<UserSessionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IReadOnlyList<UserSessionResponse>>>> GetSessions()
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing user context.",
                Detail = "The authenticated user context could not be resolved from the current request."
            });
        }

        var result = await _mediator.Send(new GetSessionsQuery(_currentUserService.UserId, _currentUserService.SessionId));
        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<LoginResponse>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing tenant context.",
                Detail = "Include the 'X-Tenant-Id' header when calling this endpoint."
            });
        }

        var command = new RefreshTokenCommand(request.RefreshToken, tenantId);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("logout")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing tenant context.",
                Detail = "Include the 'X-Tenant-Id' header when calling this endpoint."
            });
        }

        var command = new LogoutCommand(request.RefreshToken, tenantId);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpPost("logout-session")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> LogoutSession([FromBody] LogoutSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing user context.",
                Detail = "The authenticated user context could not be resolved from the current request."
            });
        }

        var command = new LogoutSessionCommand(_currentUserService.UserId, request.SessionId);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
