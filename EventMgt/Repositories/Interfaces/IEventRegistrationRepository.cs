using EventMgt.DatabaseContext.DbEntities;

namespace EventMgt.Repositories.Interfaces
{
    public interface IEventRegistrationRepository : IGenericRepository<EventRegistration>
    {
        Task<EventRegistration?> GetRegistrationWithDetailsAsync(int id);
        Task<EventRegistration?> GetByUserAndEventAsync(int userId, int eventId);
        Task<IReadOnlyList<EventRegistration>> GetRegistrationsByUserIdAsync(int userId);
        Task<IReadOnlyList<EventRegistration>> GetRegistrationsByEventIdAsync(int eventId);
        Task<int> GetActiveCountForEventAsync(int eventId);
    }
}
