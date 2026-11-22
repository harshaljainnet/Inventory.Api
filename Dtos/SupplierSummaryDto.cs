namespace Inventory.Api.Dtos
{
    public class SupplierSummaryDto
    {
        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public int ProductCount { get; set; }

        public int TotalStockQuantity { get; set; }

        // Sum of price × stock quantity across the supplier's products
        public decimal TotalStockValue { get; set; }

        public List<SupplierSummaryProductDto> Products { get; set; } = [];
    }

    public class SupplierSummaryProductDto
    {
        public string Name { get; set; } = string.Empty;

        public int StockQuantity { get; set; }
    }
}
