namespace EventManagementService.Exceptions
{
    public class NotFoundException : Exception
    {
        public string Title => "Resource not found";
        public int StatusCode => StatusCodes.Status404NotFound;

        public NotFoundException(string message) : base(message)
        {
            
        }
    }
}
