namespace EventManagementService.Exceptions
{
    public sealed class BadRequestException : DomainException
    {
        public BadRequestException(string message)
            : base(message, StatusCodes.Status400BadRequest, "Bad Request") { }
    }
}
