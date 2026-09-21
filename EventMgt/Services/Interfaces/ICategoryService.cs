using EventMgt.Helpers.Common;
using EventMgt.Models;

namespace EventMgt.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllActiveAsync();
        Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllForAdminAsync();
        Task<ServiceResult<CategoryDto>> GetByIdAsync(int id);
        Task<ServiceResult<CategoryDto>> CreateAsync(CategoryDto dto);
        Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryDto dto);
        Task<ServiceResult<bool>> SoftDeleteAsync(int id);
    }
}
