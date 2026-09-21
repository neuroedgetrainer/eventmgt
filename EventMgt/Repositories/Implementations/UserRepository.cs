using EventMgt.DatabaseContext;
using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventMgt.Repositories.Implementations
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByEmailWithRoleAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());
        }

        public async Task<User?> GetByIdWithRoleAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IReadOnlyList<User>> GetAllWithRolesAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.Role)
                .Where(u => u.Role.Name.ToLower() == roleName.Trim().ToLower())
                .ToListAsync();
        }
    }
}
