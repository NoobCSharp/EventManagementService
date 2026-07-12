using EventManagement.Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Bookings.Infrastructure.DataAccess
{
    public class BookingsDbContext : DbContext
    {
        public BookingsDbContext(DbContextOptions<BookingsDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsDbContext).Assembly);
        }
    }
}
