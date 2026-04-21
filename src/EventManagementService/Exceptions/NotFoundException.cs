namespace EventManagementService.Exceptions
{
    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(string message)
            : base(message, StatusCodes.Status404NotFound, "Resource not found") { }
    }
}
