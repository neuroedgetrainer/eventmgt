using EventMgt.DatabaseContext.DbEntities;

namespace EventMgt.Repositories.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);
        Task<IReadOnlyList<Role>> GetActiveRolesWithUserCountAsync();
    }
}
