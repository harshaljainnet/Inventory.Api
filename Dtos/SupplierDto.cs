namespace Inventory.Api.Dtos
{
    public class SupplierDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ContactEmail { get; set; }
    }
}
