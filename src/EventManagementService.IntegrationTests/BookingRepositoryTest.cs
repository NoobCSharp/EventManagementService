using EventManagementService.DataAccess;
using EventManagementService.Enums;
using EventManagementService.Models;
using EventManagementService.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace EventManagementService.IntegrationTests
{
    public class BookingRepositoryTest : IAsyncLifetime
    {
        /// <summary>
        ///  Контейнер PostgreSQL для тестирования репозитория событий. 
        ///  Он использует образ "postgres:16-alpine"
        ///  будет автоматически запущен перед выполнением тестов и остановлен после их завершения.
        /// </summary>
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        private AppDbContext CreateContext()
        {
            // Создание экземпляра AppDbContext с использованием строки подключения из контейнера PostgreSQL.
            var connectionString = _postgres.GetConnectionString();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            var context = new AppDbContext(options);

            // Создаёт таблицы по модели EF Core — аналог миграций, но без файлов миграций.
            context.Database.EnsureCreated();

            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();

            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Bookings\", \"Events\" RESTART IDENTITY CASCADE");
        }

        [Fact]
        public async Task CreateBooking_ShouldAddBookingToDatabase()
        {
            // Arrange
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var eventRepository = new EventRepository(context);
            var bookingRepository = new BookingRepository(context);

            var @event = new Event
            {
                EventId = Guid.NewGuid(),
                Title = "Test Event",
                Description = "This is a test event.",
                //PostgreSQL тип timestamp with time zone всегда хранит время в UTC. Npgsql провайдер строго это проверяет
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 100,
                AvailableSeats = 100
            };

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                EventId = @event.EventId,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                Event = @event,
                Status = BookingStatus.Pending
            };

            // Act
            await eventRepository.AddEventAsync(@event);
            await bookingRepository.CreateBookingAsync(booking);

            await context.SaveChangesAsync();

            // Assert
            await using var verifyContext = CreateContext();

            var retrievedBooking = await bookingRepository.GetBookingByIdAsync(booking.BookingId);

            retrievedBooking.Should().NotBeNull();
            retrievedBooking.EventId.Should().Be(@event.EventId);
        }

        [Fact]
        public async Task GetBookingById_ShouldReturnBooking_FromDatabase()
        {
            await ResetDatabaseAsync();

            await using var context = CreateContext();

            var bookingRepository = new BookingRepository(context);
        }

    }
}
