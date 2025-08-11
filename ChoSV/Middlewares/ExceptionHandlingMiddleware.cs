using System.Net;
using System.Text.Json;

namespace ChoSV.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Lỗi hệ thống: " + ex.Message);
            }
        }

        public static Task HandleExceptionAsync(HttpContext context, HttpStatusCode code, string errorMessage)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var result = JsonSerializer.Serialize(new
            {
                success = false,
                error = errorMessage
            });
            return context.Response.WriteAsync(result);
        }
    }
}
