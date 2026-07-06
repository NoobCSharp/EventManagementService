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
    public sealed class BookingCreatedFailedKafkaService : KafkaConsumerBackgroundService<BookingCreatedFailedMessage>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingCreatedFailedKafkaService> _logger;

        public BookingCreatedFailedKafkaService(IKafkaConsumer consumer, IServiceScopeFactory scopeFactory, ILogger<BookingCreatedFailedKafkaService> logger)
            : base(consumer, logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override string Topic => KafkaTopics.BookingCreatedFailed;

        protected override async Task HandleAsync(BookingCreatedFailedMessage message, CancellationToken cancellationToken = default)
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

            if (existingBooking.Status == BookingStatus.Rejected)
            {
                _logger.LogInformation(
                    "Бронь {BookingId} уже отклонена!",
                    message.BookingId);

                return;
            }

            existingBooking.Reject(message.CreatedAt);

            await repository.SaveChangesAsync(cancellationToken);
        }
    }
}
