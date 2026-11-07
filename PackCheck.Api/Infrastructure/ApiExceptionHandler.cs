using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PackCheck.Api.Infrastructure;

public class ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception is ApiException apiException ? apiException.StatusCode : StatusCodes.Status500InternalServerError;
        var detail = exception is ApiException ? exception.Message : "The server could not complete this request. Please try again.";
        if (status >= 500)
            logger.LogError(exception, "Request failed with status {StatusCode}", status);

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = status switch
                {
                    400 => "Invalid request",
                    404 => "Not found",
                    413 => "Image too large",
                    415 => "Unsupported image format",
                    502 => "Image recognition failed",
                    503 => "Service unavailable",
                    504 => "Image recognition timed out",
                    _ => "Server error"
                },
                Detail = detail
            }
        });
    }
}
