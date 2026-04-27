using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagementService.DataAccess.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            //Указание таблицы
            builder.ToTable("Events");

            //Указание PK
            builder.HasKey(e => e.EventId);

            //Указание, что первичный ключь обязателен и генерируется в коде
            builder.Property(e => e.EventId)
                .ValueGeneratedNever()
                .IsRequired();

            //Указание обязательного поля названия события с ограничением по кол-ву символов
            builder.Property(e => e.Title)
                .HasMaxLength(100)
                .IsRequired();

            //Указание необязательного поля описания события с ограничением по кол-ву символов
            builder.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsRequired(false);

            //Указание обязательного поля даты начала события
            builder.Property(e => e.StartAt)
                .IsRequired();

            //Указание необязательного поля даты окончания события
            builder.Property(e => e.EndAt)
                .IsRequired(false);

            //Указание обязательного поля общего количества мест на событие
            builder.Property(e => e.TotalSeats)
                .IsRequired();

            //Указание необязательного поля текущего количества мест на событие
            builder.Property(e => e.EndAt)
                .IsRequired(false);

            //Связь с Booking (один ко многим) с каскадным удалением
            builder.HasMany(e => e.Bookings)
                .WithOne()
                .HasForeignKey("EventId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}