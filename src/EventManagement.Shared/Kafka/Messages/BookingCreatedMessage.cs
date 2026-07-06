namespace EventManagement.Shared.Kafka.Messages
{
    public sealed class BookingCreatedMessage
    {
        public required Guid BookingId { get; init; }

        public required Guid EventId { get; init; }

        public required Guid UserId { get; init; }

        public required DateTime CreatedAt { get; init; }
    }
}
