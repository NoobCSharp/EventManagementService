namespace EventManagement.Shared.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Title { get; }

        protected DomainException(string message, string title) : base(message)
        {
            Title = title;
        }
    }
}
