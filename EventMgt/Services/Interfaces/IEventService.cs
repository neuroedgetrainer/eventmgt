using EventMgt.Helpers.Common;
using EventMgt.Models;

namespace EventMgt.Services.Interfaces
{
    public interface IEventService
    {
        Task<ServiceResult<PagedResult<EventDto>>> GetEventsAsync(EventQueryParameters queryParams);
        Task<ServiceResult<EventDto>> GetByIdAsync(int id);
        Task<ServiceResult<IReadOnlyList<EventDto>>> GetEventsByOrganizerAsync(int organizerId);
        Task<ServiceResult<EventDto>> CreateAsync(int currentUserId, EventDto dto);
        Task<ServiceResult<EventDto>> UpdateAsync(int eventId, int currentUserId, bool isAdmin, EventDto dto);
        Task<ServiceResult<bool>> CancelEventAsync(int eventId, int currentUserId, bool isAdmin, string cancellationReason);
        Task<ServiceResult<bool>> SoftDeleteAsync(int eventId, int currentUserId, bool isAdmin);
    }
}
