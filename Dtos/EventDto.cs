using System.ComponentModel.DataAnnotations;

namespace EventManagementService.Dtos
{
    /// <summary>
    /// DTO событие для CRUD операций создание/обновление
    /// </summary>
    public class EventDto : IValidatableObject
    {
        /// <summary>
        /// Название события.
        /// </summary>
        [Required(ErrorMessage = "Название события обязательно")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Описание события.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Дата начала события.
        /// </summary>
        [Required]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата окончания события.
        /// </summary>
        [Required]
        public DateTime EndAt { get; set; }

        /// <summary>
        /// Метод проверяет, что дата окончания EndAt больше даты начала StartAt.
        /// </summary>
        /// <param name="validationContext">Контекст валидации.</param>
        /// <returns>Коллекция ошибок валидации, если параметры заданы некорректно.</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt <= StartAt)
            {
                yield return new ValidationResult(
                    "Дата окончания события не может быть раньше даты начала события!");
            }
        }
    }
}
