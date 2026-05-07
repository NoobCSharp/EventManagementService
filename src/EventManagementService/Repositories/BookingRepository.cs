using EventManagementService.DataAccess;
using EventManagementService.Enums;
using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _appDbContext;

        public BookingRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Bookings.AddAsync(booking, cancellationToken);
        }

        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetPendingBookingsAsync(CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Bookings.Where(b => b.Status == BookingStatus.Pending).ToListAsync(cancellationToken);
        }
    }
}
