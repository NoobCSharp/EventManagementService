using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Enums;
using EventManagement.Shared.Kafka.Abstraction;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Messages;
using EventManagement.Shared.Kafka.Topics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventManagement.Bookings.Infrastructure.KafkaServices
{
    public sealed class BookingCancelledFailedKafkaService : KafkaConsumerBackgroundService<BookingCancelledFailedMessage>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingCancelledFailedKafkaService> _logger;

        public BookingCancelledFailedKafkaService(IKafkaConsumer consumer, IServiceScopeFactory scopeFactory, ILogger<BookingCancelledFailedKafkaService> logger)
            : base(consumer, logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override string Topic => KafkaTopics.BookingCancelledFailed;

        protected override async Task HandleAsync(BookingCancelledFailedMessage message, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            var existingBooking = await repository.GetBookingByIdAsync(message.BookingId, cancellationToken);

            if (existingBooking is null)
            {
                _logger.LogWarning(
                   "Бронь {BookingId} не найдена!",
                   message.BookingId);

                return;
            }

            if (existingBooking.Status == BookingStatus.Cancelled)
            {
                _logger.LogInformation(
                    "Бронь {BookingId} уже отменена!",
                    message.BookingId);

                return;
            }

            // TODO что тут делать по идее на фронт пользователю надо отдать сообщение почему не отменить!
        }
    }
}
