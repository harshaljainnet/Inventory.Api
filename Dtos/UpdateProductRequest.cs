using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Dtos
{
    // Stock is changed through the stock-adjustment endpoint, so it is not part of an update.
    public class UpdateProductRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0.0, 1000000.0)]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public int SupplierId { get; set; }
    }
}
