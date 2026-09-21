using EventMgt.DatabaseContext;
using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventMgt.Repositories.Implementations
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.Trim().ToLower());
        }

        public async Task<IReadOnlyList<Category>> GetActiveCategoriesWithEventCountAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Include(c => c.Events.Where(e => e.IsActive && !e.IsCancelled))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> HasAssociatedEventsAsync(int categoryId)
        {
            return await _context.Events.AnyAsync(e => e.CategoryId == categoryId);
        }
    }
}
