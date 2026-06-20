using Application.Interfaces;

namespace EventManagementService.BackgroundServices
{
    public class BookingProcessorHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<BookingProcessorHostedService> _logger;

        public BookingProcessorHostedService(IServiceScopeFactory factory, ILogger<BookingProcessorHostedService> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        /// <summary>
        /// Выполняет основную логику фоновой службы обработки броней.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Фоновая обработка броней запущена - {Time}", DateTime.Now);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _factory.CreateScope();

                    var processor = scope.ServiceProvider.GetRequiredService<IBookingProcessor>();

                    await processor.ProcessPendingBookingsAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не останавливаем сервис, чтобы он продолжал работать
                    _logger.LogError(ex, "Ошибка во время фоновой обработки броней!");
                }

                // Задержка между итерациями, чтобы не перегружать систему постоянными запросами к хранилищу
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }

            _logger.LogInformation("Фоновая обработка броней остановлена - {Time}", DateTime.Now);
        }
    }
}
