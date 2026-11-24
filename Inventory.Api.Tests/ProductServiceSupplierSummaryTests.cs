using Inventory.Api.Dtos;
using Inventory.Api.Services;

namespace Inventory.Api.Tests
{
    public class ProductServiceSupplierSummaryTests
    {
        // Supplier ids 1-7 own seeded products; 99 is known to the supplier service but owns none.
        private const int SupplierWithoutProducts = 99;

        private static ProductService CreateService() =>
            new(new StubCategoryService(), new StubSupplierService(1, 2, 3, 4, 5, 6, 7, SupplierWithoutProducts));

        [Fact]
        public void UnknownSupplier_ReturnsNotFound()
        {
            var result = CreateService().GetSupplierSummary(1234);

            Assert.Equal(ServiceStatus.NotFound, result.Status);
            Assert.Null(result.Value);
        }

        [Fact]
        public void SupplierWithoutProducts_ReturnsEmptySummary()
        {
            var result = CreateService().GetSupplierSummary(SupplierWithoutProducts);

            Assert.True(result.IsSuccess);
            var summary = result.Value!;
            Assert.Equal(SupplierWithoutProducts, summary.SupplierId);
            Assert.Equal("Supplier 99", summary.SupplierName);
            Assert.Equal(0, summary.ProductCount);
            Assert.Equal(0, summary.TotalStockQuantity);
            Assert.Equal(0m, summary.TotalStockValue);
            Assert.Empty(summary.Products);
        }

        [Fact]
        public void SeededSupplier_SummarisesCountQuantityAndValue()
        {
            // Supplier 1: Wireless Mouse 150 @ 24.99, Vertical Mouse 8 @ 44.95, Gaming Mouse 42 @ 59.99
            var summary = CreateService().GetSupplierSummary(1).Value!;

            Assert.Equal(1, summary.SupplierId);
            Assert.Equal("Supplier 1", summary.SupplierName);
            Assert.Equal(3, summary.ProductCount);
            Assert.Equal(200, summary.TotalStockQuantity);
            Assert.Equal(150 * 24.99m + 8 * 44.95m + 42 * 59.99m, summary.TotalStockValue);
        }

        [Fact]
        public void Summary_ListsEachProductWithItsStock()
        {
            var summary = CreateService().GetSupplierSummary(1).Value!;

            Assert.Collection(summary.Products,
                p => AssertProduct(p, "Wireless Mouse", 150),
                p => AssertProduct(p, "Ergonomic Vertical Mouse", 8),
                p => AssertProduct(p, "Gaming Mouse 16000 DPI", 42));
        }

        [Fact]
        public void Summary_IncludesProductsAcrossCategories_AndZeroStockProducts()
        {
            // Supplier 5 supplies products in categories 5 and 8, including a 0-stock webcam.
            var summary = CreateService().GetSupplierSummary(5).Value!;

            Assert.Equal(5, summary.ProductCount);
            Assert.Equal(55 + 3 + 31 + 38 + 0, summary.TotalStockQuantity);
            Assert.Equal(
                55 * 89.00m + 3 * 119.00m + 31 * 74.50m + 38 * 54.99m + 0 * 149.00m,
                summary.TotalStockValue);
            Assert.Contains(summary.Products, p => p.Name == "4K Streaming Webcam" && p.StockQuantity == 0);
        }

        [Fact]
        public void Summary_OnlyIncludesTheRequestedSuppliersProducts()
        {
            var summary = CreateService().GetSupplierSummary(7).Value!;

            Assert.Equal(3, summary.ProductCount);
            Assert.Equal(22 + 6 + 400, summary.TotalStockQuantity);
            Assert.Equal(22 * 129.00m + 6 * 38.95m + 400 * 8.49m, summary.TotalStockValue);
        }

