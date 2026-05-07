using EventManagementService.Dtos.EventDtos;
using EventManagementService.Exceptions;
using EventManagementService.Filters;
using EventManagementService.Mappers;
using EventManagementService.Models;
using EventManagementService.Repositories;

namespace EventManagementService.Services
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

        public async Task<EventDtoPaginatedResponse> GetEventsAsync(EventFilter eventFilter, CancellationToken cancellationToken = default)
        {
            return await _eventRepository.GetEventsAsync(eventFilter, cancellationToken);   
        }

        public async Task<EventDtoResponse> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            return EventMapper.EventToResponse(existingEvent);
        }

        public async Task<EventDtoResponse> AddEventAsync(EventDtoRequest requestEventDto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(requestEventDto.Title))
                throw new BadRequestException("Название события обязательно к заполнению!");
           
            if (requestEventDto.EndAt <= requestEventDto.StartAt)
                throw new BadRequestException("Дата окончания события не может быть раньше даты начала события!");

            if (requestEventDto.TotalSeats <= 0)
                throw new BadRequestException("Общее количество мест должно быть положительным числом!");

            Event @event = new()
            {
                EventId = Guid.NewGuid(),
                Title = requestEventDto.Title,
                Description = requestEventDto.Description,
                StartAt = requestEventDto.StartAt,
                EndAt = requestEventDto.EndAt,
                TotalSeats = requestEventDto.TotalSeats,
                AvailableSeats = requestEventDto.TotalSeats
            };

            await _eventRepository.AddEventAsync(@event, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EventMapper.EventToResponse(@event);
        }

        public async Task UpdateEventAsync(Guid id, EventDtoRequest requestEventDto, CancellationToken cancellationToken = default)
        {
            if (requestEventDto.EndAt <= requestEventDto.StartAt)
                throw new BadRequestException("Дата окончания события не может быть раньше даты начала события!");

            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            existingEvent.Title = requestEventDto.Title;
            existingEvent.Description = requestEventDto.Description;
            existingEvent.StartAt = requestEventDto.StartAt;
            existingEvent.EndAt = requestEventDto.EndAt;

            await _eventRepository.UpdateEventAsync(existingEvent, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveEventAsync(Guid id, CancellationToken cancellationToken = default)
        { 
            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);

            if (existingEvent is null)
                throw new NotFoundException("Событие по указанному Id не найдено!");

            await _eventRepository.RemoveEventAsync(existingEvent, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
