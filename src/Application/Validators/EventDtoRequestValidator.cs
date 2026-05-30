using Application.Dtos.EventDtos;
using FluentValidation;

namespace Application.Validators
{
    public class EventDtoRequestValidator : AbstractValidator<EventDtoRequest>
    {
        public EventDtoRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Название события обязательно");

            RuleFor(x => x.TotalSeats)
                .GreaterThan(0)
                .WithMessage("Общее количество мест должно быть положительным числом!");

            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithMessage("Дата окончания события должна быть позже даты начала!");
        }
    }
}
