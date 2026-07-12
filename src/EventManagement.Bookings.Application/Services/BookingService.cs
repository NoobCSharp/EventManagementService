using EventManagement.Bookings.Application.Dtos;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Mappers;
using EventManagement.Bookings.Application.Options;
using EventManagement.Bookings.Domain.Entities;
using EventManagement.Bookings.Domain.Enums;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Shared.Kafka.Interfaces;
using EventManagement.Shared.Kafka.Messages;
using EventManagement.Shared.Kafka.Topics;
using Microsoft.Extensions.Options;

namespace EventManagement.Bookings.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IKafkaProducer _producer;
        private readonly BookingOptions _bookingOptions;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public BookingService(IBookingRepository bookingRepository, IKafkaProducer producer, IOptions<BookingOptions> bookingSettings)
        {
            _bookingRepository = bookingRepository;
            _producer = producer;
            _bookingOptions = bookingSettings.Value;
        }

        public async Task<BookingDtoResponse> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var existingBooking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

            if (existingBooking is null)
                throw new BookingNotFoundException("Бронирование по указанному идентификатору не найдено!");

            var bookingDtoResponse = BookingMapper.BookingToResponse(existingBooking);

            return bookingDtoResponse;
        }

        public async Task<BookingDtoResponse> CreateBookingAsync(Guid eventId, Guid userId, int seatCount, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();

            try
            {
                var activeBookingsCount = await _bookingRepository.GetActiveBookingsCountAsync(userId, cancellationToken);

                if (activeBookingsCount >= _bookingOptions.ActiveBookingsLimit)
                    throw new ActiveBookingLimitExceededException($"Пользователь не может иметь более {_bookingOptions.ActiveBookingsLimit} активных броней.");

                if (seatCount <= 0)
                    throw new BookingValidationException($"Количество мест для бронирования должно быть больше нуля.");

                var booking = new Booking
                {
                    BookingId = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = userId,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    SeatCount = seatCount,
                    ProcessedAt = null
                };

                await _bookingRepository.CreateBookingAsync(booking, cancellationToken);            
                await _bookingRepository.SaveChangesAsync(cancellationToken);

                await _producer.ProduceAsync(KafkaTopics.BookingCreated, 
                    new BookingCreatedMessage() 
                    {
                         BookingId = booking.BookingId,
                         EventId = booking.EventId,
                         UserId= userId,
                         SeatCount= seatCount,
                         CreatedAt = DateTime.UtcNow
                    },
                    booking.EventId.ToString(),
                    cancellationToken);

                return BookingMapper.BookingToResponse(booking);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid userId, Role role, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

            if (booking is null)
                throw new BookingNotFoundException("Бронирование по указанному идентификатору не найдено!");

            if (role is not Role.Admin && booking.UserId != userId)
                throw new UnauthorizedAccessException("У пользователя не достаточно прав на выполнение данной операции!");

            if (booking.Status is BookingStatus.Cancelled)
                throw new BookingValidationException("Бронь уже отменена!");

            await _producer.ProduceAsync(KafkaTopics.BookingCancelled,
                    new BookingCancelledMessage() 
                    { 
                        BookingId = booking.BookingId, 
                        EventId = booking.EventId,
                        SeatCount = booking.SeatCount,
                        CreatedAt= DateTime.UtcNow,
                    },
                    booking.EventId.ToString(),
                    cancellationToken);
        }
    }
}
