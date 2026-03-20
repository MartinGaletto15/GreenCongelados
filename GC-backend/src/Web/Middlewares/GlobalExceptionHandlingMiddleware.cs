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
            _logger.LogError(ex, ex.Message);
            
            if (ex.ErrorCode == "PRODUCT_CATEGORY_CONFLICT")
            {
                context.Response.StatusCode = 409; // Conflict
            }
            else
            {
                context.Response.StatusCode = 400; // Bad Request
            }
            
            await context.Response.WriteAsJsonAsync(new { error = ex.Message, code = ex.ErrorCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            context.Response.StatusCode = 500; // Internal Server Error
            await context.Response.WriteAsJsonAsync(new { error = "Ocurrió un error inesperado en el servidor." });
        }
    }
}
