namespace EventManagement.Events.Api.Middlewares
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(
            this IApplicationBuilder builder) 
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
