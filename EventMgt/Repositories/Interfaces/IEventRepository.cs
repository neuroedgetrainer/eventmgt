using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers.Common;
using EventMgt.Models;

namespace EventMgt.Repositories.Interfaces
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        Task<Event?> GetEventWithDetailsAsync(int id);
        Task<PagedResult<Event>> GetEventsPagedAsync(EventQueryParameters queryParams);
        Task<IReadOnlyList<Event>> GetEventsByOrganizerAsync(int organizerId);
        Task<int> GetActiveRegistrationsCountAsync(int eventId);
        Task<bool> HasScheduleConflictAsync(int venueId, DateTime startDate, DateTime endDate, int? excludeEventId = null);
    }
}
