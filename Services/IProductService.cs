using Inventory.Api.Dtos;

namespace Inventory.Api.Services
{
    public interface IProductService
    {
        IReadOnlyList<ProductDto> GetAll();

        ProductDto? GetById(int id);

        IReadOnlyList<ProductDto> GetByCategory(int categoryId);

        IReadOnlyList<ProductDto> GetBySupplier(int supplierId);

        ServiceResult<ProductDto> Create(CreateProductRequest request);

        ServiceResult<ProductDto> Update(int id, UpdateProductRequest request);

        // Also removes the product's stock history.
        ServiceResult Delete(int id);

        ServiceResult<ProductDto> AdjustStock(int id, int amount);

        // Returns null if the product does not exist.
        IReadOnlyList<StockAdjustmentRecordDto>? GetStockHistory(int id);
    }
}
