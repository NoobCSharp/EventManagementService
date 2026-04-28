using EventManagementService.DataAccess;
using EventManagementService.Enums;
using EventManagementService.Models;
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
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Сервис обработки броней запущен - {Time}", DateTime.Now);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _factory.CreateScope();

                    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var pendingBookings = await appDbContext.Bookings
                        .Where(b => b.Status == BookingStatus.Pending)
                        .ToListAsync(cancellationToken);

                    // Обрабатываем каждую бронь параллельно по Id
                    var tasks = pendingBookings.Select(booking =>
                        ProcessBookingAsync(booking.BookingId, cancellationToken));

                    await Task.WhenAll(tasks);

                    // Задержка между итерациями, чтобы не перегружать систему постоянными запросами к хранилищу
                    await Task.Delay(500, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не останавливаем сервис, чтобы он продолжал работать
                    _logger.LogError(ex, "Ошибка при обработке броней!");
                }
            }

            _logger.LogInformation("Сервис управления обработкой броней остановлен");
        }

        public async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            DateTime processedAt = DateTime.UtcNow;
            Booking? booking = null;

            await Task.Delay(1000, cancellationToken);

            using var scope = _factory.CreateScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // Загружаем бронь из базы данных, включая связанные данные о событии
                booking = await appDbContext.Bookings
                    .Include(b => b.Event)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);

                if (booking is null)
                {
                    _logger.LogWarning(
                        "Бронь {BookingId} не найдена",
                        bookingId);

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
                _logger.LogInformation(
                    "Обработка брони {BookingId} была отменена",
                    bookingId);

                // В случае отмены операции, устанавливаем статус "Rejected"
                if (booking is not null)
                {
                    booking.Reject(processedAt);
                    // Если операция была отменена, освобождаем зарезервированные места, чтобы они снова стали доступными для других броней
                    booking.Event.ReleaseSeats();

                    // Сохраняем изменения в базе данных
                    await appDbContext.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена", bookingId);

                // В случае ошибки при обработке брони, устанавливаем статус "Rejected"
                booking?.Reject(processedAt);

                // Если произошла ошибка, освобождаем зарезервированные места, чтобы они снова стали доступными для других броней
                booking?.Event.ReleaseSeats();

                // Сохраняем изменения в базе данных
                await appDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
