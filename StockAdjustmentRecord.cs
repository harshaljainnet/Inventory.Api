namespace Inventory.Api
{
    public class StockAdjustmentRecord
    {
        public int ProductId { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public int Amount { get; set; }

        public int NewQuantity { get; set; }
    }
}
