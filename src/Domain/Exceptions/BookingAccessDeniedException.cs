namespace Domain.Exceptions
{
    public sealed class BookingAccessDeniedException : DomainException
    {
        public BookingAccessDeniedException(string message)
            : base(message, "Forbidden")
        {
        }
    }
}
