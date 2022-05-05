using Domain.Exceptions;
using Microsoft.Graph;

namespace Web.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                await CatchException(e, context);
            }
        }

        private async Task CatchException(Exception e, HttpContext context)
        {
            context.Response.ContentType = "application/json";
            if (e.GetType() == typeof(NotFoundException))
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            else if (e.GetType() == typeof(BadRequestException))
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            else if (e.GetType() == typeof(ConflictException))
                context.Response.StatusCode = StatusCodes.Status409Conflict;
            else if (e.GetType() == typeof(ServiceException))
            {
                await HandleGraphApiException(e as ServiceException, context);
                return;
            }
            else
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsync(e.Message);
        }

        private async Task HandleGraphApiException(ServiceException e, HttpContext context)
        {
            switch (e.Error.Code)
            {
                case "Request_BadRequest":
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                }
                default:
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                }
            }

            await context.Response.WriteAsJsonAsync(e.Error);

        }
    }
    public static class ExceptionHandlingMiddlewareExtension
    {
        public static IApplicationBuilder AddExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}


