using EventManagement.Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagement.Bookings.Infrastructure.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            //Указание таблицы
            builder.ToTable("Bookings");

            //Указание PK
            builder.HasKey(b => b.BookingId);

            //Указание, что первичный ключ обязателен и генерируется в коде
            builder.Property(b => b.BookingId)
                .ValueGeneratedNever()
                .IsRequired();

            //Указание обязательного поля внешнего ключа к Event
            builder.Property(b => b.EventId)
                .IsRequired();

            //Указание обязательного поля внешнего ключа к User
            builder.Property(x => x.UserId)
                .IsRequired();

            //Указание обязательного поля статуса с конвертацией enum -> string
            builder.Property(b => b.Status)
                .IsRequired()
                .HasConversion<string>();

            //Указание обязательного поля количества мест бронирования
            builder.Property(b => b.SeatCount)
                .IsRequired();

            //Указание обязательного поля даты и времени создания брони
            builder.Property(b => b.CreatedAt)
                .IsRequired();

            //Указание необязательного поля даты и времени обработки брони
            builder.Property(b => b.ProcessedAt)
                .IsRequired(false);
        }
    }
}