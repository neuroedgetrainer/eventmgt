using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers.Common;
using EventMgt.Models;
using EventMgt.Repositories.Interfaces;
using EventMgt.Services.Interfaces;

namespace EventMgt.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepo;

        public RoleService(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<ServiceResult<IReadOnlyList<RoleDto>>> GetAllActiveAsync()
        {
            var roles = await _roleRepo.GetActiveRolesWithUserCountAsync();
            var dtos = roles.Select(MapToDto).ToList();
            return ServiceResult<IReadOnlyList<RoleDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IReadOnlyList<RoleDto>>> GetAllForAdminAsync()
        {
            var roles = await _roleRepo.GetAllAsync();
            var dtos = roles.Select(MapToDto).ToList();
            return ServiceResult<IReadOnlyList<RoleDto>>.Success(dtos);
        }

        public async Task<ServiceResult<RoleDto>> GetByIdAsync(int id)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role == null)
                return ServiceResult<RoleDto>.Failure("Role not found.", 404);

            return ServiceResult<RoleDto>.Success(MapToDto(role));
        }

        public async Task<ServiceResult<RoleDto>> CreateAsync(RoleDto dto)
        {
            var existing = await _roleRepo.GetByNameAsync(dto.Name);
            if (existing != null)
                return ServiceResult<RoleDto>.Failure($"A role named '{dto.Name}' already exists.", 409);

            var entity = new Role
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _roleRepo.AddAsync(entity);
            await _roleRepo.SaveChangesAsync();

            return ServiceResult<RoleDto>.Success(MapToDto(entity), 201);
        }

        public async Task<ServiceResult<RoleDto>> UpdateAsync(int id, RoleDto dto)
        {
            var entity = await _roleRepo.GetByIdAsync(id);
            if (entity == null)
                return ServiceResult<RoleDto>.Failure("Role not found.", 404);

            var existingWithSameName = await _roleRepo.GetByNameAsync(dto.Name);
            if (existingWithSameName != null && existingWithSameName.Id != id)
                return ServiceResult<RoleDto>.Failure($"Role name '{dto.Name}' is already taken.", 409);

            // Protect system-critical roles from accidental renaming
            var systemRoles = new[] { "Admin", "Organizer", "Participant" };
            if (systemRoles.Contains(entity.Name, StringComparer.OrdinalIgnoreCase) &&
                !string.Equals(entity.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<RoleDto>.Failure($"System-defined role '{entity.Name}' cannot be renamed.", 400);
            }

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description?.Trim();
            entity.IsActive = dto.IsActive;

            _roleRepo.Update(entity);
            await _roleRepo.SaveChangesAsync();

            return ServiceResult<RoleDto>.Success(MapToDto(entity));
        }

        public async Task<ServiceResult<bool>> SoftDeleteAsync(int id)
        {
            var entity = await _roleRepo.GetByIdAsync(id);
            if (entity == null)
                return ServiceResult<bool>.Failure("Role not found.", 404);

            var systemRoles = new[] { "Admin", "Organizer", "Participant" };
            if (systemRoles.Contains(entity.Name, StringComparer.OrdinalIgnoreCase))
                return ServiceResult<bool>.Failure($"System role '{entity.Name}' cannot be deactivated or deleted.", 400);

            entity.IsActive = false;

            _roleRepo.Update(entity);
            await _roleRepo.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        private static RoleDto MapToDto(Role role) => new()
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            CreatedDate = role.CreatedDate,
            UsersCount = role.Users?.Count ?? 0
        };
    }
}
