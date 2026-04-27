using EventManagementService.Models;
using EventManagementService.Stores;

namespace EventManagementService.BackgroundServices
{
    public class BookingProcessingService : BackgroundService
    {
        //Почему используем IServiceProvider, а не просто IBookingStore в конструкторе?
        //Потому что BackgroundService — singleton
        //А Scoped зависимости(если появятся позже) сломаются

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingProcessingService> _logger;

        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

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
                    using var scope = _serviceProvider.CreateScope();
                    var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingStore>();

                    var pendingBookings = bookingStore.GetPending().ToList();

                    var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));

                    await Task.WhenAll(tasks);

                    // Задержка между итерациями, чтобы не перегружать систему постоянными запросами к хранилищу
                    await Task.Delay(500, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
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

        public async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingStore>();
            var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

            Event? @event = null;
            DateTime processedAt = DateTime.UtcNow;

            try
            {
                await Task.Delay(1000, stoppingToken);

                // Критическая секция: доступ к общим ресурсам (хранилищам) должен быть синхронизирован, чтобы избежать гонок данных и обеспечить целостность данных
                await _processingSemaphore.WaitAsync(stoppingToken);

                try
                {
                    @event = eventStore.Events.FirstOrDefault(e => e.EventId == booking.EventId);

                    if (@event is null)
                    {
                        _logger.LogWarning("Событие для брони {BookingId} не найдено. Бронь отклонена", booking.BookingId);

                        // Если события нет, то отклоняем бронь
                        booking.Reject(processedAt);
                        bookingStore.Update(booking);
                    }
                    else
                    {
                        // Подтверждаем бронь
                        booking.Confirm(processedAt);
                        bookingStore.Update(booking);

                        _logger.LogInformation("Бронь {BookingId} обработана и подтверждена", booking.BookingId);
                    }
                }
                finally
                {
                    // Освобождаем семафор, чтобы другие задачи могли продолжить обработку броней
                    _processingSemaphore.Release();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обработка брони {BookingId} была отменена", booking.BookingId);

                // В случае отмены операции, устанавливаем статус "Rejected"
                booking.Reject(processedAt);
                bookingStore.Update(booking);

                if (@event is not null) 
                {
                    // Если операция была отменена, освобождаем зарезервированные места, чтобы они снова стали доступными для других броней
                    @event.ReleaseSeats();
                    eventStore.Update(@event);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена", booking.BookingId);

                // В случае ошибки при обработке брони, устанавливаем статус "Rejected"
                booking.Reject(processedAt);
                bookingStore.Update(booking);

                if (@event is not null)
                {
                    // Если произошла ошибка, освобождаем зарезервированные места, чтобы они снова стали доступными для других броней
                    @event.ReleaseSeats();
                    eventStore.Update(@event);
                }
            }
        }
    }
}
