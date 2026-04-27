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

            //Указание обязательного поля названия события
            builder.Property(e => e.Title)
                .IsRequired();

            
        }
    }
}
