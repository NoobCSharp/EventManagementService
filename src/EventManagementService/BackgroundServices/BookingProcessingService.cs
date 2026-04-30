using EventManagementService.DataAccess;
using EventManagementService.Enums;
using Microsoft.EntityFrameworkCore;

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
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Сервис обработки броней запущен - {Time}", DateTime.Now);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _factory.CreateScope();

                    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Получаем список броней со статусом "Pending" для обработки, ограничивая количество обрабатываемых броней за итерацию, чтобы избежать перегрузки системы
                    var pendingBookings = await appDbContext.Bookings
                        .Where(b => b.Status == BookingStatus.Pending)
                        .Take(50)
                        .ToListAsync(cancellationToken);

                    // Обрабатываем каждую бронь параллельно по Id
                    var tasks = pendingBookings.Select(booking =>
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

            _logger.LogInformation("Сервис управления обработкой броней остановлен");
        }

        public async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            DateTime processedAt = DateTime.UtcNow;
           
            try
            {
                await Task.Delay(2000, cancellationToken);

                using var scope = _factory.CreateScope();
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Загружаем бронь из базы данных
                var booking = await appDbContext.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);

                if (booking is null)
                {
                    _logger.LogWarning(
                        "Бронь {BookingId} не найдена!",
                        bookingId);

                    return;
                }

                var @event = await appDbContext.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId, cancellationToken);

                if (@event is null)
                {
                    _logger.LogWarning(
                        "Событие для брони {BookingId} не найдено!",
                        bookingId);
                    
                    // Если событие не найдено, отклоняем бронь, так как она не может быть обработана без связанного события
                    booking.Reject(processedAt);

                    await appDbContext.SaveChangesAsync(cancellationToken);

                    return;
                }

                // Подтверждаем бронь
                booking.Confirm(processedAt);

                // Сохраняем изменения в базе данных
                await appDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Бронь {BookingId} обработана и подтверждена",
                    booking.BookingId);

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обработка брони {BookingId} отменена!", bookingId);
            }
            catch (Exception)
            {
                try
                {
                    using var scope = _factory.CreateScope();
                    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var booking = await appDbContext.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);

                    if (booking != null)
                    {
                        // В случае ошибки при обработке брони, устанавливаем статус "Rejected"
                        booking.Reject(processedAt);

                        var @event = await appDbContext.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId, cancellationToken);

                        // Если произошла ошибка, освобождаем зарезервированные места, чтобы они снова стали доступными для других броней
                        if (@event != null)
                            @event.ReleaseSeats();

                        // Сохраняем изменения в базе данных
                        await appDbContext.SaveChangesAsync(cancellationToken);

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
