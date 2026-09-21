using EventMgt.DatabaseContext.DbEntities;

namespace EventMgt.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<IReadOnlyList<Category>> GetActiveCategoriesWithEventCountAsync();
        Task<bool> HasAssociatedEventsAsync(int categoryId);
    }
}
