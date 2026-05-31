using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.EventDtos
{
    public record EventDtoRequest : IValidatableObject
    {
        /// <summary>
        /// Название события
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Описание события
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Дата начала события
        /// </summary>
        public required DateTime StartAt { get; init; }

        /// <summary>
        /// Дата окончания события
        /// </summary>
        public required DateTime EndAt { get; init; }

        /// <summary>
        /// Общее количество мест на событие
        /// </summary>
        public required int TotalSeats { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Title)) 
            {
                yield return new ValidationResult(
                    "Название события не может быть пустым!");
            }
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
