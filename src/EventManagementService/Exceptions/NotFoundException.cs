namespace EventManagementService.Exceptions
{
    public sealed class NotFoundException : DomainException
    {
        public override string Title => "Resource not found";
        public override int StatusCode => StatusCodes.Status404NotFound;

        public NotFoundException(string message) : base(message)
        { 
        }
    }
}
