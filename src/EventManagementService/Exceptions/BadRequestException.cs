namespace EventManagementService.Exceptions
{
    public sealed class BadRequestException : DomainException
    {
        public override string Title => "Bad request";
        public override int StatusCode => StatusCodes.Status400BadRequest;

        public BadRequestException(string message) : base(message)
        {
        }
    }
}
