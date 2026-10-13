using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Exchange credentials for a short-lived JWT. There is deliberately no public registration endpoint.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
        => Ok(await _auth.LoginAsync(request, ct));

    [HttpGet("me")]
    [Authorize(Policy = PolicyNames.Read)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct) => Ok(await _auth.GetProfileAsync(CurrentUserId, ct));

    [HttpPut("profile")]
    [Authorize(Policy = PolicyNames.Read)]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
        => Ok(await _auth.UpdateProfileAsync(CurrentUserId, request, ct));

    [HttpPost("change-password")]
    [Authorize(Policy = PolicyNames.Read)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _auth.ChangePasswordAsync(CurrentUserId, request, ct);
        return NoContent();
    }
}
