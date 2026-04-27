using EventManagementService.DataAccess;
using EventManagementService.Dtos.BookingDtos;
using EventManagementService.Enums;
using EventManagementService.Exceptions;
using EventManagementService.Mappers;
using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _appDbContext;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        
        public BookingService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// Создает бронь для указанного события по Id.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор события, к которому относится бронь.
        /// </param>
        /// <returns>
        /// Объект брони.
        /// </returns>
        /// <remarks>
        /// Если событие не найдено, бросает исключение NotFoundException.
        /// Если нет доступных мест на событие, бросает исключение NoAvailableSeatsException
        /// </remarks>
        public async Task<BookingDtoResponse> CreateBookingAsync(Guid id, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var existingEvent = await _appDbContext.Events
                    .FirstOrDefaultAsync(e => e.EventId == id, cancellationToken);

                if (existingEvent is null)
                    throw new NotFoundException("Событие по указанному идентификатору не найдено!");

                if (!existingEvent.TryReserveSeats())
                    throw new NoAvailableSeatsException("Нет доступных мест для бронирования на данное событие!");

                var booking = new Booking
                {
                    BookingId = Guid.NewGuid(),
                    EventId = id,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = null,
                    Event = existingEvent
                };

                _appDbContext.Bookings.Add(booking);

                await _appDbContext.SaveChangesAsync(cancellationToken);

                return BookingMapper.BookingToResponse(booking);
            }
            finally
            { 
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Получает бронь по Id.
        /// Если бронь не найдена, бросает исключение NotFoundException.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор брони.
        /// </param>
        /// <returns>
        /// Объект брони из коллекции.
        /// </returns>
        public async Task<BookingDtoResponse> GetBookingByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var existingBooking = await _appDbContext.Bookings.FirstOrDefaultAsync(b => b.BookingId == id, cancellationToken);

            if (existingBooking is null)
                throw new NotFoundException("Бронирование по указанному идентификатору не найдено!");

            var bookingDtoResponse = BookingMapper.BookingToResponse(existingBooking);

            return bookingDtoResponse;
        }
    }
}
