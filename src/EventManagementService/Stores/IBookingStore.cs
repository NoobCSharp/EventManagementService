using EventManagementService.Models;

namespace EventManagementService.Stores
{
    public interface IBookingStore
    {
        /// <summary>
        /// Коллекция броней в хранилище
        /// </summary>
        public List<Booking> Bookings { get; set; }
    }
}
