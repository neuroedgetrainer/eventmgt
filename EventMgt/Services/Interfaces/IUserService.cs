using EventMgt.Helpers.Common;
using EventMgt.Models;

namespace EventMgt.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<AuthTokenDto>> AuthenticateAsync(UserLoginDto loginDto);
        Task<ServiceResult<AuthTokenDto>> RegisterParticipantAsync(UserDto registerDto);
        Task<ServiceResult<UserDto>> CreateUserByAdminAsync(UserDto createDto);
        Task<ServiceResult<UserDto>> GetByIdAsync(int id);
        Task<ServiceResult<IReadOnlyList<UserDto>>> GetAllAsync();
        Task<ServiceResult<UserDto>> UpdateProfileAsync(int id, UserDto dto);
        Task<ServiceResult<bool>> ChangeUserStatusAsync(int id, bool isActive);
    }
}
