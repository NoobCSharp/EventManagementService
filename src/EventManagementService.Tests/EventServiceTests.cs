using EventManagementService.Dtos;
using EventManagementService.Exceptions;
using EventManagementService.Fiters;
using EventManagementService.Models;
using EventManagementService.Repositories;
using EventManagementService.Services;
using FluentAssertions;
using Moq;

namespace EventManagementService.Tests
{
    public class EventServiceTests
    {
        // Метод подготовки входных данных (типизированный)
        public static TheoryData<List<Event>> EventsData => new()
        {
             CreateEvents()
        };

        private static List<Event> CreateEvents() => new()
        {
            new Event() { Id = 1, Title = "AA", Description = "Description1", StartAt = new DateTime(2026, 03, 10), EndAt = new DateTime(2026, 03, 11) },
            new Event() { Id = 2, Title = "AB", Description = "Description2", StartAt = new DateTime(2026, 03, 12), EndAt = new DateTime(2026, 03, 13) },
            new Event() { Id = 3, Title = "BB", Description = "Description3", StartAt = new DateTime(2026, 03, 14), EndAt = new DateTime(2026, 03, 15) },
            new Event() { Id = 4, Title = "BC", Description = "Description4", StartAt = new DateTime(2026, 03, 16), EndAt = new DateTime(2026, 03, 17) },
            new Event() { Id = 5, Title = "CC", Description = "Description5", StartAt = new DateTime(2026, 03, 18), EndAt = new DateTime(2026, 03, 19) }
        };

        #region Successful scenarios 

        [Fact]
        public void AddEvent_ShouldAddEvent_To_Collection()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .SetupProperty(r => r.Events, events)
                .Setup(r => r.GetAvailableId()).Returns(mockRepository.Object.Events.Any() ? mockRepository.Object.Events.Max(e => e.Id) + 1 : 1);

            var service = new EventService(mockRepository.Object);

            // Act (действие)
            var result = service.AddEvent(
                new RequestEventDto()
                {
                    Title = "Test",
                    Description = "TestDescription",
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now.AddDays(1),
                }
            );

            // Assert (проверка)
            // Проверяем что в коллекцию добавлено событие, проверяем по уникальному Id
            var addedEvent = mockRepository.Object.Events.Single(e => e.Id == result.Id);
            //Проверяем что у добавленного соития совпадает Title
            addedEvent.Title.Should().Be("Test");
            //Проверяем что у добавленного соития совпадает Description
            addedEvent.Description.Should().Be("TestDescription");

        }

