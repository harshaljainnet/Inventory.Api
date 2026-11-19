namespace Inventory.Api.Dtos
{
    public class StockAdjustmentRecordDto
    {
        public int ProductId { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public int Amount { get; set; }

        public int NewQuantity { get; set; }
    }
}
