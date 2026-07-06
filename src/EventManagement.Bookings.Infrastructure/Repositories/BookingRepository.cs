using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Entities;
using EventManagement.Bookings.Domain.Enums;
using EventManagement.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Bookings.Infrastructure.Repositories
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

        public Task<int> GetActiveBookingsCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _appDbContext.Bookings.CountAsync(b => b.UserId == userId && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed), cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
