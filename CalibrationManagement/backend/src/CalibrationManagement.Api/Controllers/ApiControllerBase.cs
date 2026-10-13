using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CalibrationManagement.Application.Common;

namespace CalibrationManagement.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Id of the signed-in user taken from the validated JWT.</summary>
    protected int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedAppException("The token does not contain a valid user id.");
}