        [Fact]
        public void Summary_ReflectsStockAdjustments()
        {
            var service = CreateService();
            service.AdjustStock(1, -50); // Wireless Mouse 150 -> 100
            service.AdjustStock(4, 12);  // Vertical Mouse 8 -> 20

            var summary = service.GetSupplierSummary(1).Value!;

            Assert.Equal(100 + 20 + 42, summary.TotalStockQuantity);
            Assert.Equal(100 * 24.99m + 20 * 44.95m + 42 * 59.99m, summary.TotalStockValue);
            Assert.Contains(summary.Products, p => p.Name == "Wireless Mouse" && p.StockQuantity == 100);
        }

        [Fact]
        public void Summary_ReflectsNewProduct()
        {
            var service = CreateService();
            var created = service.Create(new CreateProductRequest
            {
                Name = "  Trackball  ",
                Price = 30.00m,
                StockQuantity = 10,
                CategoryId = 1,
                SupplierId = SupplierWithoutProducts
            });
            Assert.True(created.IsSuccess);

            var summary = service.GetSupplierSummary(SupplierWithoutProducts).Value!;

            Assert.Equal(1, summary.ProductCount);
            Assert.Equal(10, summary.TotalStockQuantity);
            Assert.Equal(300.00m, summary.TotalStockValue);
            AssertProduct(Assert.Single(summary.Products), "Trackball", 10);
        }

        [Fact]
        public void Summary_ReflectsDeletedProduct()
        {
            var service = CreateService();
            Assert.True(service.Delete(1).IsSuccess);

            var summary = service.GetSupplierSummary(1).Value!;

            Assert.Equal(2, summary.ProductCount);
            Assert.Equal(8 + 42, summary.TotalStockQuantity);
            Assert.Equal(8 * 44.95m + 42 * 59.99m, summary.TotalStockValue);
            Assert.DoesNotContain(summary.Products, p => p.Name == "Wireless Mouse");
        }

        [Fact]
        public void Summary_ReflectsProductMovedToAnotherSupplier()
        {
            var service = CreateService();
            var update = service.Update(1, new UpdateProductRequest
            {
                Name = "Wireless Mouse",
                Price = 24.99m,
                CategoryId = 1,
                SupplierId = 2
            });
            Assert.True(update.IsSuccess);

            Assert.Equal(2, service.GetSupplierSummary(1).Value!.ProductCount);
            Assert.Equal(4, service.GetSupplierSummary(2).Value!.ProductCount);
        }

        private static void AssertProduct(SupplierSummaryProductDto product, string name, int stock)
        {
            Assert.Equal(name, product.Name);
            Assert.Equal(stock, product.StockQuantity);
        }

        // Knows only the supplier ids it is given; ProductService only calls GetById.
        private sealed class StubSupplierService(params int[] ids) : ISupplierService
        {
            public SupplierDto? GetById(int id) =>
                ids.Contains(id) ? new SupplierDto { Id = id, Name = $"Supplier {id}" } : null;

            public IReadOnlyList<SupplierDto> GetAll() => throw new NotSupportedException();
            public ServiceResult<SupplierDto> Create(SupplierRequest request) => throw new NotSupportedException();
            public ServiceResult<SupplierDto> Update(int id, SupplierRequest request) => throw new NotSupportedException();
            public ServiceResult Delete(int id, bool hasProducts) => throw new NotSupportedException();
        }

        // Every category id is considered valid.
        private sealed class StubCategoryService : ICategoryService
        {
            public CategoryDto? GetById(int id) => new() { Id = id, Name = $"Category {id}" };

            public IReadOnlyList<CategoryDto> GetAll() => throw new NotSupportedException();
            public ServiceResult<CategoryDto> Create(CategoryRequest request) => throw new NotSupportedException();
            public ServiceResult<CategoryDto> Update(int id, CategoryRequest request) => throw new NotSupportedException();
            public ServiceResult Delete(int id, bool hasProducts) => throw new NotSupportedException();
        }
    }
}
