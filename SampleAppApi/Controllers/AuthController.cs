using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleAppApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    [HttpGet("whoami")]
    [Authorize]   // kein Policy -> loest den OIDC-Redirect aus
    public IActionResult WhoAmI()
    {
        return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
    }

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var identityUrl = _config["ServiceUrls:IdentityApi"];
        return Redirect($"{identityUrl}/login");
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        var identityUrl = _config["ServiceUrls:IdentityApi"];
        return Redirect($"{identityUrl}/register");
    }
}

