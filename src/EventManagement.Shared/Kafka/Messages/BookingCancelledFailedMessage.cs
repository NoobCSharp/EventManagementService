namespace EventManagement.Shared.Kafka.Messages
{
    public sealed class BookingCancelledFailedMessage
    {
        public required Guid BookingId { get; init; }

        public required string Reason { get; init; }

        public required DateTime CreatedAt { get; set; }
    }
}
