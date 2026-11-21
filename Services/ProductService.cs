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
            new Product { Id = 3, Name = "USB-C Hub", Price = 39.00m, StockQuantity = 85, CategoryId = 3, SupplierId = 3 }
        ];
        private int _nextId = 4;

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