        [Fact]
        public void GetEvents_ShouldReturns_AllEvents_From_Colletion()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);
            var filter = new EventFilter();

            // Act (действие)
            var result = service.GetEvents(filter);

            // Assert (проверка)
            // Проверяем, что метод GetEvents возвращает PaginatedResultDto
            // с внутренней коллекцией ResponseEventDtos с равным количеством событий
            result.ResponseEventDtos.Should().HaveCount(events.Count());
        }

        [Fact]
        public void GetEventById_ShouldReturn_Event_From_Colletion_ById()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            // Act (действие)
            var result = service.GetEventById(1);

            // Assert (проверка)
            //Проверяем что событие существует
            result.Should().NotBeNull();
            //Проверяем что событие имеет правильный Id
            result.Id.Should().Be(1);
        }

        [Fact]
        public void ChangeEvent_ShouldUpdate_Event_In_Collection()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            var requestEventDto = new RequestEventDto()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 26),
            };

            // Act (действие)
            service.ChangeEvent(1, requestEventDto);

            // Assert (проверка) - проверяем, что у события по указанному Id были обновлены свойства
            var updatedEvent = events.Single(e => e.Id == 1);

            updatedEvent.Title.Should().Be(requestEventDto.Title);
            updatedEvent.Description.Should().Be(requestEventDto.Description);
            updatedEvent.StartAt.Should().Be(requestEventDto.StartAt);
            updatedEvent.EndAt.Should().Be(requestEventDto.EndAt);
        }

        [Fact]
        public void RemoveEvent_ShouldRemove_Event_From_Collection()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            // Act (действие)
            service.RemoveEvent(1);
            var removedEvent = events.FirstOrDefault(e => e.Id == 1);

            // Assert (проверка)
            // Проверяем, что событие больше не существует
            removedEvent.Should().BeNull();
            // Проверяем, что в коллекции уменьшилось количество событий
            events.Should().HaveCount(4);
        }

        [Fact]
        public void GetEvents_ShouldReturns_FilteredEvents_ByTitle_From_Colletion()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            var title = "B";

            var filter = new EventFilter()
            {
                Title = title,
            };

            // Act (действие)
            var result = service.GetEvents(filter);

            // Assert (проверка)
            // Проверяем что в коллекции содержаться только события после фильтрации по Title
            result.ResponseEventDtos.Should().OnlyContain(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetEvents_ShouldReturns_FilteredEvents_ByDate_From_Colletion()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            var fromDate = new DateTime(2026, 03, 12);
            var toDate = new DateTime(2026, 03, 15);

            var filter = new EventFilter()
            {
                From = fromDate,
                To = toDate
            };

            // Act (действие)
            var result = service.GetEvents(filter);

            // Assert (проверка)
            // Проверяем что в коллекции содержаться только события после фильтрации по StartAt и EndAt
            result.ResponseEventDtos.Should().OnlyContain(e => e.StartAt >= fromDate && e.EndAt <= toDate);
        }

        [Fact]
        public void GetEvents_ShouldReturns_PaginatedEvents_From_Colletion()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            var page = 2;
            var pageSize = 2;

            var filter = new EventFilter()
            {
                Page = page,
                PageSize = pageSize
            };

            // Act (действие)
            var result = service.GetEvents(filter);

            // Assert (проверка)
            // Проверяем что количество элементов соответствует количеству элементов на странице после пагинации
            result.ResponseEventDtos.Should().HaveCount(pageSize);
            // Проверяем каждый элемент по Id
            result.ResponseEventDtos.Select(e => e.Id).Should().Equal(3, 4);
        }

        [Fact]
        public void GetEvents_ShouldReturns_FilteredAndPaginatedEvents_From_Colletion()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

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
            var result = service.GetEvents(filter);

            // Assert (проверка)
            // Проверяем что есть события в коллекции после фильтрации и пагинации
            result.ResponseEventDtos.Any().Should().BeTrue();
            // Проверяем что Id событий верный
            result.ResponseEventDtos.Select(e => e.Id).Should().Equal(4);
            // Проверяем что в коллекции содержаться только события после фильтрации
            result.ResponseEventDtos
                .Should().OnlyContain(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase)
                && e.StartAt >= fromDate
                && e.EndAt <= toDate);
        }

        #endregion

        #region Unsuccessful scenarios

        [Fact]
        public void GetEventById_WithNonExistingId_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            // Assert (проверка)
            // Проверяем что выбрасывается ожидаемое исключение NotFoundException
            service.Invoking(s => s.GetEventById(10))
                .Should()
                .Throw<NotFoundException>();
        }

        [Fact]
        public void ChangeEvent_WithNonExistingId_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            var requestEventDto = new RequestEventDto()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 26),
            };

            // Assert(проверка)
            // Проверяем что выбрасывается ожидаемое исключение NotFoundException
            service.Invoking(s => s.ChangeEvent(10, requestEventDto))
                .Should()
                .Throw<NotFoundException>();
        }

        [Fact]
        public void AddEvent_WithIncorrectData_ShouldThrow_ArgumentException()
        {
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            var requestEventDto = new RequestEventDto
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 24),
            };

            // Assert(проверка)
            // Проверяем что выбрасывается ожидаемое исключение ArgumentException
            service.Invoking(s => s.AddEvent(requestEventDto))
                .Should()
                .Throw<ArgumentException>();
        }

        [Fact]
        public void ChangeEvent_WithIncorrectData_EndAtEarlierStartAt()
        {
            // Arrange (подготовка)
            var events = CreateEvents();
            var mockRepository = new Mock<IEventRepository>();

            mockRepository
                .Setup(r => r.Events)
                .Returns(events);

            var service = new EventService(mockRepository.Object);

            var requestEventDto = new RequestEventDto()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 24),
            };

            // Assert(проверка)
            // Проверяем что выбрасывается ожидаемое исключение ArgumentException
            service.Invoking(s => s.ChangeEvent(1, requestEventDto))
                .Should()
                .Throw<ArgumentException>();
        }

        #endregion
    }
}
