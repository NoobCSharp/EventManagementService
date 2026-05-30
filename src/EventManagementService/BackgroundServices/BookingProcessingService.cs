using Application.Interfaces;

namespace EventManagementService.BackgroundServices
{
    public class BookingProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<BookingProcessingService> _logger;

        public BookingProcessingService(IServiceScopeFactory factory, ILogger<BookingProcessingService> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        /// <summary>
        /// Выполняет основную логику фоновой службы обработки броней.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Сервис обработки броней запущен - {Time}.", DateTime.Now);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _factory.CreateScope();

                    var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                    // Получаем список броней со статусом "Pending" для обработки,
                    var pendingBookings = await repository.GetPendingBookingsAsync(cancellationToken);

                    // Обрабатываем каждую бронь параллельно по Id,
                    // ограничивая количество обрабатываемых броней за итерацию, чтобы избежать перегрузки системы
                    var tasks = pendingBookings.Take(50).Select(booking =>
                        ProcessBookingAsync(booking.BookingId, cancellationToken));

                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не останавливаем сервис, чтобы он продолжал работать
                    _logger.LogError(ex, "Ошибка при обработке броней!");
                }

                // Задержка между итерациями, чтобы не перегружать систему постоянными запросами к хранилищу
                await Task.Delay(5000, cancellationToken);
            }

            _logger.LogInformation("Сервис управления обработкой броней остановлен.");
        }

        public async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            DateTime processedAt = DateTime.UtcNow;
           
            try
            {
                await Task.Delay(2000, cancellationToken);

                using var scope = _factory.CreateScope();

                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Загружаем бронь из базы данных
                var booking = await bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

                if (booking is null)
                {
                    _logger.LogWarning(
                        "Бронь {BookingId} не найдена!",
                        bookingId);

                    return;
                }

                var @event = await eventRepository.GetEventByIdAsync(booking.EventId, cancellationToken);

                if (@event is null)
                {
                    _logger.LogWarning(
                        "Событие для брони {BookingId} не найдено!",
                        bookingId);
                    
                    // Если событие не найдено, отклоняем бронь, так как она не может быть обработана без связанного события
                    booking.Reject(processedAt);

                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    return;
                }

                // Подтверждаем бронь
                booking.Confirm(processedAt);

                // Сохраняем изменения в базе данных
                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Бронь {BookingId} обработана и подтверждена.",
                    booking.BookingId);

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обработка брони {BookingId} отменена!", bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке брони {BookingId}!", bookingId);

                try
                {
                    using var scope = _factory.CreateScope();

                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var booking = await bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

                    if (booking != null)
                    {
                        // В случае ошибки при обработке брони, устанавливаем статус "Rejected"
                        booking.Reject(processedAt);

                        var @event = await eventRepository.GetEventByIdAsync(booking.EventId, cancellationToken);

                        // Если произошла ошибка, освобождаем зарезервированные места, чтобы они снова стали доступными для других броней
                        if (@event != null)
                            @event.ReleaseSeats();

                        // Сохраняем изменения в базе данных
                        await unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена!", bookingId);
                    }
                }
                catch (Exception)
                {
                    _logger.LogError("Ошибка при отклонении брони {BookingId} после неудачной обработки!", bookingId);
                }
            }
        }
    }
}
