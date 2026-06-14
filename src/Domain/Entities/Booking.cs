using Domain.Enums;

namespace Domain.Entities
{
    public class Booking
    {
        /// <summary>
        /// Уникальный идентификатор брони
        /// </summary>
        public required Guid BookingId { get; set; }

        /// <summary>
        /// Идентификатор события, к которому относится бронь
        /// </summary>
        public required Guid EventId { get; set; }

        /// <summary>
        /// Идентификатор пользователя, который создал бронь
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// Текущий статус брони
        /// </summary>
        public required BookingStatus Status { get; set; }

        /// <summary>
        /// Дата и время создания брони
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время обработки
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Событие, к которому относится бронь
        /// </summary>
        public required Event Event { get; set; }

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

        // TODO Еще подумать над этим методом, нужна защита от повторного вызова, например, если бронь уже подтверждена или отклонена, то нельзя ее отменить
        public void Cancel(DateTime processedAt)
        {
            Status = BookingStatus.Cancelled;
            ProcessedAt = processedAt;
        }
    }
}