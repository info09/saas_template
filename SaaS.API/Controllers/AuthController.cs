using Microsoft.AspNetCore.Mvc;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login()
    {
        // Minimal placeholder for JWT generation. 
        // In a real app, you would validate credentials using UserManager and generate a real JWT.
        return Ok(new { Token = "mock_jwt_token_for_template" });
    }
}
