using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaS.API.Extensions;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Tenants;
using SaaS.Application.Features.Tenants.Commands.CreateTenant;
using SaaS.Application.Interfaces;

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
    public async Task<ActionResult<Result<string>>> CreateTenant([FromBody] CreateTenantRequest request)
    {
        // This endpoint is for demonstration. In a real app, tenant creation would likely be an admin-only operation.
        var command = new CreateTenantCommand
        {
            Name = request.Name,
            AdminEmail = request.AdminEmail,
            AdminPassword = request.AdminPassword
        };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
