using EventManagementService.Models;

namespace EventManagementService.Stores
{
    public class BookingStore : IBookingStore
    {
        /// <summary>
        /// Коллекция броней в хранилище
        /// </summary>
        public List<Booking> Bookings { get; set; } = [];
    }
}
