using Application.Dtos.BookingDtos;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public BookingService(IEventRepository eventRepository, IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _eventRepository = eventRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
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
        public async Task<BookingDtoResponse> CreateBookingAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();

            try
            {
                var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

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

                await _bookingRepository.CreateBookingAsync(booking, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        /// Объект брони из хранилища данных.
        /// </returns>
        public async Task<BookingDtoResponse> GetBookingByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingBooking = await _bookingRepository.GetBookingByIdAsync(id, cancellationToken);

            if (existingBooking is null)
                throw new NotFoundException("Бронирование по указанному идентификатору не найдено!");

            var bookingDtoResponse = BookingMapper.BookingToResponse(existingBooking);

            return bookingDtoResponse;
        }
    }
}
