namespace EventManagementService.Exceptions
{
    public class BadRequestException : Exception
    {
        public string Title => "Bad request";
        public int StatusCode => StatusCodes.Status400BadRequest;

        public BadRequestException(string message) : base(message)
        {
        }
    }
}
