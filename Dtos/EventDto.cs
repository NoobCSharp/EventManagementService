using System.ComponentModel.DataAnnotations;

namespace EventManagementService.Dtos
{
    /// <summary>
    /// DTO событие для CRUD операций создание/обновление
    /// </summary>
    public class EventDto
    {
        [Required(ErrorMessage = "Название события обязательно")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Дата начала события обязательна")]
        public DateTime? StartAt { get; set; }

        [Required(ErrorMessage = "Дата окончания события обязательна")]
        public DateTime? EndAt { get; set; }
    }
}
