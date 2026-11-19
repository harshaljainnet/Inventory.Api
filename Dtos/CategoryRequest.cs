using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Dtos
{
    public class CategoryRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
