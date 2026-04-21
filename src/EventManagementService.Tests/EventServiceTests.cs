using EventManagementService.Exceptions;
using EventManagementService.Filters;
using EventManagementService.Stores;
using EventManagementService.Services;
using FluentAssertions;
using Moq;
using EventManagementService.Dtos.EventDtos;

namespace EventManagementService.Tests
{
    public class EventServiceTests
    {
        #region Successful scenarios for EventService

        [Fact]
        public async Task AddEvent_ShouldAddEvent_To_Collection()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

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
            // Проверяем, что в коллекцию добавлено событие, проверяем по уникальному Id
            var addedEvent = mockEventStore.Object.Events.Single(e => e.EventId == result.EventId);
            //Проверяем, что у добавленного соития совпадает Title
            addedEvent.Title.Should().Be("Test");
            //Проверяем, что у добавленного события совпадает Description
            addedEvent.Description.Should().Be("TestDescription");
            //Проверяем, что у добавленного события совпадает TotalSeats
            addedEvent.TotalSeats.Should().Be(1);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_AllEvents_From_Colletion()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);
            var filter = new EventFilter();

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            // Проверяем, что метод GetEvents возвращает PaginatedResultDto
            // с внутренней коллекцией ResponseEventDtos с равным количеством событий
            result.ResponseEventDtos.Should().HaveCount(events.Count());
        }

        [Fact]
        public async Task GetEvents_ShouldReturn_EmptyCollection_WhenFilterHasNoMatches()
        {
            // Arrange (подготовка)
            
            var mockEventStore = new Mock<IEventStore>();
            var events = ServicesTestHelper.CreateFakeEvents();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);
            
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
        public async Task GetEventById_ShouldReturn_Event_From_Colletion_ById()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

            // Act (действие)
            var result = await service.GetEventByIdAsync(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"));

            // Assert (проверка)
            //Проверяем, что событие существует
            result.Should().NotBeNull();
            //Проверяем, что событие имеет правильный Id
            result.EventId.Should().Be(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"));
        }

        [Fact]
        public async Task ChangeEvent_ShouldUpdate_Event_In_Collection()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper. CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

            var eventDtoRequest = new EventDtoRequest()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 26),
            };

            // Act (действие)
            await service.ChangeEvent(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"), eventDtoRequest);

            // Assert (проверка)
            // Проверяем, что у события по указанному Id были обновлены свойства
            var updatedEvent = events.Single(e => e.EventId == Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"));

            updatedEvent.Title.Should().Be(eventDtoRequest.Title);
            updatedEvent.Description.Should().Be(eventDtoRequest.Description);
            updatedEvent.StartAt.Should().Be(eventDtoRequest.StartAt);
            updatedEvent.EndAt.Should().Be(eventDtoRequest.EndAt);
        }

        [Fact]
        public async Task RemoveEvent_ShouldRemove_Event_From_Collection()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

            // Act (действие)
            await service.RemoveEventAsync(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"));
            var removedEvent = events.FirstOrDefault(e => e.EventId == Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"));

            // Assert (проверка)
            // Проверяем, что событие больше не существует
            removedEvent.Should().BeNull();
            // Проверяем, что в коллекции уменьшилось количество событий
            events.Should().HaveCount(4);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_FilteredEvents_ByTitle_From_Colletion()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

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
        public async Task GetEvents_ShouldReturns_FilteredEvents_ByDate_From_Colletion()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

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
            // Проверяем, что в коллекции содержаться только события после фильтрации по StartAt и EndAt
            result.ResponseEventDtos.Should().OnlyContain(e => e.StartAt >= fromDate && e.EndAt <= toDate);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_PaginatedEvents_From_Colletion()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

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
        public async Task GetEvents_ShouldReturns_FilteredAndPaginatedEvents_From_Colletion()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

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
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();
            
            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

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
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

            // Assert (проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException
            await service.Invoking(s => s.GetEventByIdAsync(new Guid()))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public void ChangeEvent_WithNonExistingId_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

            var eventDtoRequest = new EventDtoRequest()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 26),
            };

            // Assert(проверка)
            // Проверяем, что выбрасывается ожидаемое исключение NotFoundException
            service.Invoking(s => s.ChangeEvent(new Guid(), eventDtoRequest))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public void AddEvent_WithIncorrectData_Title_ShouldThrow_BadRequestException()
        {
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

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
            service.Invoking(s => s.AddEventAsync(eventDtoRequest))
                .Should()
                .ThrowAsync<BadRequestException>();
        }

        [Fact]
        public void ChangeEvent_WithIncorrectData_EndAtEarlierStartAt_ShouldThrow_BadRequestException()
        {
            // Arrange (подготовка)
            var events = ServicesTestHelper.CreateFakeEvents();
            var mockEventStore = new Mock<IEventStore>();

            mockEventStore.Setup(e => e.Events).Returns(events);

            var service = new EventService(mockEventStore.Object);

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
            service.Invoking(s => s.ChangeEvent(Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"), eventDtoRequest))
                .Should()
                .ThrowAsync<BadRequestException>();
        }

        #endregion
    }
}
