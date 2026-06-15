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

        public async Task<BookingDtoResponse> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var existingBooking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

            if (existingBooking is null)
                throw new NotFoundException("Бронирование по указанному идентификатору не найдено!");

            var bookingDtoResponse = BookingMapper.BookingToResponse(existingBooking);

            return bookingDtoResponse;
        }

        public async Task<BookingDtoResponse> CreateBookingAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();

            try
            {
                var existingEvent = await _eventRepository.GetEventByIdAsync(bookingId, cancellationToken);

                if (existingEvent is null)
                    throw new NotFoundException("Событие по указанному идентификатору не найдено!");

                if (existingEvent.StartAt <= DateTime.UtcNow)
                    throw new EventAlreadyStartedException("Невозможно забронировать прошедшее или уже начавшееся событие!");

                if (!existingEvent.TryReserveSeats())
                    throw new NoAvailableSeatsException("Нет доступных мест для бронирования на данное событие!");

                var activeBookingsCount = await _bookingRepository.GetActiveBookingsCountAsync(userId, cancellationToken);

                if (activeBookingsCount >= 10)
                    throw new ActiveBookingLimitExceededException("Превышен допустимый лимит активных бронирований!");

                var booking = new Booking
                {
                    BookingId = Guid.NewGuid(),
                    EventId = bookingId,
                    UserId = Guid.NewGuid(), //TODO временное решение, пока нет авторизации и аутентификации
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

        public async Task RemoveBookingAsync(Guid bookingId, Guid userId, Role role, CancellationToken cancellationToken = default)
        {
            Booking? booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

            if (booking is null)
                throw new NotFoundException("Бронирование по указанному идентификатору не найдено!");

            if (role is not Role.Admin && booking.UserId != userId)
                throw new BookingAccessDeniedException("У пользователя не достаточно прав на выполнение данной операции!");

            _bookingRepository.RemoveBooking(booking);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
