using Application.Dtos.EventDtos;
using Application.Filters;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class EventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EventService(IEventRepository eventRepository, IUnitOfWork unitOfWork)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Получение списка событий с поддержкой фильтрации по названию и диапазону дат, а также с поддержкой пагинации.
        /// </summary>
        /// <param name="eventFilter">Фильтр для поиска событий.</param>
        /// <returns>Список отфильтрованных событий с поддержкой пагинации.</returns>
        public async Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken = default)
        {
            var pagedResult = await _eventRepository.GetEventsAsync(eventFilter, cancellationToken);

            return new EventDtoPaginatedResponse
            {
                CurrentPage = pagedResult.Page,
                NumberOnCurrentPage = pagedResult.PageSize,
                TotalEventsCount = pagedResult.TotalCount,
                ResponseEventDtos = pagedResult.Items
                    .Select(EventMapper.EventToResponse)
                    .ToList(),
            };
        }

        /// <summary>
        /// Получение события по его уникальному идентификатору. 
        /// Если событие с указанным Id не найдено, будет выброшено исключение NotFoundException.
        /// </summary>
        /// <param name="id">Уникальный идентификатор события.</param>
        /// <returns>Объект события.</returns>
        public async Task<EventDtoResponse> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            return EventMapper.EventToResponse(existingEvent);
        }

        /// <summary>
        /// Добавление нового события. При добавлении события выполняются следующие проверки:
        /// - Название события обязательно к заполнению.
        /// - Дата окончания события не может быть раньше даты начала события.
        /// - Общее количество мест должно быть положительным числом.
        /// Если событие некорректно, будет выброшено исключение BadRequestException.
        /// </summary>
        /// <param name="eventDtoRequest">Данные для создания нового события.</param>
        /// <returns>Созданное событие.</returns>
        public async Task<EventDtoResponse> AddEventAsync(EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventDtoRequest.Title))
                throw new BadRequestException("Название события обязательно к заполнению!");

            if (eventDtoRequest.EndAt <= eventDtoRequest.StartAt)
                throw new BadRequestException("Дата окончания события не может быть раньше даты начала события!");

            if (eventDtoRequest.TotalSeats <= 0)
                throw new BadRequestException("Общее количество мест должно быть положительным числом!");

            Event @event = new()
            {
                EventId = Guid.NewGuid(),
                Title = eventDtoRequest.Title,
                Description = eventDtoRequest.Description,
                StartAt = eventDtoRequest.StartAt,
                EndAt = eventDtoRequest.EndAt,
                TotalSeats = eventDtoRequest.TotalSeats,
                AvailableSeats = eventDtoRequest.TotalSeats
            };

            await _eventRepository.AddEventAsync(@event, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EventMapper.EventToResponse(@event);
        }

        /// <summary>
        /// Обновление существующего события. При обновлении события выполняются следующие проверки:
        /// - Дата окончания события не может быть раньше даты начала события.
        /// - Общее количество мест не может быть меньше количества уже забронированных мест.
        /// Если событие некорректно, будет выброшено исключение BadRequestException.
        /// Если событие не найдено, будет выброшено исключение NotFoundException.
        /// </summary>
        /// <param name="id">Идентификатор события для обновления.</param>
        /// <param name="eventDtoRequest">Данные для обновления события.</param>
        public async Task UpdateEventAsync(Guid id, EventDtoRequest eventDtoRequest, CancellationToken cancellationToken = default)
        {
            if (eventDtoRequest.EndAt <= eventDtoRequest.StartAt)
                throw new BadRequestException("Дата окончания события не может быть раньше даты начала события!");

            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            // TODO подумать над тем, что будет,
            // если при обновлении события общее количество мест будет меньше,
            // чем количество уже забронированных мест.
            if (eventDtoRequest.TotalSeats < existingEvent.AvailableSeats)
                throw new BadRequestException("Общее количество мест не может быть меньше количества уже забронированных мест!");

            existingEvent.Title = eventDtoRequest.Title;
            existingEvent.Description = eventDtoRequest.Description;
            existingEvent.StartAt = eventDtoRequest.StartAt;
            existingEvent.EndAt = eventDtoRequest.EndAt;
            existingEvent.TotalSeats = eventDtoRequest.TotalSeats;
            existingEvent.AvailableSeats = eventDtoRequest.TotalSeats - existingEvent.AvailableSeats;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Удаляет событие по его уникальному идентификатору.
        /// Если событие с указанным Id не найдено, будет выброшено исключение NotFoundException.
        /// </summary>
        /// <param name="id">Идентификатор события для удаления.</param>
        public async Task RemoveEventAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            _eventRepository.RemoveEvent(existingEvent);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
