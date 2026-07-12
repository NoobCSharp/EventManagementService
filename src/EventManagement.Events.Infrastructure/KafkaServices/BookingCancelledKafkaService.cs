using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Shared.Kafka.Abstraction;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Messages;
using EventManagement.Shared.Kafka.Topics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventManagement.Events.Infrastructure.KafkaServices
{
    public sealed class BookingCancelledKafkaService : KafkaConsumerBackgroundService<BookingCancelledMessage>
    {
        private readonly IKafkaProducer _producer;
        private readonly IServiceScopeFactory _scopeFactory;

        public BookingCancelledKafkaService(IKafkaConsumer consumer, IKafkaProducer producer, IServiceScopeFactory scopeFactory, ILogger<BookingCancelledKafkaService> logger)
            : base(consumer, logger)
        {
            _producer = producer;
            _scopeFactory = scopeFactory;
        }

        protected override string Topic => KafkaTopics.BookingCancelled;

        protected override async Task HandleAsync(BookingCancelledMessage message, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var existingEvent = await repository.GetEventByIdAsync(message.EventId, cancellationToken);

            if (existingEvent is null)
            {
                await _producer.ProduceAsync(KafkaTopics.BookingCancelledFailed,
                    new BookingCancelledFailedMessage
                    {
                        BookingId = message.BookingId,
                        Reason = "Событие по указанному идентификатору не найдено!",
                        CreatedAt = DateTime.UtcNow
                    },
                    message.EventId.ToString(),
                    cancellationToken);

                return;
            }

            if (existingEvent.StartAt <= DateTime.UtcNow)
            {
                await _producer.ProduceAsync(KafkaTopics.BookingCancelledFailed,
                    new BookingCancelledFailedMessage
                    {
                        BookingId = message.BookingId,
                        Reason = "Невозможно отменить бронь для начавшегося или оконченного событие!",
                        CreatedAt = DateTime.UtcNow

                    },
                    message.EventId.ToString(),
                    cancellationToken);

                return;
            }

            existingEvent.ReleaseSeats(message.SeatCount);

            await repository.SaveChangesAsync(cancellationToken);

            await _producer.ProduceAsync(KafkaTopics.BookingCancelledConfirmed,
                new BookingCancelledConfirmedMessage
                {
                    BookingId = message.BookingId,
                    EventId = message.EventId,
                    CreatedAt = DateTime.UtcNow,
                },
                message.EventId.ToString(),
                cancellationToken);
        }
    }
}
