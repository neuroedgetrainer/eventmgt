using EventMgt.DatabaseContext;
using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers.Common;
using EventMgt.Models;
using EventMgt.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventMgt.Repositories.Implementations
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Event?> GetEventWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<PagedResult<Event>> GetEventsPagedAsync(EventQueryParameters queryParams)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .Include(e => e.Registrations)
                .AsQueryable();

            // 1. Status & Active filters
            if (queryParams.OnlyActive.HasValue && queryParams.OnlyActive.Value)
            {
                query = query.Where(e => e.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Status))
            {
                query = query.Where(e => e.Status.ToString().ToLower() == queryParams.Status.Trim().ToLower());
            }

            // 2. Keyword Search
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var term = queryParams.SearchTerm.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(term) ||
                                         (e.Description != null && e.Description.ToLower().Contains(term)));
            }

            // 3. Foreign Key filters
            if (queryParams.CategoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == queryParams.CategoryId.Value);
            }

            if (queryParams.VenueId.HasValue)
            {
                query = query.Where(e => e.VenueId == queryParams.VenueId.Value);
            }

            if (queryParams.OrganizerId.HasValue)
            {
                query = query.Where(e => e.OrganizerId == queryParams.OrganizerId.Value);
            }

            // 4. Date ranges
            if (queryParams.FromDate.HasValue)
            {
                query = query.Where(e => e.StartDate >= queryParams.FromDate.Value);
            }

            if (queryParams.ToDate.HasValue)
            {
                query = query.Where(e => e.EndDate <= queryParams.ToDate.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.StartDate)
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PagedResult<Event>(items, totalCount, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<IReadOnlyList<Event>> GetEventsByOrganizerAsync(int organizerId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Registrations)
                .Where(e => e.OrganizerId == organizerId)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetActiveRegistrationsCountAsync(int eventId)
        {
            return await _context.EventRegistrations
                .AsNoTracking()
                .CountAsync(r => r.EventId == eventId && r.Status.ToString() == "Registered");
        }

        public async Task<bool> HasScheduleConflictAsync(int venueId, DateTime startDate, DateTime endDate, int? excludeEventId = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(e => e.VenueId == venueId &&
                            e.IsActive &&
                            !e.IsCancelled &&
                            e.StartDate < endDate &&
                            e.EndDate > startDate);

            if (excludeEventId.HasValue)
            {
                query = query.Where(e => e.Id != excludeEventId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
