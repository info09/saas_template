using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Interfaces;
using SaaS.Application.Tenants.Commands.CreateTenant;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IMediator _mediator;

    public TenantController(ITenantService tenantService, IMediator mediator)
    {
        _tenantService = tenantService;
        _mediator = mediator;
    }

    [HttpGet("current")]
    public IActionResult GetCurrentTenant()
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        
        if (string.IsNullOrEmpty(tenantId))
        {
            return BadRequest("No tenant identified in the current request context (Missing X-Tenant-Id header).");
        }

        return Ok(new { TenantId = tenantId });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant(CreateTenantCommand command)
    {
        var tenantId = await _mediator.Send(command);
        return Ok(new { message = "Tenant created and provisioned successfully", tenantId });
    }
}
