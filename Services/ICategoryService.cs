using Inventory.Api.Dtos;

namespace Inventory.Api.Services
{
    public interface ICategoryService
    {
        IReadOnlyList<CategoryDto> GetAll();

        CategoryDto? GetById(int id);

        ServiceResult<CategoryDto> Create(CategoryRequest request);

        ServiceResult<CategoryDto> Update(int id, CategoryRequest request);

        // hasProducts: whether any product still belongs to the category (those block deletion).
        ServiceResult Delete(int id, bool hasProducts);
    }
}
