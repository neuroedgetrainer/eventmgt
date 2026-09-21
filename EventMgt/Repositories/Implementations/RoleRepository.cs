using EventMgt.DatabaseContext;
using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventMgt.Repositories.Implementations
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name.ToLower() == name.Trim().ToLower());
        }

        public async Task<IReadOnlyList<Role>> GetActiveRolesWithUserCountAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.IsActive)
                .Include(r => r.Users)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }
    }
}
