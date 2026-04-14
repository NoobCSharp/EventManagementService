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
        private readonly IEventStore _eventStore;
        private readonly IBookingStore _bookingStore;
        private readonly ILogger<BookingProcessingService> _logger;


        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

        public BookingProcessingService(IServiceProvider serviceProvider, IEventStore eventStore, IBookingStore bookingStore, ILogger<BookingProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _eventStore = eventStore;
            _bookingStore = bookingStore;
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

                    // Получаем список броней, которые находятся в статусе "Pending" и не имеют даты обработки
                    List<Booking> pendingBookings = bookingStore.GetPending().ToList();

                    var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));

                    // Ожидаем завершения всех задач обработки броней
                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не останавливаем сервис, чтобы он продолжал работать
                    _logger.LogError(ex, "Ошибка при изменении статуса брони!");
                }
            }

            _logger.LogInformation("Сервис управления обработкой броней остановлен");
        }

        private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            Event? @event = null;
            DateTime processedAt = DateTime.UtcNow;

            try
            {
                await Task.Delay(1000, stoppingToken);

                await _processingSemaphore.WaitAsync(stoppingToken);

                @event = _eventStore.Events.FirstOrDefault(e => e.EventId == booking.EventId);

                if (@event is null)
                {
                    _logger.LogWarning("Событие для брони {BookingId} не найдено. Бронь отклонена", booking.Id);

                    // Если события нет, то отклоняем бронь
                    booking.Reject(processedAt);

                    // Обновляем бронь в хранилище
                    _bookingStore.Update(booking);

                    return;
                }

                booking.Confirm(processedAt);

                _logger.LogInformation("Бронь {BookingId} обработана и подтверждена", booking.Id);

            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обработка брони {BookingId} была отменена", booking.Id);

                // В случае отмены операции, устанавливаем статус "Rejected"
                booking.Reject(processedAt);

                // Обновляем бронь в хранилище
                _bookingStore.Update(booking);

                if (@event is not null) 
                {
                    // Освобождаем зарезервированные места
                    @event.ReleaseSeats();

                    _eventStore.Update(@event);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Ошибка при обработке брони {BookingId}. Бронь отклонена", booking.Id);

                // В случае ошибки при обработке брони, устанавливаем статус "Rejected"
                booking.Reject(processedAt);

                // Обновляем бронь в хранилище
                _bookingStore.Update(booking);

                if (@event is not null)
                {
                    // Освобождаем зарезервированные места
                    @event.ReleaseSeats();

                    _eventStore.Update(@event);
                }
            }
            finally
            {
                // Освобождаем семафор, чтобы другие задачи могли продолжить обработку броней
                if (_processingSemaphore.CurrentCount == 0)
                {
                    _processingSemaphore.Release();
                }
            }
        }
    }
}
