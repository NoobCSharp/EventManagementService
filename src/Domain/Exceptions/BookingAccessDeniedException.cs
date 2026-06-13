namespace Domain.Exceptions
{
    public sealed class BookingAccessDeniedException : DomainException
    {
        //У пользователя нет прав на выполнение данной операции.
        public BookingAccessDeniedException(string message)
            : base(message, "Forbidden")
        {
        }
    }
}
