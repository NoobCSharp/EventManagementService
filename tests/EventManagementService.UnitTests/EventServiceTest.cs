using EventManagement.Events.Domain.Entities;
using EventManagement.Events.Domain.Exceptions;
using EventManagement.Events.Infrastructure.Common;
using EventManagement.Events.Infrastructure.Dtos;
using EventManagement.Events.Infrastructure.Filters;
using EventManagement.Events.Infrastructure.Interfaces;
using EventManagement.Events.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace EventManagementService.UnitTests
{
    public class EventServiceTest
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock = new();

        private EventService CreateEventService()
        {
            return new EventService(_eventRepositoryMock.Object);
        }

        #region Successful scenarios for EventService

        [Fact]
        public async Task AddEventAsync_ShouldAdd_Event()
        {
            // Arrange
            var request = new EventDtoRequest
            {
                Title = "Test",
                Description = "TestDescription",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
            };

            var service = CreateEventService();

            // Act
            var response = await service.AddEventAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.EventId.Should().NotBeEmpty();

            _eventRepositoryMock.Verify(
                r => r.AddEventAsync(It.Is<Event>(e =>
                    e.Title == request.Title &&
                    e.Description == request.Description &&
                    e.StartAt == request.StartAt &&
                    e.EndAt == request.EndAt &&
                    e.TotalSeats == request.TotalSeats &&
                    e.AvailableSeats == request.TotalSeats)),
                Times.Once);

            _eventRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturns_AllEvents_When_FilterIsEmpty()
        {
            // Arrange
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

            _eventRepositoryMock
                .Setup(r => r.GetEventsAsync(It.IsAny<EventFilter>()))
                .ReturnsAsync(pagedResult);

            var service = CreateEventService();

            var filter = new EventFilter();

            // Act
            var result = await service.GetEventsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.ResponseEventDtos.Should().HaveCount(2);

            result.ResponseEventDtos[0].Title.Should().Be("Event one");
            result.ResponseEventDtos[1].Title.Should().Be("Event two");

            _eventRepositoryMock.Verify(
                r => r.GetEventsAsync(It.IsAny<EventFilter>()),
                Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturn_EmptyCollection_WhenFilterHasNoMatches()
        {
            // Arrange
            var filter = new EventFilter
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

            _eventRepositoryMock
                .Setup(r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = CreateEventService();

            // Act
            var result = await service.GetEventsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.ResponseEventDtos.Should().BeEmpty();
            result.TotalEventsCount.Should().Be(0);

            _eventRepositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturns_FilteredEvents_ByTitle()
        {
            // Arrange
            var filter = new EventFilter
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

            _eventRepositoryMock
                .Setup(r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = CreateEventService();

            // Act
            var result = await service.GetEventsAsync(filter);

            // Assert
            result.ResponseEventDtos.Should().HaveCount(1);
            result.ResponseEventDtos[0].Title.Should().Be("M");

            _eventRepositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturns_FilteredEvents_ByDate()
        {
            // Arrange
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

            _eventRepositoryMock
                .Setup(r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = CreateEventService();

            // Act
            var result = await service.GetEventsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.ResponseEventDtos.Should().HaveCount(2);

            _eventRepositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturns_PaginatedEvents()
        {
            // Arrange
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

            _eventRepositoryMock
                .Setup(r => r.GetEventsAsync(It.IsAny<EventFilter>()))
                .ReturnsAsync(pagedResult);

            var service = CreateEventService();

            // Act
            var result = await service.GetEventsAsync(filter);

            // Assert
            result.CurrentPage.Should().Be(2);
            result.NumberOnCurrentPage.Should().Be(1);
            result.TotalEventsCount.Should().Be(2);
            result.ResponseEventDtos.Should().HaveCount(1);

            _eventRepositoryMock.Verify(
                r => r.GetEventsAsync(It.IsAny<EventFilter>()),
                Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturns_FilteredAndPaginatedEvents()
        {
            // Arrange
            var filter = new EventFilter
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

            _eventRepositoryMock
                .Setup(r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = CreateEventService();

            // Act
            var result = await service.GetEventsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.CurrentPage.Should().Be(2);
            result.NumberOnCurrentPage.Should().Be(2);
            result.TotalEventsCount.Should().Be(2);
            result.ResponseEventDtos.Should().HaveCount(2);

            result.ResponseEventDtos
                .Select(e => e.Title)
                .Should()
                .Contain(new[] { "Event one", "Event two" });

            _eventRepositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturn_LastPage_WithRemainingItem()
        {
            // Arrange
            var filter = new EventFilter
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

            _eventRepositoryMock
                .Setup(r => r.GetEventsAsync(filter))
                .ReturnsAsync(pagedResult);

            var service = CreateEventService();

            // Act
            var result = await service.GetEventsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.CurrentPage.Should().Be(2);
            result.NumberOnCurrentPage.Should().Be(1);
            result.TotalEventsCount.Should().Be(2);
            result.ResponseEventDtos.Should().HaveCount(1);
            result.ResponseEventDtos[0].Title.Should().Be("Event last");

            _eventRepositoryMock.Verify(
                r => r.GetEventsAsync(filter),
                Times.Once);
        }

        [Fact]
        public async Task GetEventByIdAsync_ShouldReturn_Event_ById()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Event one",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1,
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateEventService();

            // Act
            var result = await service.GetEventByIdAsync(eventId);

            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().Be(eventId);

            _eventRepositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_ShouldUpdate_AllFields_AndRecalculateAvailableSeats()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var existingEvent = new Event
            {
                EventId = eventId,
                Title = "Old Title",
                Description = "Old Description",
                StartAt = new DateTime(2026, 01, 01),
                EndAt = new DateTime(2026, 01, 02),
                TotalSeats = 10,
                AvailableSeats = 6
            };

            var request = new EventDtoRequest
            {
                Title = "New Title",
                Description = "New Description",
                StartAt = new DateTime(2026, 02, 01),
                EndAt = new DateTime(2026, 02, 02),
                TotalSeats = 12
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);

            var service = CreateEventService();

            // Act
            await service.UpdateEventAsync(eventId, request);

            // Assert
            existingEvent.Title.Should().Be(request.Title);
            existingEvent.Description.Should().Be(request.Description);
            existingEvent.TotalSeats.Should().Be(request.TotalSeats);

            _eventRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveEventAsync_ShouldRemove_Event()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var fakeEvent = new Event
            {
                EventId = eventId,
                Title = "Event one",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1,
                AvailableSeats = 1,
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(fakeEvent);

            var service = CreateEventService();

            // Act
            await service.RemoveEventAsync(eventId);

            // Assert
            _eventRepositoryMock.Verify(r => r.RemoveEvent(fakeEvent), Times.Once);
            _eventRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region Unsuccessful scenarios for EventService

        [Fact]
        public async Task GetEventByIdAsync_WithNonExisting_EventId_ShouldThrow_NotFoundException()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            var service = CreateEventService();

            // Act & Assert
            await service.Invoking(s => s.GetEventByIdAsync(eventId))
                .Should()
                .ThrowAsync<EventNotFoundException>();

            _eventRepositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_WithNonExisting_EventId_ShouldThrow_EventNotFoundException()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            var service = CreateEventService();

            var eventDtoRequest = new EventDtoRequest
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1
            };

            // Act & Assert
            await service.Invoking(s => s.UpdateEventAsync(eventId, eventDtoRequest))
                .Should()
                .ThrowAsync<EventNotFoundException>();

            _eventRepositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task AddEventAsync_WithIncorrectData_Title_ShouldThrow_BadRequestException()
        {
            // Arrange
            var eventDtoRequest = new EventDtoRequest
            {
                Title = string.Empty,
                Description = "NewDescription",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1
            };

            var service = CreateEventService();

            // Act & Assert
            await service.Invoking(s => s.AddEventAsync(eventDtoRequest))
                .Should()
                .ThrowAsync<EventValidationException>();

            _eventRepositoryMock.Verify(
                r => r.AddEventAsync(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateEventAsync_WithIncorrectData_EndAtEarlierStartAt_ShouldThrow_EventValidationException()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var eventDtoRequest = new EventDtoRequest
            {
                Title = "NewTitle",
                Description = "NewDescription",
                StartAt = new DateTime(2026, 3, 25),
                EndAt = new DateTime(2026, 3, 24),
                TotalSeats = 1
            };

            var service = CreateEventService();

            // Act & Assert
            await service.Invoking(s => s.UpdateEventAsync(eventId, eventDtoRequest))
                .Should()
                .ThrowAsync<EventValidationException>();

            _eventRepositoryMock.Verify(
                r => r.GetEventByIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _eventRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task AddEventAsync_WhenTotalSeatsIsZero_ShouldThrow_EventValidationException()
        {
            // Arrange (подготовка)
            var eventDtoRequest = new EventDtoRequest
            {
                Title = "New Event",
                Description = "Description",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 0
            };

            var service = CreateEventService();

            // Act & Assert (действие и проверка)
            await service
                .Invoking(s => s.AddEventAsync(eventDtoRequest))
                .Should()
                .ThrowAsync<EventValidationException>();

            _eventRepositoryMock.Verify(
                r => r.AddEventAsync(It.IsAny<Event>()),
                Times.Never);

            _eventRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task RemoveEvent_WithNonExistingId_ShouldThrow_EventNotFoundException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            var service = CreateEventService();

            // Act & Assert (действие и проверка)
            await service
                .Invoking(s => s.RemoveEventAsync(eventId))
                .Should()
                .ThrowAsync<EventNotFoundException>();

            _eventRepositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId),
                Times.Once);

            _eventRepositoryMock.Verify(
                r => r.RemoveEvent(It.IsAny<Event>()),
                Times.Never);

            _eventRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task UpdateEvent_WhenTotalSeatsLessThanBookedSeats_ShouldThrow_EventValidationException()
        {
            // Arrange (подготовка)
            var eventId = Guid.NewGuid();

            var existingEvent = new Event
            {
                EventId = eventId,
                Title = "Event",
                Description = "Description",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 10,
                AvailableSeats = 2
            };

            var eventDtoRequest = new EventDtoRequest
            {
                Title = "Updated Event",
                Description = "Updated Description",
                StartAt = existingEvent.StartAt,
                EndAt = existingEvent.EndAt,
                TotalSeats = 5
            };

            _eventRepositoryMock
                .Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);

            var service = CreateEventService();

            // Act & Assert (действие и проверка)
            await service
                .Invoking(s => s.UpdateEventAsync(eventId, eventDtoRequest))
                .Should()
                .ThrowAsync<EventValidationException>();

            _eventRepositoryMock.Verify(
                r => r.GetEventByIdAsync(eventId),
                Times.Once);

            _eventRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        #endregion
    }
}
