using EventManagementService.Enums;
using EventManagementService.Stores;

namespace EventManagementService.BackgroundServices
{
    public class BookingProcessingService : BackgroundService
    {
        //Почему используем IServiceProvider, а не просто IBookingStore в конструкторе?
        //👉 Потому что BackgroundService — singleton
        //👉 А Scoped зависимости(если появятся позже) сломаются

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingProcessingService> _logger;

        public BookingProcessingService(IServiceProvider serviceProvider, ILogger<BookingProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Выполняет основную логику фоновой службы обработки броней.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис обработки броней запущен - {Time}", DateTime.Now);

            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    await ProcessPendingBookingsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при изменении статуса брони!");
                }

                // Задержка после цикла для снижения нагрузки CPU
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Сервис управления обработкой броней остановлен");
        }

        public async Task ProcessPendingBookingsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingStore>();

            // Фильтруем коллекцию броней по статусу и отсутствию даты обработки
            var pendingBookings = bookingStore.Bookings
                .Where(b => b.Status == BookingStatus.Pending && b.ProcessedAt == null)
                .ToList();

            foreach (var booking in pendingBookings)
            {
                // Имитация внешней обработки
                await Task.Delay(2000, stoppingToken);

                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.UtcNow;
            }
        }
    }
}
