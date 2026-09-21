using EventMgt.Helpers.Common;
using EventMgt.Models;

namespace EventMgt.Services.Interfaces
{
    public interface IVenueService
    {
        Task<ServiceResult<IReadOnlyList<VenueDto>>> GetAllActiveAsync();
        Task<ServiceResult<IReadOnlyList<VenueDto>>> GetAllForAdminAsync();
        Task<ServiceResult<VenueDto>> GetByIdAsync(int id);
        Task<ServiceResult<VenueDto>> CreateAsync(VenueDto dto);
        Task<ServiceResult<VenueDto>> UpdateAsync(int id, VenueDto dto);
        Task<ServiceResult<bool>> SoftDeleteAsync(int id);
    }
}
