namespace Domain.Exceptions
{
    public sealed class ActiveBookingLimitExceededException : DomainException
    {
        //Превышен допустимый лимит активных бронирований."
        public ActiveBookingLimitExceededException(string message)
            : base(message, "Conflict")
        {
        }
    }
}
