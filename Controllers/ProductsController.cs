using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private static readonly Product[] SampleProducts =
        [
            new Product { Id = 1, Name = "Wireless Mouse", Price = 24.99m, StockQuantity = 150 },
            new Product { Id = 2, Name = "Mechanical Keyboard", Price = 79.50m, StockQuantity = 60 },
            new Product { Id = 3, Name = "USB-C Hub", Price = 39.00m, StockQuantity = 85 }
        ];

        [HttpGet(Name = "GetProducts")]
        public IEnumerable<Product> Get()
        {
            return SampleProducts;
        }
    }
}
