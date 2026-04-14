using EventManagementService.Enums;

namespace EventManagementService.Models
{
    public class Booking
    {
        /// <summary>
        /// Уникальный идентификатор брони
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор события, к которому относится бронь
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Текущий статус брони
        /// </summary>
        public BookingStatus Status { get; set; }

        /// <summary>
        /// Дата и время создания брони
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время обработки
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

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