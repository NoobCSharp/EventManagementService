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
    public sealed class BookingCancelledConfirmedKafkaService : KafkaConsumerBackgroundService<BookingCancelledConfirmedMessage>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingCancelledConfirmedKafkaService> _logger;

        public BookingCancelledConfirmedKafkaService(IKafkaConsumer consumer, IServiceScopeFactory scopeFactory, ILogger<BookingCancelledConfirmedKafkaService> logger)
            : base(consumer, logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override string Topic => KafkaTopics.BookingCancelledConfirmed;

        protected override async Task HandleAsync(BookingCancelledConfirmedMessage message, CancellationToken cancellationToken = default)
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

            existingBooking.Cancel(DateTime.UtcNow);

            await repository.SaveChangesAsync(cancellationToken);
        }
    }
}
