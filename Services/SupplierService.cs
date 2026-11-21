using Inventory.Api.Dtos;

namespace Inventory.Api.Services
{
    // Registered as a singleton: holds the in-memory supplier data.
    public class SupplierService : ISupplierService
    {
        private readonly object _sync = new();
        private readonly List<Supplier> _suppliers =
        [
            new Supplier { Id = 1, Name = "Logi Distribution", ContactEmail = "sales@logi-dist.example" },
            new Supplier { Id = 2, Name = "KeyCraft Wholesale", ContactEmail = "orders@keycraft.example" },
            new Supplier { Id = 3, Name = "Hubworks Supply", ContactEmail = null },
            new Supplier { Id = 4, Name = "Pixel Display Co.", ContactEmail = "wholesale@pixeldisplay.example" },
            new Supplier { Id = 5, Name = "SoundWave Trading", ContactEmail = "orders@soundwave-trading.example" },
            new Supplier { Id = 6, Name = "DataVault Distribution", ContactEmail = "sales@datavault-dist.example" },
            new Supplier { Id = 7, Name = "NetLink Supply", ContactEmail = null }
        ];
        private int _nextId = 8;

        public IReadOnlyList<SupplierDto> GetAll()
        {
            lock (_sync)
            {
                return _suppliers.Select(ToDto).ToList();
            }
        }

        public SupplierDto? GetById(int id)
        {
            lock (_sync)
            {
                var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
                return supplier is null ? null : ToDto(supplier);
            }
        }

        public ServiceResult<SupplierDto> Create(SupplierRequest request)
        {
            var name = request.Name.Trim();
            lock (_sync)
            {
                if (NameTaken(name, exceptId: null))
                {
                    return ServiceResult<SupplierDto>.Conflict($"A supplier named '{name}' already exists.");
                }

                var supplier = new Supplier { Id = _nextId++, Name = name, ContactEmail = request.ContactEmail?.Trim() };
                _suppliers.Add(supplier);
                return ServiceResult<SupplierDto>.Ok(ToDto(supplier));
            }
        }

        public ServiceResult<SupplierDto> Update(int id, SupplierRequest request)
        {
            var name = request.Name.Trim();
            lock (_sync)
            {
                var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
                if (supplier is null)
                {
                    return ServiceResult<SupplierDto>.NotFound();
                }

                if (NameTaken(name, exceptId: id))
                {
                    return ServiceResult<SupplierDto>.Conflict($"A supplier named '{name}' already exists.");
                }

                supplier.Name = name;
                supplier.ContactEmail = request.ContactEmail?.Trim();
                return ServiceResult<SupplierDto>.Ok(ToDto(supplier));
            }
        }

        public ServiceResult Delete(int id, bool hasProducts)
        {
            lock (_sync)
            {
                var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
                if (supplier is null)
                {
                    return ServiceResult.NotFound();
                }

                if (hasProducts)
                {
                    return ServiceResult.Conflict("The supplier still has products. Move or delete them first.");
                }

                _suppliers.Remove(supplier);
                return ServiceResult.Ok();
            }
        }

        private bool NameTaken(string name, int? exceptId) =>
            _suppliers.Any(s => s.Id != exceptId && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

        private static SupplierDto ToDto(Supplier supplier) =>
            new() { Id = supplier.Id, Name = supplier.Name, ContactEmail = supplier.ContactEmail };
    }
}
