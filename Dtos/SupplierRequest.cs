using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Dtos
{
    public class SupplierRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress, StringLength(200)]
        public string? ContactEmail { get; set; }
    }
}
