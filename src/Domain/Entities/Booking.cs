using Domain.Enums;

namespace Domain.Entities
{
    public class Booking
    {
        /// <summary>
        /// Уникальный идентификатор брони
        /// </summary>
        required public Guid BookingId { get; set; }

        /// <summary>
        /// Идентификатор события, к которому относится бронь
        /// </summary>
        required public Guid EventId { get; set; }

        /// <summary>
        /// Текущий статус брони
        /// </summary>
        required public BookingStatus Status { get; set; }

        /// <summary>
        /// Дата и время создания брони
        /// </summary>
        required public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время обработки
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Событие, к которому относится бронь
        /// </summary>
        required public Event Event { get; set; }

        public Booking()
        {
        }

        public void Reject(DateTime processedAt)
        {
            Status = BookingStatus.Rejected;
            ProcessedAt = processedAt;
        }
            
        public void Confirm(DateTime processedAt)
        {
            Status = BookingStatus.Confirmed;
            ProcessedAt = processedAt;
        }  
    }
}