using EventMgt.Helpers.Common;
using EventMgt.Models;

namespace EventMgt.Services.Interfaces
{
    public interface IRoleService
    {
        Task<ServiceResult<IReadOnlyList<RoleDto>>> GetAllActiveAsync();
        Task<ServiceResult<IReadOnlyList<RoleDto>>> GetAllForAdminAsync();
        Task<ServiceResult<RoleDto>> GetByIdAsync(int id);
        Task<ServiceResult<RoleDto>> CreateAsync(RoleDto dto);
        Task<ServiceResult<RoleDto>> UpdateAsync(int id, RoleDto dto);
        Task<ServiceResult<bool>> SoftDeleteAsync(int id);
    }
}
