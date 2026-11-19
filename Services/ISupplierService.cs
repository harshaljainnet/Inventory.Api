using Inventory.Api.Dtos;

namespace Inventory.Api.Services
{
    public interface ISupplierService
    {
        IReadOnlyList<SupplierDto> GetAll();

        SupplierDto? GetById(int id);

        ServiceResult<SupplierDto> Create(SupplierRequest request);

        ServiceResult<SupplierDto> Update(int id, SupplierRequest request);

        // hasProducts: whether any product is still supplied by this supplier (those block deletion).
        ServiceResult Delete(int id, bool hasProducts);
    }
}
