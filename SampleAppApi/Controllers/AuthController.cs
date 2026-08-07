using Microsoft.AspNetCore.Mvc;

namespace SampleAppApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

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

