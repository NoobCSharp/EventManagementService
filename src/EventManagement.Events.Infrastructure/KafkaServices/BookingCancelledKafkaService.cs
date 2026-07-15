using EventManagement.Events.Application.Caching;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Events.Infrastructure.Mappers;
using EventManagement.Shared.Kafka.Abstraction;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Messages;
using EventManagement.Shared.Kafka.Topics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventManagement.Events.Infrastructure.KafkaServices
{
    public sealed class BookingCancelledKafkaService : KafkaConsumerBackgroundService<BookingCancelledMessage>
    {
        private readonly IKafkaProducer _producer;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly CacheTtlOptions _options;

        public BookingCancelledKafkaService(IKafkaConsumer consumer, IKafkaProducer producer, IServiceScopeFactory scopeFactory, IOptions<CacheTtlOptions> options, ILogger<BookingCancelledKafkaService> logger)
            : base(consumer, logger)
        {
            _producer = producer;
            _scopeFactory = scopeFactory;

            _options = options.Value;
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

            var value = EventMapper.EventToResponse(existingEvent);

            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            await cacheService.SetAsync(CacheKeys.Event(existingEvent.EventId), value, TimeSpan.FromMinutes(_options.EventMinutes), cancellationToken);
        }
    }
}
