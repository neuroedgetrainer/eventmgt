using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers.Common;
using EventMgt.Models;
using EventMgt.Repositories.Interfaces;
using EventMgt.Services.Interfaces;

namespace EventMgt.Services.Implementations
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _venueRepo;

        public VenueService(IVenueRepository venueRepo)
        {
            _venueRepo = venueRepo;
        }

        public async Task<ServiceResult<IReadOnlyList<VenueDto>>> GetAllActiveAsync()
        {
            var venues = await _venueRepo.GetActiveVenuesWithEventCountAsync();
            var dtos = venues.Select(MapToDto).ToList();
            return ServiceResult<IReadOnlyList<VenueDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IReadOnlyList<VenueDto>>> GetAllForAdminAsync()
        {
            var venues = await _venueRepo.GetAllAsync();
            var dtos = venues.Select(MapToDto).ToList();
            return ServiceResult<IReadOnlyList<VenueDto>>.Success(dtos);
        }

        public async Task<ServiceResult<VenueDto>> GetByIdAsync(int id)
        {
            var venue = await _venueRepo.GetByIdAsync(id);
            if (venue == null)
                return ServiceResult<VenueDto>.Failure("Venue not found.", 404);

            return ServiceResult<VenueDto>.Success(MapToDto(venue));
        }

        public async Task<ServiceResult<VenueDto>> CreateAsync(VenueDto dto)
        {
            var existing = await _venueRepo.GetByNameAsync(dto.Name);
            if (existing != null)
                return ServiceResult<VenueDto>.Failure($"A venue named '{dto.Name}' already exists.", 409);

            var entity = new Venue
            {
                Name = dto.Name.Trim(),
                Address = dto.Address?.Trim(),
                City = dto.City?.Trim(),
                State = dto.State?.Trim(),
                Country = dto.Country?.Trim(),
                PostalCode = dto.PostalCode?.Trim(),
                Capacity = dto.Capacity,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _venueRepo.AddAsync(entity);
            await _venueRepo.SaveChangesAsync();

            return ServiceResult<VenueDto>.Success(MapToDto(entity), 201);
        }

        public async Task<ServiceResult<VenueDto>> UpdateAsync(int id, VenueDto dto)
        {
            var entity = await _venueRepo.GetByIdAsync(id);
            if (entity == null)
                return ServiceResult<VenueDto>.Failure("Venue not found.", 404);

            var existingWithSameName = await _venueRepo.GetByNameAsync(dto.Name);
            if (existingWithSameName != null && existingWithSameName.Id != id)
                return ServiceResult<VenueDto>.Failure($"Venue name '{dto.Name}' is already taken.", 409);

            entity.Name = dto.Name.Trim();
            entity.Address = dto.Address?.Trim();
            entity.City = dto.City?.Trim();
            entity.State = dto.State?.Trim();
            entity.Country = dto.Country?.Trim();
            entity.PostalCode = dto.PostalCode?.Trim();
            entity.Capacity = dto.Capacity;
            entity.IsActive = dto.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;

            _venueRepo.Update(entity);
            await _venueRepo.SaveChangesAsync();

            return ServiceResult<VenueDto>.Success(MapToDto(entity));
        }

        public async Task<ServiceResult<bool>> SoftDeleteAsync(int id)
        {
            var entity = await _venueRepo.GetByIdAsync(id);
            if (entity == null)
                return ServiceResult<bool>.Failure("Venue not found.", 404);

            var hasEvents = await _venueRepo.HasAssociatedEventsAsync(id);
            if (hasEvents)
                return ServiceResult<bool>.Failure("Cannot deactivate a venue with scheduled events.", 400);

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;

            _venueRepo.Update(entity);
            await _venueRepo.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        private static VenueDto MapToDto(Venue entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            Capacity = entity.Capacity,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate,
            ActiveEventsCount = entity.Events?.Count ?? 0
        };
    }
}
