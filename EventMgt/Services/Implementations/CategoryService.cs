using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers.Common;
using EventMgt.Models;
using EventMgt.Repositories.Interfaces;
using EventMgt.Services.Interfaces;

namespace EventMgt.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllActiveAsync()
        {
            var categories = await _categoryRepo.GetActiveCategoriesWithEventCountAsync();
            var dtos = categories.Select(MapToDto).ToList();

            return ServiceResult<IReadOnlyList<CategoryDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllForAdminAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            var dtos = categories.Select(MapToDto).ToList();

            return ServiceResult<IReadOnlyList<CategoryDto>>.Success(dtos);
        }

        public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
                return ServiceResult<CategoryDto>.Failure("Category not found.", 404);

            return ServiceResult<CategoryDto>.Success(MapToDto(category));
        }

        public async Task<ServiceResult<CategoryDto>> CreateAsync(CategoryDto dto)
        {
            var existing = await _categoryRepo.GetByNameAsync(dto.Name);
            if (existing != null)
                return ServiceResult<CategoryDto>.Failure($"A category named '{dto.Name}' already exists.", 409);

            var entity = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _categoryRepo.AddAsync(entity);
            await _categoryRepo.SaveChangesAsync();

            return ServiceResult<CategoryDto>.Success(MapToDto(entity), 201);
        }

        public async Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryDto dto)
        {
            var entity = await _categoryRepo.GetByIdAsync(id);
            if (entity == null)
                return ServiceResult<CategoryDto>.Failure("Category not found.", 404);

            var existingWithSameName = await _categoryRepo.GetByNameAsync(dto.Name);
            if (existingWithSameName != null && existingWithSameName.Id != id)
                return ServiceResult<CategoryDto>.Failure($"Category name '{dto.Name}' is already taken.", 409);

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description?.Trim();
            entity.IsActive = dto.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;

            _categoryRepo.Update(entity);
            await _categoryRepo.SaveChangesAsync();

            return ServiceResult<CategoryDto>.Success(MapToDto(entity));
        }

        public async Task<ServiceResult<bool>> SoftDeleteAsync(int id)
        {
            var entity = await _categoryRepo.GetByIdAsync(id);
            if (entity == null)
                return ServiceResult<bool>.Failure("Category not found.", 404);

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;

            _categoryRepo.Update(entity);
            await _categoryRepo.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
        private static CategoryDto MapToDto(Category entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate,
            ActiveEventsCount = entity.Events?.Count ?? 0
        };
    }
}
