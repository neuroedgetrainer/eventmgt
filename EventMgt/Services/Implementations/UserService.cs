using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers.Common;
using EventMgt.Models;
using EventMgt.Repositories.Interfaces;
using EventMgt.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EventMgt.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IJwtTokenService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IJwtTokenService jwtService,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<ServiceResult<AuthTokenDto>> AuthenticateAsync(UserLoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.UserName) || string.IsNullOrWhiteSpace(loginDto.Password))
                return ServiceResult<AuthTokenDto>.Failure("Username and password are required.", 400);

            var user = await _userRepo.GetByEmailWithRoleAsync(loginDto.UserName);
            if (user == null || !user.IsActive)
                return ServiceResult<AuthTokenDto>.Failure("Invalid credentials or account is deactivated.", 401);

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
                return ServiceResult<AuthTokenDto>.Failure("Invalid credentials.", 401);

            var token = _jwtService.GenerateToken(user);

            var response = new AuthTokenDto
            {
                Token = token,
                ExpiresAt = _jwtService.GetExpirationDate(),
                User = MapToDto(user)
            };

            return ServiceResult<AuthTokenDto>.Success(response);
        }

        public async Task<ServiceResult<AuthTokenDto>> RegisterParticipantAsync(UserDto registerDto)
        {
            if (string.IsNullOrWhiteSpace(registerDto.Password))
                return ServiceResult<AuthTokenDto>.Failure("Password is required for registration.", 400);

            var existing = await _userRepo.GetByEmailWithRoleAsync(registerDto.Email);
            if (existing != null)
                return ServiceResult<AuthTokenDto>.Failure("Email address is already in use.", 409);

            var participantRole = await _roleRepo.GetByNameAsync("Participant");
            if (participantRole == null)
                return ServiceResult<AuthTokenDto>.Failure("Participant role is not configured.", 500);

            var user = new User
            {
                Name = registerDto.Name.Trim(),
                Email = registerDto.Email.Trim().ToLower(),
                RoleId = participantRole.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            user.Role = participantRole;
            var token = _jwtService.GenerateToken(user);

            return ServiceResult<AuthTokenDto>.Success(new AuthTokenDto
            {
                Token = token,
                ExpiresAt = _jwtService.GetExpirationDate(),
                User = MapToDto(user)
            }, 201);
        }

        public async Task<ServiceResult<UserDto>> CreateUserByAdminAsync(UserDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Password))
                return ServiceResult<UserDto>.Failure("Initial password is required.", 400);

            var existing = await _userRepo.GetByEmailWithRoleAsync(createDto.Email);
            if (existing != null)
                return ServiceResult<UserDto>.Failure("Email address is already in use.", 409);

            var role = await _roleRepo.GetByIdAsync(createDto.RoleId);
            if (role == null || !role.IsActive)
                return ServiceResult<UserDto>.Failure("Specified role does not exist or is inactive.", 400);

            var user = new User
            {
                Name = createDto.Name.Trim(),
                Email = createDto.Email.Trim().ToLower(),
                RoleId = createDto.RoleId,
                IsActive = createDto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, createDto.Password);

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            user.Role = role;
            return ServiceResult<UserDto>.Success(MapToDto(user), 201);
        }

        public async Task<ServiceResult<UserDto>> GetByIdAsync(int id)
        {
            var user = await _userRepo.GetByIdWithRoleAsync(id);
            if (user == null)
                return ServiceResult<UserDto>.Failure("User not found.", 404);

            return ServiceResult<UserDto>.Success(MapToDto(user));
        }

        public async Task<ServiceResult<IReadOnlyList<UserDto>>> GetAllAsync()
        {
            var users = await _userRepo.GetAllWithRolesAsync();
            var dtos = users.Select(MapToDto).ToList();
            return ServiceResult<IReadOnlyList<UserDto>>.Success(dtos);
        }

        public async Task<ServiceResult<UserDto>> UpdateProfileAsync(int id, UserDto dto)
        {
            var user = await _userRepo.GetByIdWithRoleAsync(id);
            if (user == null)
                return ServiceResult<UserDto>.Failure("User not found.", 404);

            var existingEmail = await _userRepo.GetByEmailWithRoleAsync(dto.Email);
            if (existingEmail != null && existingEmail.Id != id)
                return ServiceResult<UserDto>.Failure("Email address is already in use.", 409);

            user.Name = dto.Name.Trim();
            user.Email = dto.Email.Trim().ToLower();
            user.UpdatedDate = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            }

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return ServiceResult<UserDto>.Success(MapToDto(user));
        }

        public async Task<ServiceResult<bool>> ChangeUserStatusAsync(int id, bool isActive)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found.", 404);

            user.IsActive = isActive;
            user.UpdatedDate = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<IReadOnlyList<RoleDto>>> GetAllRolesAsync()
        {
            var roles = await _roleRepo.GetActiveRolesWithUserCountAsync();
            var dtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsActive = r.IsActive,
                CreatedDate = r.CreatedDate,
                UsersCount = r.Users?.Count ?? 0
            }).ToList();

            return ServiceResult<IReadOnlyList<RoleDto>>.Success(dtos);
        }

        private static UserDto MapToDto(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate,
            UpdatedDate = user.UpdatedDate
        };
    }
}
