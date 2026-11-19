namespace Inventory.Api.Dtos
{
    public class StockAdjustmentRequest
    {
        // Signed amount to add to the current stock (negative to remove).
        public int Amount { get; set; }
    }
}
