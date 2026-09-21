using EventMgt.DatabaseContext.DbEntities;

namespace EventMgt.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailWithRoleAsync(string email);
        Task<User?> GetByIdWithRoleAsync(int id);
        Task<IReadOnlyList<User>> GetAllWithRolesAsync();
        Task<IReadOnlyList<User>> GetUsersByRoleAsync(string roleName);
    }
}
