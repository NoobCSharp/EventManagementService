using EventManagementService.Enums;
using EventManagementService.Models;

namespace EventManagementService.Tests
{
    public static class ServicesTestHelper
    {
        public static List<Event> CreateEvents() =>
        [
            new Event() { EventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"), Title = "AA", Description = "Description1", StartAt = new DateTime(2026, 03, 10), EndAt = new DateTime(2026, 03, 11) },
            new Event() { EventId = Guid.Parse("2F9619FF-8B86-D011-B42D-00C04FC964FF"), Title = "AB", Description = "Description2", StartAt = new DateTime(2026, 03, 12), EndAt = new DateTime(2026, 03, 13) },
            new Event() { EventId = Guid.Parse("3F9619FF-8B86-D011-B42D-00C04FC964FF"), Title = "BB", Description = "Description3", StartAt = new DateTime(2026, 03, 14), EndAt = new DateTime(2026, 03, 15) },
            new Event() { EventId = Guid.Parse("4F9619FF-8B86-D011-B42D-00C04FC964FF"), Title = "BC", Description = "Description4", StartAt = new DateTime(2026, 03, 16), EndAt = new DateTime(2026, 03, 17) },
            new Event() { EventId = Guid.Parse("5F9619FF-8B86-D011-B42D-00C04FC964FF"), Title = "CC", Description = "Description5", StartAt = new DateTime(2026, 03, 18), EndAt = new DateTime(2026, 03, 19) }
        ];

        public static List<Booking> CreateBookings() =>
        [
            new Booking() { BookingId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"), EventId = Guid.Parse("1F9619FF-8B86-D011-B42D-00C04FC964FF"), Status = BookingStatus.Pending, CreatedAt = new DateTime(2026, 03, 11), ProcessedAt = null },
            new Booking() { BookingId = Guid.Parse("2F9619FF-8B86-D011-B42D-00C04FC964FF"), EventId = Guid.Parse("2F9619FF-8B86-D011-B42D-00C04FC964FF"), Status = BookingStatus.Pending, CreatedAt = new DateTime(2026, 03, 13), ProcessedAt = null },
            new Booking() { BookingId = Guid.Parse("3F9619FF-8B86-D011-B42D-00C04FC964FF"), EventId = Guid.Parse("3F9619FF-8B86-D011-B42D-00C04FC964FF"), Status = BookingStatus.Pending, CreatedAt = new DateTime(2026, 03, 15), ProcessedAt = null },
        ];
    }
}
