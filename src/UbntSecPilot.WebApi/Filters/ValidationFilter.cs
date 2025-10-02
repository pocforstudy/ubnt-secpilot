using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace UbntSecPilot.WebApi.Filters
{
    /// <summary>
    /// Global validation filter for API requests
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(e => e.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                var result = new
                {
                    success = false,
                    message = "Validation failed",
                    errors = errors
                };

                context.Result = new BadRequestObjectResult(result);
                return;
            }

            await next();
        }
    }

    /// <summary>
    /// API Response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }

        public ApiResponse(T data, string message = null)
        {
            Success = true;
            Data = data;
            Message = message;
            Errors = new string[0];
        }

        public ApiResponse(string message, string[] errors)
        {
            Success = false;
            Data = default;
            Message = message;
            Errors = errors ?? new string[0];
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = null)
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse<T> ErrorResponse(string message, string[] errors = null)
        {
            return new ApiResponse<T>(message, errors);
        }
    }

    /// <summary>
    /// API Controller base class with common functionality
    /// </summary>
    public class ApiControllerBase : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string message = null)
        {
            return Ok(ApiResponse<T>.SuccessResponse(data, message));
        }

        protected ActionResult<ApiResponse<T>> ErrorResponse<T>(string message, string[] errors = null)
        {
            return BadRequest(ApiResponse<T>.ErrorResponse(message, errors));
        }

        protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message = "Resource not found")
        {
            return NotFound(ApiResponse<T>.ErrorResponse(message));
        }

        protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(string message = "Unauthorized")
        {
            return Unauthorized(ApiResponse<T>.ErrorResponse(message));
        }

        protected ActionResult<ApiResponse<T>> ForbiddenResponse<T>(string message = "Forbidden")
        {
            return Forbid(ApiResponse<T>.ErrorResponse(message));
        }
    }
}
