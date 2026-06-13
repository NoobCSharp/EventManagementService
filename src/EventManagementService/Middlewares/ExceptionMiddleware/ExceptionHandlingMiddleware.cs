using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Middlewares.ExceptionMiddleware
{
    public class ExceptionHandlingMiddleware
    {
        /// <summary>
        /// Делегат, представляющий следующий middleware в конвейере обработки.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Стандартный интерфейс логирования.
        /// </summary>
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Централизованный перехватчик исключений в HTTP-pipeline,
        /// формирует единый корректный HTTP-ответ в формате ProblemDetails.
        /// </summary>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {           
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                //Проверяем, отправлялся ли уже ответ клиенту.
                //Если да — менять его нельзя, выходим.
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, cannot handle exception: {Message}", ex.Message);

                    //Middleware не может обработать ответ, поэтому пробрасывает исключение дальше.
                    //Клиент, вероятно, получит неполный ответ или стандартный 500.
                    throw;
                }

                await HandleExceptionAsync(context, ex);
            }
        }


        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            int statusCode = ex switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                NoAvailableSeatsException => StatusCodes.Status409Conflict,
                DomainException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            ProblemDetails problem;

            if (ex is DomainException domainEx)
            {
                problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = domainEx.Title,
                    Detail = domainEx.Message
                };

                _logger.LogWarning(ex.Message, "Domain exception occurred");
            }
            else
            {
                problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = "Internal server error",
                    Detail = ex.Message
                };

                _logger.LogError(ex,
                    "Unhandled exception. Method: {Method}, Path: {Path}\n", context.Request.Method, context.Request.Path);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
