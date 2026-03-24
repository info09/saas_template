using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaS.API.Extensions;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Features.Auth.Login;
using SaaS.Application.Interfaces;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantService _tenantService;

    public AuthController(IMediator mediator, ITenantService tenantService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(Result<LoginResponse>.Failure(
                "Missing tenant context. Include the 'X-Tenant-Id' header when calling this endpoint."));
        }

        var command = new LoginCommand(request.Email, request.Password, tenantId);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
