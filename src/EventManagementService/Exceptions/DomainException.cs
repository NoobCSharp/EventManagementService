namespace EventManagementService.Exceptions
{
    public abstract class DomainException : Exception
    {
        public abstract string Title { get; }
        public abstract int StatusCode {  get; }

        protected DomainException(string message) : base(message)
        { 
        }
    }
}
