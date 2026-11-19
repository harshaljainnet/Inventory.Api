using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Dtos
{
    public class CreateProductRequest : UpdateProductRequest
    {
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}
