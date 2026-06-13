using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public sealed class BookingProcessorService : IBookingProcessor
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingProcessorService> _logger;

        public BookingProcessorService(IBookingRepository bookingRepository, IEventRepository eventRepository, IUnitOfWork unitOfWork, ILogger<BookingProcessorService> logger)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken = default)
        {
            var pendingBookings = await _bookingRepository.GetPendingBookingsAsync(cancellationToken);

            foreach (var booking in pendingBookings)
            {
                await ProcessBookingAsync(booking.BookingId, cancellationToken);
            }
        }

        public async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var processedAt = DateTime.UtcNow;

            try
            {
                await Task.Delay(2000, cancellationToken);

                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

                if (booking is null)
                {
                    _logger.LogWarning("Бронь {BookingId} не найдена.", bookingId);

                    return;
                }

                var @event = await _eventRepository.GetEventByIdAsync(booking.EventId, cancellationToken);

                if (@event is null)
                {
                    _logger.LogWarning("Событие для брони {BookingId} не найдено.", bookingId);

                    booking.Reject(processedAt);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return;
                }

                booking.Confirm(processedAt);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Бронь {BookingId} успешно подтверждена.", bookingId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обработка брони {BookingId} отменена.", bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке брони {BookingId}.", bookingId);

                try
                {
                    var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

                    if (booking is null)
                        return;

                    booking.Reject(processedAt);

                    var @event = await _eventRepository.GetEventByIdAsync(booking.EventId, cancellationToken);

                    @event?.ReleaseSeats();

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Бронь {BookingId} отклонена после возникновения ошибки.", bookingId);
                }
                catch (Exception rejectException)
                {
                    _logger.LogError(rejectException, "Не удалось отклонить бронь {BookingId}.", bookingId);
                }
            }
        }
    }
}
