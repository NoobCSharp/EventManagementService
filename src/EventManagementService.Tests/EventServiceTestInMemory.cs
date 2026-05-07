using EventManagementService.DataAccess;
using EventManagementService.Dtos.EventDtos;
using EventManagementService.Exceptions;
using EventManagementService.Filters;
using EventManagementService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagementService.Tests
{
    public class EventServiceTestInMemory
    {
        private readonly ServiceProvider _serviceProvider;

        public EventServiceTestInMemory()
        {
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            // Регистрируем сервисы
            services.AddScoped<EventService>();
            services.AddScoped<BookingService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        #region Successful scenarios for EventService

        [Fact]
        public async Task AddEvent_ShouldAddEvent_To_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            // Act (действие)
            var result = await service.AddEventAsync(
                new EventDtoRequest()
                {
                    Title = "Test",
                    Description = "TestDescription",
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now.AddDays(1),
                    TotalSeats = 1,
                }
            );

            // Assert (проверка)
            // Проверяем, что в базу данных добавлено событие, проверяем по уникальному Id
            var addedEvent = await context.Events.SingleAsync(e => e.EventId == result.EventId);
            //Проверяем, что у добавленного события совпадает Title
            addedEvent.Title.Should().Be("Test");
            //Проверяем, что у добавленного события совпадает Description
            addedEvent.Description.Should().Be("TestDescription");
            //Проверяем, что у добавленного события совпадает TotalSeats
            addedEvent.TotalSeats.Should().Be(1);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_AllEvents_From_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var filter = new EventFilter();

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            // Проверяем, что метод GetEvents возвращает PaginatedResultDto
            // с внутренней коллекцией ResponseEventDtos с равным количеством событий
            result.ResponseEventDtos.Should().HaveCount(context.Events.Count());
        }

        [Fact]
        public async Task GetEvents_ShouldReturn_EmptyCollection_WhenFilterHasNoMatches()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var filter = new EventFilter()
            {
                Title = "Y",
            };

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            // Проверяем, что коллекция событий не null, если нет совпадения по фильтру
            result.ResponseEventDtos.Should().NotBeNull();
            // Проверяем, что коллекция событий пустая, если нет совпадения по фильтру
            result.ResponseEventDtos.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEventById_ShouldReturn_Event_From_DB_ById()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();
            
            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act (действие)
            var result = await service.GetEventByIdAsync(eventId);

            // Assert (проверка)
            // Проверяем, что событие существует
            result.Should().NotBeNull();
            // Проверяем, что событие имеет правильный Id
            result.EventId.Should().Be(eventId);
        }

        [Fact]
        public async Task ChangeEvent_ShouldUpdate_Event_In_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            var eventDtoRequest = new EventDtoRequest()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 26),
            };

            // Act (действие)
            await service.UpdateEventAsync(eventId, eventDtoRequest);

            // Assert (проверка)
            // Проверяем, что у события по указанному Id были обновлены свойства
            var updatedEvent = context.Events.Single(e => e.EventId == eventId);

            updatedEvent.Title.Should().Be(eventDtoRequest.Title);
            updatedEvent.Description.Should().Be(eventDtoRequest.Description);
            updatedEvent.StartAt.Should().Be(eventDtoRequest.StartAt);
            updatedEvent.EndAt.Should().Be(eventDtoRequest.EndAt);
        }

        [Fact]
        public async Task RemoveEvent_ShouldRemove_Event_From_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF");

            // Act (действие)
            await service.RemoveEventAsync(eventId);

            var removedEvent = context.Events.FirstOrDefault(e => e.EventId == eventId);

            // Assert (проверка)
            // Проверяем, что событие больше не существует
            removedEvent.Should().BeNull();
            // Проверяем, что в коллекции уменьшилось количество событий
            context.Events.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_FilteredEvents_ByTitle_From_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());
            
            await context.SaveChangesAsync();

            var title = "B";

            var filter = new EventFilter()
            {
                Title = title,
            };

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            // Проверяем, что в коллекции содержаться только события после фильтрации по Title
            result.ResponseEventDtos.Should().OnlyContain(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_FilteredEvents_ByDate_From_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());
            
            await context.SaveChangesAsync();

            var fromDate = new DateTime(2026, 03, 12);
            var toDate = new DateTime(2026, 03, 15);

            var filter = new EventFilter()
            {
                From = fromDate,
                To = toDate
            };

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            // Проверяем, что вообще что-то вернулось
            result.ResponseEventDtos.Should().NotBeEmpty();
            // Проверяем, что в коллекции содержаться только события после фильтрации по StartAt и EndAt
            result.ResponseEventDtos.Should().OnlyContain(e => e.StartAt >= fromDate && e.EndAt <= toDate);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_PaginatedEvents_From_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var page = 2;
            var pageSize = 2;

            var filter = new EventFilter()
            {
                Page = page,
                PageSize = pageSize
            };

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            // Проверяем, что количество элементов соответствует количеству элементов на странице после пагинации
            result.ResponseEventDtos.Should().HaveCount(pageSize);
            // Проверяем, что каждый элемент соответствует требуемому Id
            result.ResponseEventDtos.Select(e => e.EventId).Should().Equal(Guid.Parse("3F9619FF-8B86-D011-B42D-00C04FC964FF"), Guid.Parse("4F9619FF-8B86-D011-B42D-00C04FC964FF"));
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_FilteredAndPaginatedEvents_From_DB()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var title = "B";
            var fromDate = new DateTime(2026, 03, 12);
            var toDate = new DateTime(2026, 03, 19);
            var page = 2;
            var pageSize = 2;

            var filter = new EventFilter()
            {
                Title = title,
                From = fromDate,
                To = toDate,
                Page = page,
                PageSize = pageSize
            };

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            // Проверяем, что есть события в коллекции после фильтрации и пагинации
            result.ResponseEventDtos.Any().Should().BeTrue();
            // Проверяем что Id событий верный
            result.ResponseEventDtos.Select(e => e.EventId).Should().Equal(Guid.Parse("4F9619FF-8B86-D011-B42D-00C04FC964FF"));
            // Проверяем, что в коллекции содержаться только события после фильтрации
            result.ResponseEventDtos
                .Should().OnlyContain(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase)
                && e.StartAt >= fromDate
                && e.EndAt <= toDate);
        }

        [Fact]
        public async Task GetEvents_ShouldReturnLastPage_WithRemainingItem()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var filer = new EventFilter()
            {
                Page = 3,
                PageSize = 2
            };

            // Act (действие)
            var result = await service.GetEventsAsync(filer);

            // Assert (проверка)
            // Проверяем, что на последней странице содержится только одно событие с требуемым Id
            result.ResponseEventDtos.Should().ContainSingle(e => e.EventId == Guid.Parse("5F9619FF-8B86-D011-B42D-00C04FC964FF"));
        }

        #endregion

        #region Unsuccessful scenarios for EventService

        [Fact]
        public async Task GetEventById_WithNonExistingId_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException
            await service.Invoking(s => s.GetEventByIdAsync(new Guid()))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ChangeEvent_WithNonExistingId_ShouldThrow_NotFoundExceptionAsync()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventDtoRequest = new EventDtoRequest()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 26),
            };

            // Assert(проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException
            await service.Invoking(s => s.UpdateEventAsync(new Guid(), eventDtoRequest))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task AddEvent_WithIncorrectData_Title_ShouldThrow_BadRequestException()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventDtoRequest = new EventDtoRequest
            {
                Title = string.Empty,
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 24),
            };

            // Assert(проверка)
            // Проверяем, что выбрасывается ожидаемое исключение BadRequestException
            // с некорректными входными данными по свойству Title
            await service.Invoking(s => s.AddEventAsync(eventDtoRequest))
                .Should()
                .ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ChangeEvent_WithIncorrectData_EndAtEarlierStartAt_ShouldThrow_BadRequestException()
        {
            // Arrange (подготовка)
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<EventService>();

            context.Events.AddRange(ServicesTestHelper.CreateFakeEvents());

            await context.SaveChangesAsync();

            var eventDtoRequest = new EventDtoRequest()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 24),
            };

            // Assert(проверка)
            // Проверяем, что выбрасывается ожидаемое исключение BadRequestException
            // с датой окончания события раньше даты начала события
            await service.Invoking(s => s.UpdateEventAsync(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"), eventDtoRequest))
                .Should()
                .ThrowAsync<BadRequestException>();
        }

        #endregion
    }
}
