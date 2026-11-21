using Inventory.Api.Dtos;

namespace Inventory.Api.Services
{
    // Registered as a singleton: holds the in-memory product data and stock history.
    // All state is guarded by a single lock (_sync); ToDto must be called while holding it.
    public class ProductService : IProductService
    {
        private const int LowStockThreshold = 10;

        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly object _sync = new();

        private readonly List<Product> _products =
        [
            new Product { Id = 1, Name = "Wireless Mouse", Price = 24.99m, StockQuantity = 150, CategoryId = 1, SupplierId = 1 },
            new Product { Id = 2, Name = "Mechanical Keyboard", Price = 79.50m, StockQuantity = 60, CategoryId = 2, SupplierId = 2 },
            new Product { Id = 3, Name = "USB-C Hub", Price = 39.00m, StockQuantity = 85, CategoryId = 3, SupplierId = 3 },
            new Product { Id = 4, Name = "Ergonomic Vertical Mouse", Price = 44.95m, StockQuantity = 8, CategoryId = 1, SupplierId = 1 },
            new Product { Id = 5, Name = "Gaming Mouse 16000 DPI", Price = 59.99m, StockQuantity = 42, CategoryId = 1, SupplierId = 1 },
            new Product { Id = 6, Name = "Compact Wireless Keyboard", Price = 49.00m, StockQuantity = 73, CategoryId = 2, SupplierId = 2 },
            new Product { Id = 7, Name = "Low-Profile Bluetooth Keyboard", Price = 64.90m, StockQuantity = 5, CategoryId = 2, SupplierId = 2 },
            new Product { Id = 8, Name = "Laptop Stand (Aluminium)", Price = 34.50m, StockQuantity = 120, CategoryId = 3, SupplierId = 3 },
            new Product { Id = 9, Name = "Cable Organizer Set", Price = 12.99m, StockQuantity = 200, CategoryId = 3, SupplierId = 3 },
            new Product { Id = 10, Name = "24\" Full HD IPS Monitor", Price = 139.00m, StockQuantity = 27, CategoryId = 4, SupplierId = 4 },
            new Product { Id = 11, Name = "27\" 4K UHD Monitor", Price = 329.00m, StockQuantity = 9, CategoryId = 4, SupplierId = 4 },
            new Product { Id = 12, Name = "34\" Ultrawide Curved Monitor", Price = 449.99m, StockQuantity = 14, CategoryId = 4, SupplierId = 4 },
            new Product { Id = 13, Name = "Noise-Cancelling Headset", Price = 89.00m, StockQuantity = 55, CategoryId = 5, SupplierId = 5 },
            new Product { Id = 14, Name = "USB Conference Speakerphone", Price = 119.00m, StockQuantity = 3, CategoryId = 5, SupplierId = 5 },
            new Product { Id = 15, Name = "Desktop Studio Microphone", Price = 74.50m, StockQuantity = 31, CategoryId = 5, SupplierId = 5 },
            new Product { Id = 16, Name = "1TB Portable SSD", Price = 99.99m, StockQuantity = 64, CategoryId = 6, SupplierId = 6 },
            new Product { Id = 17, Name = "2TB External Hard Drive", Price = 69.00m, StockQuantity = 48, CategoryId = 6, SupplierId = 6 },
            new Product { Id = 18, Name = "128GB USB 3.2 Flash Drive", Price = 17.49m, StockQuantity = 310, CategoryId = 6, SupplierId = 6 },
            new Product { Id = 19, Name = "Wi-Fi 6 Router AX3000", Price = 129.00m, StockQuantity = 22, CategoryId = 7, SupplierId = 7 },
            new Product { Id = 20, Name = "8-Port Gigabit Switch", Price = 38.95m, StockQuantity = 6, CategoryId = 7, SupplierId = 7 },
            new Product { Id = 21, Name = "Cat6 Ethernet Cable 10m", Price = 8.49m, StockQuantity = 400, CategoryId = 7, SupplierId = 7 },
            new Product { Id = 22, Name = "1080p HD Webcam", Price = 54.99m, StockQuantity = 38, CategoryId = 8, SupplierId = 5 },
            new Product { Id = 23, Name = "4K Streaming Webcam", Price = 149.00m, StockQuantity = 0, CategoryId = 8, SupplierId = 5 }
        ];
        private int _nextId = 24;

        // In-memory log of all stock adjustments, in the order they were applied
        private readonly List<StockAdjustmentRecord> _stockHistory = [];

        public ProductService(ICategoryService categoryService, ISupplierService supplierService)
        {
            _categoryService = categoryService;
            _supplierService = supplierService;
        }

