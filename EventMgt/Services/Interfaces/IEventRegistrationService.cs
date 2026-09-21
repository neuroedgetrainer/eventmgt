using EventMgt.Helpers.Common;
using EventMgt.Models;

namespace EventMgt.Services.Interfaces
{
    public interface IEventRegistrationService
    {
        Task<ServiceResult<EventRegistrationDto>> RegisterAsync(int currentUserId, EventRegistrationDto dto);
        Task<ServiceResult<bool>> CancelRegistrationAsync(int registrationId, int currentUserId, bool isAdmin);
        Task<ServiceResult<EventRegistrationDto>> GetByIdAsync(int registrationId, int currentUserId, bool isAdmin);
        Task<ServiceResult<IReadOnlyList<EventRegistrationDto>>> GetMyRegistrationsAsync(int currentUserId);
        Task<ServiceResult<IReadOnlyList<EventRegistrationDto>>> GetEventRegistrationsAsync(int eventId, int currentUserId, bool isAdmin);
    }
}
