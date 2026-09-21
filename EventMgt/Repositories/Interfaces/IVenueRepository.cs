using EventMgt.DatabaseContext.DbEntities;

namespace EventMgt.Repositories.Interfaces
{
    public interface IVenueRepository : IGenericRepository<Venue>
    {
        Task<Venue?> GetByNameAsync(string name);
        Task<IReadOnlyList<Venue>> GetActiveVenuesWithEventCountAsync();
        Task<bool> HasAssociatedEventsAsync(int venueId);
    }
}
