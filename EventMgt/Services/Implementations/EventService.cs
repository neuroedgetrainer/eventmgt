using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers.Common;
using EventMgt.Models;
using EventMgt.Repositories.Interfaces;
using EventMgt.Services.Interfaces;

namespace EventMgt.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IVenueRepository _venueRepo;
        private readonly IUserRepository _userRepo;

        public EventService(
            IEventRepository eventRepo,
            ICategoryRepository categoryRepo,
            IVenueRepository venueRepo,
            IUserRepository userRepo)
        {
            _eventRepo = eventRepo;
            _categoryRepo = categoryRepo;
            _venueRepo = venueRepo;
            _userRepo = userRepo;
        }

        public async Task<ServiceResult<PagedResult<EventDto>>> GetEventsAsync(EventQueryParameters queryParams)
        {
            var pagedEvents = await _eventRepo.GetEventsPagedAsync(queryParams);
            var dtos = pagedEvents.Items.Select(MapToDto).ToList();

            var result = new PagedResult<EventDto>(
                dtos,
                pagedEvents.TotalCount,
                pagedEvents.PageNumber,
                pagedEvents.PageSize);

            return ServiceResult<PagedResult<EventDto>>.Success(result);
        }

        public async Task<ServiceResult<EventDto>> GetByIdAsync(int id)
        {
            var eventEntity = await _eventRepo.GetEventWithDetailsAsync(id);
            if (eventEntity == null)
                return ServiceResult<EventDto>.Failure("Event not found.", 404);

            return ServiceResult<EventDto>.Success(MapToDto(eventEntity));
        }

        public async Task<ServiceResult<IReadOnlyList<EventDto>>> GetEventsByOrganizerAsync(int organizerId)
        {
            var events = await _eventRepo.GetEventsByOrganizerAsync(organizerId);
            var dtos = events.Select(MapToDto).ToList();
            return ServiceResult<IReadOnlyList<EventDto>>.Success(dtos);
        }

        public async Task<ServiceResult<EventDto>> CreateAsync(int currentUserId, EventDto dto)
        {
            // 1. Date Validation
            if (dto.EndDate <= dto.StartDate)
                return ServiceResult<EventDto>.Failure("End date must be greater than start date.", 400);

            if (dto.StartDate < DateTime.UtcNow)
                return ServiceResult<EventDto>.Failure("Event start date cannot be in the past.", 400);

            // 2. Validate Category
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null || !category.IsActive)
                return ServiceResult<EventDto>.Failure("Selected category is invalid or inactive.", 400);

            // 3. Validate Venue & Capacity
            var venue = await _venueRepo.GetByIdAsync(dto.VenueId.Value);
            if (venue == null || !venue.IsActive)
                return ServiceResult<EventDto>.Failure("Selected venue is invalid or inactive.", 400);

            if (dto.MaxParticipants > venue.Capacity)
                return ServiceResult<EventDto>.Failure($"Max participants ({dto.MaxParticipants}) cannot exceed venue capacity ({venue.Capacity}).", 400);

            // 4. Check for venue schedule overlap
            var hasConflict = await _eventRepo.HasScheduleConflictAsync(dto.VenueId.Value, dto.StartDate, dto.EndDate);
            if (hasConflict)
                return ServiceResult<EventDto>.Failure("The selected venue already has a scheduled event during this timeframe.", 409);

            var eventEntity = new Event
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CategoryId = dto.CategoryId,
                VenueId = dto.VenueId,
                OrganizerId = currentUserId, // Enforce authenticated creator identity
                MaxParticipants = dto.MaxParticipants,
                Status = Helpers.EventStatus.Published,
                RegistrationStartDate = dto.RegistrationStartDate,
                RegistrationEndDate = dto.RegistrationEndDate,
                EventImageUrl = dto.EventImageUrl?.Trim(),
                IsCancelled = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _eventRepo.AddAsync(eventEntity);
            await _eventRepo.SaveChangesAsync();

            var createdEvent = await _eventRepo.GetEventWithDetailsAsync(eventEntity.Id);
            return ServiceResult<EventDto>.Success(MapToDto(createdEvent!), 201);
        }

        public async Task<ServiceResult<EventDto>> UpdateAsync(int eventId, int currentUserId, bool isAdmin, EventDto dto)
        {
            var eventEntity = await _eventRepo.GetEventWithDetailsAsync(eventId);
            if (eventEntity == null)
                return ServiceResult<EventDto>.Failure("Event not found.", 404);

            // Authorization check: Only Admin or the event owner can modify
            if (!isAdmin && eventEntity.OrganizerId != currentUserId)
                return ServiceResult<EventDto>.Failure("You are not authorized to edit this event.", 403);

            if (eventEntity.IsCancelled)
                return ServiceResult<EventDto>.Failure("Cancelled events cannot be modified.", 400);

            if (dto.EndDate <= dto.StartDate)
                return ServiceResult<EventDto>.Failure("End date must be greater than start date.", 400);

            // Venue & Capacity check
            var venue = await _venueRepo.GetByIdAsync(dto.VenueId.Value);
            if (venue == null || !venue.IsActive)
                return ServiceResult<EventDto>.Failure("Selected venue is invalid or inactive.", 400);

            if (dto.MaxParticipants > venue.Capacity)
                return ServiceResult<EventDto>.Failure($"Max participants ({dto.MaxParticipants}) cannot exceed venue capacity ({venue.Capacity}).", 400);

            var activeRegistrationsCount = await _eventRepo.GetActiveRegistrationsCountAsync(eventId);
            if (dto.MaxParticipants < activeRegistrationsCount)
                return ServiceResult<EventDto>.Failure($"Max participants cannot be reduced below existing registrations ({activeRegistrationsCount}).", 400);

            // Venue schedule conflict check excluding self
            var hasConflict = await _eventRepo.HasScheduleConflictAsync(dto.VenueId.Value, dto.StartDate, dto.EndDate, eventId);
            if (hasConflict)
                return ServiceResult<EventDto>.Failure("The selected venue already has a scheduled event during this timeframe.", 409);

            // Apply updates
            eventEntity.Title = dto.Title.Trim();
            eventEntity.Description = dto.Description?.Trim();
            eventEntity.StartDate = dto.StartDate;
            eventEntity.EndDate = dto.EndDate;
            eventEntity.CategoryId = dto.CategoryId;
            eventEntity.VenueId = dto.VenueId;
            eventEntity.MaxParticipants = dto.MaxParticipants;
            eventEntity.RegistrationStartDate = dto.RegistrationStartDate;
            eventEntity.RegistrationEndDate = dto.RegistrationEndDate;
            eventEntity.EventImageUrl = dto.EventImageUrl?.Trim();
            eventEntity.IsActive = dto.IsActive;
            eventEntity.UpdatedDate = DateTime.UtcNow;

            _eventRepo.Update(eventEntity);
            await _eventRepo.SaveChangesAsync();

            var updatedEvent = await _eventRepo.GetEventWithDetailsAsync(eventId);
            return ServiceResult<EventDto>.Success(MapToDto(updatedEvent!));
        }

        public async Task<ServiceResult<bool>> CancelEventAsync(int eventId, int currentUserId, bool isAdmin, string cancellationReason)
        {
            var eventEntity = await _eventRepo.GetByIdAsync(eventId);
            if (eventEntity == null)
                return ServiceResult<bool>.Failure("Event not found.", 404);

            if (!isAdmin && eventEntity.OrganizerId != currentUserId)
                return ServiceResult<bool>.Failure("You are not authorized to cancel this event.", 403);

            if (eventEntity.IsCancelled)
                return ServiceResult<bool>.Failure("Event is already cancelled.", 400);

            eventEntity.IsCancelled = true;
            eventEntity.Status = Helpers.EventStatus.Cancelled;
            eventEntity.CancellationReason = string.IsNullOrWhiteSpace(cancellationReason)
                ? "Cancelled by organizer"
                : cancellationReason.Trim();
            eventEntity.UpdatedDate = DateTime.UtcNow;

            _eventRepo.Update(eventEntity);
            await _eventRepo.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> SoftDeleteAsync(int eventId, int currentUserId, bool isAdmin)
        {
            var eventEntity = await _eventRepo.GetByIdAsync(eventId);
            if (eventEntity == null)
                return ServiceResult<bool>.Failure("Event not found.", 404);

            if (!isAdmin && eventEntity.OrganizerId != currentUserId)
                return ServiceResult<bool>.Failure("You are not authorized to delete this event.", 403);

            eventEntity.IsActive = false;
            eventEntity.UpdatedDate = DateTime.UtcNow;

            _eventRepo.Update(eventEntity);
            await _eventRepo.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        private static EventDto MapToDto(Event e)
        {
            var registeredCount = e.Registrations?.Count(r => r.Status == Helpers.RegistrationStatus.Registered) ?? 0;

            return new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name,
                VenueId = e.VenueId,
                VenueName = e.Venue?.Name,
                OrganizerId = e.OrganizerId,
                OrganizerName = e.Organizer?.Name,
                OrganizerEmail = e.Organizer?.Email,
                MaxParticipants = e.MaxParticipants,
                Status = e.Status.ToString(),
                RegistrationStartDate = e.RegistrationStartDate,
                RegistrationEndDate = e.RegistrationEndDate,
                EventImageUrl = e.EventImageUrl,
                IsCancelled = e.IsCancelled,
                CancellationReason = e.CancellationReason,
                IsActive = e.IsActive,
                CreatedDate = e.CreatedDate,
                UpdatedDate = e.UpdatedDate,
                RegisteredParticipantsCount = registeredCount
            };
        }
    }
}
