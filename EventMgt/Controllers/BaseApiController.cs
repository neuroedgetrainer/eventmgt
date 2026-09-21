using EventMgt.Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMgt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected int CurrentUserId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(idClaim, out var id) ? id : 0;
            }
        }

        protected string CurrentUserEmail =>
            User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        protected string CurrentUserRole =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        protected bool IsAdmin =>
            User.IsInRole("Admin");

        protected bool IsOrganizer =>
            User.IsInRole("Organizer");

        protected IActionResult HandleResult<T>(ServiceResult<T> result)
        {
            if (result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    201 => StatusCode(201, result.Data),
                    204 => NoContent(),
                    _ => Ok(result.Data)
                };
            }

            return result.StatusCode switch
            {
                400 => BadRequest(new { message = result.ErrorMessage }),
                401 => Unauthorized(new { message = result.ErrorMessage }),
                403 => Forbid(),
                404 => NotFound(new { message = result.ErrorMessage }),
                409 => Conflict(new { message = result.ErrorMessage }),
                _ => StatusCode(result.StatusCode, new { message = result.ErrorMessage })
            };
        }
    }
}
