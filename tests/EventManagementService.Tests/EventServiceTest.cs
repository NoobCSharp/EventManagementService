using Application.Dtos.EventDtos;
using Application.Filters;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Infrastructure.Entities;
using Moq;

namespace EventManagementService.Tests
{
    public class EventServiceTest
    {
        #region Successful scenarios for EventService

        [Fact]
        public async Task AddEvent_ShouldAdd_Event()
        {
            // Arrange (подготовка)  
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            var request = new EventDtoRequest()
            {
                Title = "Test",
                Description = "TestDescription",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                TotalSeats = 1,
            };

            //// Act (действие)
            var response = await service.AddEventAsync(request);

            // Assert (проверка)
            response.Should().NotBeNull();
            response.EventId.Should().NotBeEmpty();

            repositoryMock.Verify(
                r => r.AddEventAsync(It.Is<Event>(e => 
                    e.Title == request.Title && 
                    e.Description == request.Description &&
                    e.StartAt == request.StartAt &&
                    e.EndAt == request.EndAt &&
                    e.TotalSeats == request.TotalSeats &&
                    e.AvailableSeats == request.TotalSeats)),
                Times.Once);

            unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_AllEvents_When_FilterIsEmpty()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var fakeEvents = new List<Event>
            {
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "Event one",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow.AddDays(1),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                },
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "Event two",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow.AddDays(1),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                }
            };

            var pagedResult = new PagedResult<Event>
            {
                Page = 1,
                PageSize = 10,
                TotalCount = 2,
                Items = fakeEvents
            };

            repositoryMock.Setup(
                r => r.GetEventsAsync(
                    It.IsAny<EventFilter>()))
                .ReturnsAsync(pagedResult);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            var filter = new EventFilter();

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            result.Should().NotBeNull();
            result.ResponseEventDtos.Should().HaveCount(2);

            result.ResponseEventDtos[0].Title.Should().Be("Event one");
            result.ResponseEventDtos[1].Title.Should().Be("Event two");

