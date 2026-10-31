using Domain.Exceptions;

namespace Web.Middlewares;

public class GlobalExceptionHandlingMiddleware: IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (AppValidationException ex)
        {
            _logger.LogWarning("Error de negocio [{Code}]: {Message}", ex.ErrorCode, ex.Message);

            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message, code = ex.ErrorCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en el servidor");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Ocurrió un error inesperado en el servidor.", code = "INTERNAL_SERVER_ERROR" });
        }
    }
}
