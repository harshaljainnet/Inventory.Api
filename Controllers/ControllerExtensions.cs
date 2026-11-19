using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers
{
    public static class ControllerExtensions
    {
        // Maps a failed service result to 404 / 400 / 409. Only call when result.IsSuccess is false.
        public static ActionResult Failure(this ControllerBase controller, ServiceResult result)
        {
            return result.Status switch
            {
                ServiceStatus.NotFound => controller.NotFound(),
                ServiceStatus.Invalid => controller.BadRequest(Problem(result, "Invalid request", StatusCodes.Status400BadRequest)),
                ServiceStatus.Conflict => controller.Conflict(Problem(result, "Conflict", StatusCodes.Status409Conflict)),
                _ => throw new InvalidOperationException("Result was successful.")
            };
        }

        private static ProblemDetails Problem(ServiceResult result, string defaultTitle, int status) => new()
        {
            Title = result.Title ?? defaultTitle,
            Detail = result.Error,
            Status = status
        };
    }
}
