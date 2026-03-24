using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaS.API.Extensions;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Features.Auth.Login;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password, request.TenantId);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
