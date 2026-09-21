using EventMgt.DatabaseContext;
using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventMgt.Repositories.Implementations
{
    public class VenueRepository : GenericRepository<Venue>, IVenueRepository
    {
        public VenueRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Venue?> GetByNameAsync(string name)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Name.ToLower() == name.Trim().ToLower());
        }

        public async Task<IReadOnlyList<Venue>> GetActiveVenuesWithEventCountAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(v => v.IsActive)
                .Include(v => v.Events.Where(e => e.IsActive && !e.IsCancelled))
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        public async Task<bool> HasAssociatedEventsAsync(int venueId)
        {
            return await _context.Events.AnyAsync(e => e.VenueId == venueId);
        }
    }
}
