using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Shared.Kafka.Abstraction;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Messages;
using EventManagement.Shared.Kafka.Topics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventManagement.Events.Infrastructure.KafkaServices
{
    public sealed class BookingCreatedKafkaService : KafkaConsumerBackgroundService<BookingCreatedMessage>
    {
        private readonly IKafkaProducer _producer;
        private readonly IServiceScopeFactory _scopeFactory;

        public BookingCreatedKafkaService(IKafkaConsumer consumer, IKafkaProducer producer, IServiceScopeFactory scopeFactory, ILogger<BookingCreatedKafkaService> logger)
            : base(consumer, logger)
        {
            _producer = producer;
            _scopeFactory = scopeFactory;
        }

        protected override string Topic => KafkaTopics.BookingCreated;

        protected override async Task HandleAsync(BookingCreatedMessage message, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var existingEvent = await repository.GetEventByIdAsync(message.EventId, cancellationToken);

            if (existingEvent is null)
            {
                await _producer.ProduceAsync(KafkaTopics.BookingCreatedFailed,
                    new BookingCreatedFailedMessage()
                    {
                        BookingId = message.BookingId,
                        Reason = "Событие по указанному идентификатору не найдено!",
                        CreatedAt = DateTime.UtcNow,
                    },
                    message.EventId.ToString(),
                    cancellationToken);

                return;
            }

            if (existingEvent.StartAt <= DateTime.UtcNow)
            {
                await _producer.ProduceAsync(KafkaTopics.BookingCreatedFailed,
                    new BookingCreatedFailedMessage()
                    {
                        BookingId = message.BookingId,
                        Reason = "Невозможно забронировать начавшееся или оконченное событие!",
                        CreatedAt = DateTime.UtcNow,
                    },
                    message.EventId.ToString(),
                    cancellationToken);

                return;
            }

            if (!existingEvent.TryReserveSeats(message.SeatCount))
            {
                await _producer.ProduceAsync(KafkaTopics.BookingCreatedFailed,
                    new BookingCreatedFailedMessage()
                    {
                        BookingId = message.BookingId,
                        Reason = "Нет достаточного количества свободных мест для бронирования на данное событие!",
                        CreatedAt = DateTime.UtcNow,
                    },
                    message.EventId.ToString(),
                    cancellationToken);

                return;
            }

            await repository.SaveChangesAsync(cancellationToken);

            await _producer.ProduceAsync(KafkaTopics.BookingCreatedConfirmed,
                new BookingCreatedConfirmedMessage()
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
