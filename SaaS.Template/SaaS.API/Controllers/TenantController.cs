using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Interfaces;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
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
}
