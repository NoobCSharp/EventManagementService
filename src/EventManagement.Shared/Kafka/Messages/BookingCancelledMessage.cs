namespace EventManagement.Shared.Kafka.Messages
{
    public sealed class BookingCancelledMessage
    {
        public required Guid BookingId { get; init; }

        public required Guid EventId { get; init; }

        public required DateTime CreatedAt { get; set; }
    }
}
