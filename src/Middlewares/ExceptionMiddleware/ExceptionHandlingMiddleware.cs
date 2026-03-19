using EventManagementService.Exceptions;
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
            ProblemDetails problem;
            int statusCode;

            switch (ex)
            {
                case NotFoundException notFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    problem = new ProblemDetails
                    {
                        Status = notFoundEx.StatusCode,
                        Title = notFoundEx.Title,
                        Detail = ex.Message
                    };

                    _logger.LogInformation(ex.Message);
                    break;

                case BadRequestException badRequestEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    problem = new ProblemDetails
                    {
                        Status = badRequestEx.StatusCode,
                        Title = badRequestEx.Title,
                        Detail = ex.Message
                    };

                    _logger.LogInformation(ex.Message);
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    problem = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = "Internal server error",
                        Detail = ex.Message
                    };

                    _logger.LogError(ex,
                        "Unhandled exception. Method: {Method}, Path: {Path}\n", context.Request.Method, context.Request.Path);
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(problem);
        }





        //private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        //{
        //    var statusCode = ex switch
        //    {
        //        BadRequestException => StatusCodes.Status400BadRequest,
        //        NotFoundException => StatusCodes.Status404NotFound,
        //        _ => StatusCodes.Status500InternalServerError
        //    };

        //    context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = statusCode;

        //    var problem = new ProblemDetails()
        //    {
        //        Title = "Request processing error",
        //        Status = statusCode,
        //        Detail = ex.Message
        //    };

        //    return context.Response.WriteAsJsonAsync(problem);
        //}
    }
}
