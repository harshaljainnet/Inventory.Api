using Inventory.Api.Dtos;

namespace Inventory.Api.Services
{
    // Registered as a singleton: holds the in-memory category data.
    public class CategoryService : ICategoryService
    {
        private readonly object _sync = new();
        private readonly List<Category> _categories =
        [
            new Category { Id = 1, Name = "Mice" },
            new Category { Id = 2, Name = "Keyboards" },
            new Category { Id = 3, Name = "Accessories" },
            new Category { Id = 4, Name = "Monitors" },
            new Category { Id = 5, Name = "Headsets & Audio" },
            new Category { Id = 6, Name = "Storage" },
            new Category { Id = 7, Name = "Networking" },
            new Category { Id = 8, Name = "Webcams" }
        ];
        private int _nextId = 9;

        public IReadOnlyList<CategoryDto> GetAll()
        {
            lock (_sync)
            {
                return _categories.Select(ToDto).ToList();
            }
        }

        public CategoryDto? GetById(int id)
        {
            lock (_sync)
            {
                var category = _categories.FirstOrDefault(c => c.Id == id);
                return category is null ? null : ToDto(category);
            }
        }

        public ServiceResult<CategoryDto> Create(CategoryRequest request)
        {
            var name = request.Name.Trim();
            lock (_sync)
            {
                if (NameTaken(name, exceptId: null))
                {
                    return ServiceResult<CategoryDto>.Conflict($"A category named '{name}' already exists.");
                }

                var category = new Category { Id = _nextId++, Name = name };
                _categories.Add(category);
                return ServiceResult<CategoryDto>.Ok(ToDto(category));
            }
        }

        public ServiceResult<CategoryDto> Update(int id, CategoryRequest request)
        {
            var name = request.Name.Trim();
            lock (_sync)
            {
                var category = _categories.FirstOrDefault(c => c.Id == id);
                if (category is null)
                {
                    return ServiceResult<CategoryDto>.NotFound();
                }

                if (NameTaken(name, exceptId: id))
                {
                    return ServiceResult<CategoryDto>.Conflict($"A category named '{name}' already exists.");
                }

                category.Name = name;
                return ServiceResult<CategoryDto>.Ok(ToDto(category));
            }
        }

        public ServiceResult Delete(int id, bool hasProducts)
        {
            lock (_sync)
            {
                var category = _categories.FirstOrDefault(c => c.Id == id);
                if (category is null)
                {
                    return ServiceResult.NotFound();
                }

                if (hasProducts)
                {
                    return ServiceResult.Conflict("The category still has products. Move or delete them first.");
                }

                _categories.Remove(category);
                return ServiceResult.Ok();
            }
        }

        private bool NameTaken(string name, int? exceptId) =>
            _categories.Any(c => c.Id != exceptId && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        private static CategoryDto ToDto(Category category) =>
            new() { Id = category.Id, Name = category.Name };
    }
}
