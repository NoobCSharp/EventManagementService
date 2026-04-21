using EventManagementService.Enums;
using EventManagementService.Models;

namespace EventManagementService.Stores
{
    public class BookingStore : IBookingStore
    {
        public List<Booking> Bookings { get; set; } = [];

        public IEnumerable<Booking> GetPending()
        {
            return Bookings
                .Where(b => b.Status == BookingStatus.Pending && b.ProcessedAt == null);
        }

        public void Update(Booking booking)
        {
            // В текущей реализации метод Update не выполняет никаких действий, так как коллекция Bookings является списком в памяти.
        }
    }
}
