using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Auth;
using SaaS.Application.Common.Models;

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

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<LoginResponse>>> Login(LoginCommand command)
    {
        var response = await _mediator.Send(command);

        if (!response.Succeeded)
        {
            return Unauthorized(response.Errors);
        }

        return Ok(response);
    }
}
