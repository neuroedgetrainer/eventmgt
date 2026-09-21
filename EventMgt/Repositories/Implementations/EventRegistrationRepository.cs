using EventMgt.DatabaseContext;
using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventMgt.Repositories.Implementations
{
    public class EventRegistrationRepository : GenericRepository<EventRegistration>, IEventRegistrationRepository
    {
        public EventRegistrationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<EventRegistration?> GetRegistrationWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Event)
                    .ThenInclude(e => e.Venue)
                .Include(r => r.Event)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<EventRegistration?> GetByUserAndEventAsync(int userId, int eventId)
        {
            return await _dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetRegistrationsByUserIdAsync(int userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(r => r.Event)
                    .ThenInclude(e => e.Venue)
                .Include(r => r.Event)
                    .ThenInclude(e => e.Category)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<EventRegistration>> GetRegistrationsByEventIdAsync(int eventId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.EventId == eventId)
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();
        }

        public async Task<int> GetActiveCountForEventAsync(int eventId)
        {
            return await _dbSet
                .CountAsync(r => r.EventId == eventId && r.Status == Helpers.RegistrationStatus.Registered);
        }
    }
}
