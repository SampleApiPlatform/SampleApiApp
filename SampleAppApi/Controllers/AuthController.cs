using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenAcquisition tokenAcquisition, ILogger<AuthController> logger)
    {
        _tokenAcquisition = tokenAcquisition;
        _logger = logger;
    }

    // GET /auth/login
    // This triggers the Microsoft login redirect. After login, the token is stored
    // in the user's session. The client then calls /auth/token to retrieve it.
    [HttpGet("login")]
    public IActionResult Login()
    {
        // Challenge the user to authenticate via Microsoft
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/auth/callback"  // after login, redirect here
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    // GET /auth/callback
    // Microsoft redirects here after successful login. The cookie is now set.
    [HttpGet("callback")]
    public IActionResult Callback()
    {
        return Ok(new { message = "Login successful. Now call GET /auth/token to get your JWT." });
    }

    // GET /auth/token
    // Returns the access token as JSON for the console client to copy.
    [HttpGet("token")]
    [Authorize]  // User must be logged in first
    public async Task<IActionResult> GetToken()
    {
        try
        {
            // Request an access token for your API (same audience)
            //var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(
            //    scopes: new[] { "api://f9a3d163-e6e5-481b-9ea9-869076084bc7/access_as_user" });
            
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            return Ok(new
            {
                access_token = accessToken,
                token_type = "Bearer",
                expires_in = 3600
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire token");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET /auth/whoami  → DIAGNOSTIC: shows what is in the auth cookie
    [HttpGet("whoami")]
    [AllowAnonymous]
    public async Task<IActionResult> WhoAmI()
    {
        var keys = new[] { "access_token", "id_token", "refresh_token" };
        var result = new Dictionary<string, string>();

        foreach (var key in keys)
        {
            var value = await HttpContext.GetTokenAsync(key);
            result[key] = string.IsNullOrEmpty(value) ? "MISSING" : $"present (len={value.Length})";
        }

        result["authenticated"] = User.Identity?.IsAuthenticated == true ? "yes" : "no";
        result["name"] = User.Identity?.Name ?? "(none)";

        return Ok(result);
    }
}