            repositoryMock.Verify(
                r => r.GetEventsAsync(
                    It.IsAny<EventFilter>()),
                Times.Once);
        }

        [Fact]
        public async Task GetEvents_ShouldReturn_EmptyCollection_WhenFilterHasNoMatches()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var filter = new EventFilter()
            {
                Title = "Y",
            };

            var pagedResult = new PagedResult<Event>
            {
                Page = 1,
                PageSize = 10,
                TotalCount = 0,
                Items = []
            };

            repositoryMock.Setup(
                r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            result.Should().NotBeNull();

            result.ResponseEventDtos.Should().NotBeNull();
            result.ResponseEventDtos.Should().BeEmpty();

            result.TotalEventsCount.Should().Be(0);

            repositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_FilteredEvents_ByTitle()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var filter = new EventFilter()
            {
                Title = "M",
            };


            var filteredEvents = new List<Event>
            {
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "M",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow.AddDays(1),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                }
            };

            var pagedResult = new PagedResult<Event>
            {
                Page = 1,
                PageSize = 10,
                TotalCount = 1,
                Items = filteredEvents
            };

            repositoryMock.Setup(
                r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            result.ResponseEventDtos.Should().HaveCount(1);

            result.ResponseEventDtos[0].Title.Should().Be("M");

            repositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_FilteredEvents_ByDate()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var filter = new EventFilter
            {
                From = new DateTime(2026, 03, 09),
                To = new DateTime(2026, 03, 14)
            };

            var filteredEvents = new List<Event>
            {
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "Event one",
                    StartAt = new DateTime(2026, 03, 10),
                    EndAt = new DateTime(2026, 03, 11),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                },
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "Event two",
                    StartAt = new DateTime(2026, 03, 12),
                    EndAt = new DateTime(2026, 03, 13),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                },
            };

            var pagedResult = new PagedResult<Event>
            {
                Page = 1,
                PageSize = 10,
                TotalCount = 2,
                Items = filteredEvents
            };

            repositoryMock.Setup(
                r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            result.Should().NotBeNull();

            result.ResponseEventDtos.Should().HaveCount(2);

            repositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_PaginatedEvents()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var filter = new EventFilter
            {
                Page = 2,
                PageSize = 1,
            };

            var filteredEvents = new List<Event>
            {
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "Event one",
                    StartAt = new DateTime(2026, 03, 10),
                    EndAt = new DateTime(2026, 03, 11),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                },
            };

            var pagedResult = new PagedResult<Event>
            {
                Page = 2,
                PageSize = 1,
                TotalCount = 2,
                Items = filteredEvents
            };

            repositoryMock.Setup(
               r => r.GetEventsAsync(
                   It.IsAny<EventFilter>()))
               .ReturnsAsync(pagedResult);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            result.Should().NotBeNull();

            result.CurrentPage.Should().Be(2);
            result.NumberOnCurrentPage.Should().Be(1);
            result.TotalEventsCount.Should().Be(2);

            result.ResponseEventDtos.Should().HaveCount(1);

            result.ResponseEventDtos[0].Title.Should().Be("Event one");

            repositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEvents_ShouldReturns_FilteredAndPaginatedEvents()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var filter = new EventFilter()
            {
                Title = "E",
                From = new DateTime(2026, 03, 9),
                To = new DateTime(2026, 03, 14),
                Page = 2,
                PageSize = 2
            };

            var filteredEvents = new List<Event>
            {
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "Event one",
                    StartAt = new DateTime(2026, 03, 10),
                    EndAt = new DateTime(2026, 03, 11),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                },
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "Event two",
                    StartAt = new DateTime(2026, 03, 12),
                    EndAt = new DateTime(2026, 03, 13),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                },
            };

            var pagedResult = new PagedResult<Event>
            {
                Page = 2,
                PageSize = 2,
                TotalCount = 2,
                Items = filteredEvents
            };

            repositoryMock.Setup(
               r => r.GetEventsAsync(filter))
               .ReturnsAsync(pagedResult);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            result.Should().NotBeNull();

            result.CurrentPage.Should().Be(2);
            result.NumberOnCurrentPage.Should().Be(2);
            result.TotalEventsCount.Should().Be(2);

            result.ResponseEventDtos.Should().HaveCount(2);

            result.ResponseEventDtos
                .Select(e => e.Title)
                .Should()
                .Contain(["Event one", "Event two"]);

            repositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEvents_ShouldReturn_LastPage_WithRemainingItem()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var filter = new EventFilter()
            {
                Page = 2,
                PageSize = 1
            };

            var filteredEvents = new List<Event>
            {
                new()
                {
                    EventId = Guid.NewGuid(),
                    Title = "Event last",
                    StartAt = new DateTime(2026, 03, 12),
                    EndAt = new DateTime(2026, 03, 13),
                    TotalSeats = 1,
                    AvailableSeats = 1,
                },
            };

            var pagedResult = new PagedResult<Event>
            {
                Page = 2,
                PageSize = 1,
                TotalCount = 2,
                Items = filteredEvents
            };

            repositoryMock.Setup(
                r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var result = await service.GetEventsAsync(filter);

            // Assert (проверка)
            result.Should().NotBeNull();

            result.CurrentPage.Should().Be(2);
            result.NumberOnCurrentPage.Should().Be(1);
            result.TotalEventsCount.Should().Be(2);

            result.ResponseEventDtos.Should().HaveCount(1);

            result.ResponseEventDtos[0].Title.Should().Be("Event last");

            repositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEventById_ShouldReturn_Event_ById()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            Event fakeEvent = new Event()
            {
                EventId = eventId,
                Title = "Event one",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1,
            };

            repositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            var result = await service.GetEventByIdAsync(eventId);

            // Assert (проверка)
            result.Should().NotBeNull();

            result.EventId.Should().Be(eventId);
            result.Title.Should().Be("Event one");

            repositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId), 
                Times.Once);
        }

        [Fact]
        public async Task ChangeEvent_ShouldUpdate_Event()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            Event fakeEvent = new Event()
            {
                EventId = eventId,
                Title = "Event one",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1,
            };

            var eventDtoRequest = new EventDtoRequest()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 10, 24),
                EndAt = new DateTime(2026, 10, 25),
                TotalSeats = 1,
            };

            repositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId)).
                ReturnsAsync(fakeEvent);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)
            await service.UpdateEventAsync(eventId, eventDtoRequest);

            // Assert (проверка)
            fakeEvent.Title.Should().Be(eventDtoRequest.Title);
            fakeEvent.Description.Should().Be(eventDtoRequest.Description);
            fakeEvent.StartAt.Should().Be(eventDtoRequest.StartAt);
            fakeEvent.EndAt.Should().Be(eventDtoRequest.EndAt);

            repositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId),
                Times.Once);

            unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RemoveEvent_ShouldRemove_Event()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            Event fakeEvent = new Event()
            {
                EventId = eventId,
                Title = "Event one",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1,
            };

            repositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Act (действие)

            await service.RemoveEventAsync(eventId);

            // Assert (проверка)
            repositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId),
                Times.Once);

            repositoryMock.Verify(
                r => r.RemoveEvent(fakeEvent),
                Times.Once);

            unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(),
                Times.Once);
        }

        #endregion

        #region Unsuccessful scenarios for EventService

        [Fact]
        public async Task GetEventById_WithNonExisting_EventId_ShouldThrow_NotFoundException()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            repositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Assert (проверка)
            await service.Invoking(
                s => s.GetEventByIdAsync(eventId))
                .Should()
                .ThrowAsync<NotFoundException>();

            repositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task UpdateEvent_WithNonExisting_EventId_ShouldThrow_NotFoundExceptionAsync()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventId = Guid.NewGuid();

            repositoryMock.Setup(
                r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            var eventDtoRequest = new EventDtoRequest()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1
            };

            // Assert(проверка)
            await service.Invoking(s => s.UpdateEventAsync(new Guid(), eventDtoRequest))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task AddEvent_WithIncorrectData_Title_ShouldThrow_BadRequestException()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventDtoRequest = new EventDtoRequest
            {
                Title = string.Empty,
                Description = "NewDescription",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1
            };

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Assert(проверка)
            await service.Invoking(s => s.AddEventAsync(eventDtoRequest))
                .Should()
                .ThrowAsync<BadRequestException>();

            repositoryMock.Verify(
                r => r.AddEventAsync(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangeEvent_WithIncorrectData_EndAtEarlierStartAt_ShouldThrow_BadRequestException()
        {
            // Arrange (подготовка)
            var repositoryMock = new Mock<IEventRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var eventDtoRequest = new EventDtoRequest()
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 24),
                TotalSeats = 1
            };

            var service = new EventService(repositoryMock.Object, unitOfWorkMock.Object);

            // Assert(проверка)
            await service.Invoking(s => s.AddEventAsync(eventDtoRequest))
                .Should()
                .ThrowAsync<BadRequestException>();

            repositoryMock.Verify(
                r => r.AddEventAsync(It.IsAny<Event>()),
                Times.Never);
        }

        #endregion
    }
}