        public IReadOnlyList<ProductDto> GetAll()
        {
            lock (_sync)
            {
                return _products.Select(ToDto).ToList();
            }
        }

        public ProductDto? GetById(int id)
        {
            lock (_sync)
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                return product is null ? null : ToDto(product);
            }
        }

        public IReadOnlyList<ProductDto> GetLowStock()
        {
            lock (_sync)
            {
                return _products.Where(p => p.StockQuantity < LowStockThreshold).Select(ToDto).ToList();
            }
        }

        public IReadOnlyList<ProductDto> GetByCategory(int categoryId)
        {
            lock (_sync)
            {
                return _products.Where(p => p.CategoryId == categoryId).Select(ToDto).ToList();
            }
        }

        public IReadOnlyList<ProductDto> GetBySupplier(int supplierId)
        {
            lock (_sync)
            {
                return _products.Where(p => p.SupplierId == supplierId).Select(ToDto).ToList();
            }
        }

        public ServiceResult<ProductDto> Create(CreateProductRequest request)
        {
            var error = ValidateReferences(request);
            if (error is not null)
            {
                return ServiceResult<ProductDto>.Invalid(error);
            }

            lock (_sync)
            {
                var product = new Product
                {
                    Id = _nextId++,
                    Name = request.Name.Trim(),
                    Price = request.Price,
                    StockQuantity = request.StockQuantity,
                    CategoryId = request.CategoryId,
                    SupplierId = request.SupplierId
                };
                _products.Add(product);
                return ServiceResult<ProductDto>.Ok(ToDto(product));
            }
        }

        public ServiceResult<ProductDto> Update(int id, UpdateProductRequest request)
        {
            var error = ValidateReferences(request);

            lock (_sync)
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product is null)
                {
                    return ServiceResult<ProductDto>.NotFound();
                }

                if (error is not null)
                {
                    return ServiceResult<ProductDto>.Invalid(error);
                }

                product.Name = request.Name.Trim();
                product.Price = request.Price;
                product.CategoryId = request.CategoryId;
                product.SupplierId = request.SupplierId;
                return ServiceResult<ProductDto>.Ok(ToDto(product));
            }
        }

        public ServiceResult Delete(int id)
        {
            lock (_sync)
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product is null)
                {
                    return ServiceResult.NotFound();
                }

                _products.Remove(product);
                _stockHistory.RemoveAll(r => r.ProductId == id);
                return ServiceResult.Ok();
            }
        }

        public ServiceResult<ProductDto> AdjustStock(int id, int amount)
        {
            lock (_sync)
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product is null)
                {
                    return ServiceResult<ProductDto>.NotFound();
                }

                long newQuantity = (long)product.StockQuantity + amount;
                if (newQuantity < 0 || newQuantity > int.MaxValue)
                {
                    return ServiceResult<ProductDto>.Invalid(
                        $"Adjusting stock of {product.StockQuantity} by {amount} would result in an invalid quantity.",
                        "Invalid stock adjustment");
                }

                product.StockQuantity = (int)newQuantity;
                _stockHistory.Add(new StockAdjustmentRecord
                {
                    ProductId = product.Id,
                    Timestamp = DateTimeOffset.UtcNow,
                    Amount = amount,
                    NewQuantity = product.StockQuantity
                });

                return ServiceResult<ProductDto>.Ok(ToDto(product));
            }
        }

        public IReadOnlyList<StockAdjustmentRecordDto>? GetStockHistory(int id)
        {
            lock (_sync)
            {
                if (!_products.Any(p => p.Id == id))
                {
                    return null;
                }

                return _stockHistory
                    .Where(r => r.ProductId == id)
                    .Select(r => new StockAdjustmentRecordDto
                    {
                        ProductId = r.ProductId,
                        Timestamp = r.Timestamp,
                        Amount = r.Amount,
                        NewQuantity = r.NewQuantity
                    })
                    .ToList();
            }
        }

        // Returns an error message if the category or supplier does not exist, otherwise null.
        private string? ValidateReferences(UpdateProductRequest request)
        {
            if (_categoryService.GetById(request.CategoryId) is null)
            {
                return $"Category {request.CategoryId} does not exist.";
            }

            if (_supplierService.GetById(request.SupplierId) is null)
            {
                return $"Supplier {request.SupplierId} does not exist.";
            }

            return null;
        }

        private ProductDto ToDto(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = _categoryService.GetById(product.CategoryId)?.Name,
            SupplierId = product.SupplierId,
            SupplierName = _supplierService.GetById(product.SupplierId)?.Name
        };
    }
}
