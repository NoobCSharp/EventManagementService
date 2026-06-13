namespace Domain.Exceptions
{
    public sealed class EventAlreadyStartedException : DomainException
    {
        //"Невозможно забронировать прошедшее или уже начавшееся событие."
        public EventAlreadyStartedException(string message)
            : base(message, "Conflict") 
        {
        }
    }
}
