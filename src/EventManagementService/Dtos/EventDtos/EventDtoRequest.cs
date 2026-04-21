using System.ComponentModel.DataAnnotations;

namespace EventManagementService.Dtos.EventDtos
{
    /// <summary>
    /// DTO событие для запросов
    /// </summary>
    public record EventDtoRequest : IValidatableObject
    {
        /// <summary>
        /// Название события.
        /// </summary>
        [Required(ErrorMessage = "Название события обязательно")]
        required public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Описание события.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Дата начала события.
        /// </summary>
        [Required]
        public required DateTime StartAt { get; init; }

        /// <summary>
        /// Дата окончания события.
        /// </summary>
        [Required]
        public required DateTime EndAt { get; init; }

        /// <summary>
        /// Общее количество мест на событие.
        /// </summary>
        [Required]
        public int TotalSeats { get; init; }

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

            if (TotalSeats <= 0)
            {
                yield return new ValidationResult(
                    "Общее количество мест должно быть положительным числом!");
            }
        }
    }
}
