using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers.Common;
using EventMgt.Models;
using EventMgt.Repositories.Interfaces;
using EventMgt.Services.Interfaces;

namespace EventMgt.Services.Implementations
{
    public class EventRegistrationService : IEventRegistrationService
    {
        private readonly IEventRegistrationRepository _registrationRepo;
        private readonly IEventRepository _eventRepo;
        private readonly IUserRepository _userRepo;

        public EventRegistrationService(
            IEventRegistrationRepository registrationRepo,
            IEventRepository eventRepo,
            IUserRepository userRepo)
        {
            _registrationRepo = registrationRepo;
            _eventRepo = eventRepo;
            _userRepo = userRepo;
        }

        public async Task<ServiceResult<EventRegistrationDto>> RegisterAsync(int currentUserId, EventRegistrationDto dto)
        {
            var targetEvent = await _eventRepo.GetEventWithDetailsAsync(dto.EventId);
            if (targetEvent == null || !targetEvent.IsActive)
                return ServiceResult<EventRegistrationDto>.Failure("Event does not exist or is inactive.", 404);

            if (targetEvent.IsCancelled)
                return ServiceResult<EventRegistrationDto>.Failure("Cannot register for a cancelled event.", 400);

            var now = DateTime.UtcNow;

            if (targetEvent.StartDate <= now)
                return ServiceResult<EventRegistrationDto>.Failure("Cannot register for an event that has already started or completed.", 400);

            if (targetEvent.RegistrationStartDate.HasValue && now < targetEvent.RegistrationStartDate.Value)
                return ServiceResult<EventRegistrationDto>.Failure("Registration for this event has not opened yet.", 400);

            if (targetEvent.RegistrationEndDate.HasValue && now > targetEvent.RegistrationEndDate.Value)
                return ServiceResult<EventRegistrationDto>.Failure("Registration for this event has closed.", 400);

            // Capacity check
            var activeCount = await _registrationRepo.GetActiveCountForEventAsync(dto.EventId);
            if (activeCount >= targetEvent.MaxParticipants)
                return ServiceResult<EventRegistrationDto>.Failure("Event has reached maximum participant capacity.", 400);

            // Check for existing registration record (handling re-registrations if previously cancelled)
            var existingRegistration = await _registrationRepo.GetByUserAndEventAsync(currentUserId, dto.EventId);
            if (existingRegistration != null)
            {
                if (existingRegistration.Status == Helpers.RegistrationStatus.Registered)
                    return ServiceResult<EventRegistrationDto>.Failure("You are already registered for this event.", 409);

                // Reactivate previously cancelled registration
                existingRegistration.Status = Helpers.RegistrationStatus.Registered;
                existingRegistration.RegistrationDate = DateTime.UtcNow;
                existingRegistration.CancelledDate = null;
                existingRegistration.UpdatedDate = DateTime.UtcNow;

                _registrationRepo.Update(existingRegistration);
                await _registrationRepo.SaveChangesAsync();

                var reactivated = await _registrationRepo.GetRegistrationWithDetailsAsync(existingRegistration.Id);
                return ServiceResult<EventRegistrationDto>.Success(MapToDto(reactivated!));
            }

            var registration = new EventRegistration
            {
                EventId = dto.EventId,
                UserId = currentUserId,
                RegistrationDate = DateTime.UtcNow,
                Status = Helpers.RegistrationStatus.Registered,
                CreatedDate = DateTime.UtcNow
            };

            await _registrationRepo.AddAsync(registration);
            await _registrationRepo.SaveChangesAsync();

            var created = await _registrationRepo.GetRegistrationWithDetailsAsync(registration.Id);
            return ServiceResult<EventRegistrationDto>.Success(MapToDto(created!), 201);
        }

        public async Task<ServiceResult<bool>> CancelRegistrationAsync(int registrationId, int currentUserId, bool isAdmin)
        {
            var registration = await _registrationRepo.GetRegistrationWithDetailsAsync(registrationId);
            if (registration == null)
                return ServiceResult<bool>.Failure("Registration not found.", 404);

            if (!isAdmin && registration.UserId != currentUserId)
                return ServiceResult<bool>.Failure("You are not authorized to cancel this registration.", 403);

            if (registration.Status == Helpers.RegistrationStatus.Cancelled)
                return ServiceResult<bool>.Failure("Registration is already cancelled.", 400);

            if (registration.Event != null && registration.Event.StartDate <= DateTime.UtcNow)
                return ServiceResult<bool>.Failure("Cannot cancel registration for an event that has already started.", 400);

            registration.Status = Helpers.RegistrationStatus.Cancelled;
            registration.CancelledDate = DateTime.UtcNow;
            registration.UpdatedDate = DateTime.UtcNow;

            _registrationRepo.Update(registration);
            await _registrationRepo.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<EventRegistrationDto>> GetByIdAsync(int registrationId, int currentUserId, bool isAdmin)
        {
            var registration = await _registrationRepo.GetRegistrationWithDetailsAsync(registrationId);
            if (registration == null)
                return ServiceResult<EventRegistrationDto>.Failure("Registration not found.", 404);

            var isOrganizer = registration.Event?.OrganizerId == currentUserId;
            if (!isAdmin && registration.UserId != currentUserId && !isOrganizer)
                return ServiceResult<EventRegistrationDto>.Failure("Access denied.", 403);

            return ServiceResult<EventRegistrationDto>.Success(MapToDto(registration));
        }

        public async Task<ServiceResult<IReadOnlyList<EventRegistrationDto>>> GetMyRegistrationsAsync(int currentUserId)
        {
            var registrations = await _registrationRepo.GetRegistrationsByUserIdAsync(currentUserId);
            var dtos = registrations.Select(MapToDto).ToList();
            return ServiceResult<IReadOnlyList<EventRegistrationDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IReadOnlyList<EventRegistrationDto>>> GetEventRegistrationsAsync(int eventId, int currentUserId, bool isAdmin)
        {
            var targetEvent = await _eventRepo.GetByIdAsync(eventId);
            if (targetEvent == null)
                return ServiceResult<IReadOnlyList<EventRegistrationDto>>.Failure("Event not found.", 404);

            if (!isAdmin && targetEvent.OrganizerId != currentUserId)
                return ServiceResult<IReadOnlyList<EventRegistrationDto>>.Failure("You are not authorized to view registrations for this event.", 403);

            var registrations = await _registrationRepo.GetRegistrationsByEventIdAsync(eventId);
            var dtos = registrations.Select(MapToDto).ToList();
            return ServiceResult<IReadOnlyList<EventRegistrationDto>>.Success(dtos);
        }

        private static EventRegistrationDto MapToDto(EventRegistration reg) => new()
        {
            Id = reg.Id,
            EventId = reg.EventId,
            UserId = reg.UserId,
            RegistrationDate = reg.RegistrationDate,
            Status = reg.Status.ToString(),
            CancelledDate = reg.CancelledDate,
            CreatedDate = reg.CreatedDate,
            UpdatedDate = reg.UpdatedDate,
            ParticipantName = reg.User?.Name,
            ParticipantEmail = reg.User?.Email,
            EventTitle = reg.Event?.Title,
            EventStartDate = reg.Event?.StartDate,
            EventEndDate = reg.Event?.EndDate,
            VenueName = reg.Event?.Venue?.Name,
            CategoryName = reg.Event?.Category?.Name
        };
    }
}
