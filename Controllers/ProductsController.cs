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

        // Returns all products
        [HttpGet(Name = "GetProducts")]
        public IEnumerable<Product> Get()
        {
            return SampleProducts;
        }

        // Returns a single product by Id, or 404 if not found
        [HttpGet("{id:int}", Name = "GetProductById")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> GetById(int id)
        {
            var product = SampleProducts.FirstOrDefault(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }

            return product;
        }

        // Adjusts a product's stock by a signed amount (positive adds, negative removes).
        // Returns 404 if the product is missing, 400 if the result would be negative.
        [HttpPatch("{id:int}/stock", Name = "AdjustProductStock")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> AdjustStock(int id, [FromBody] StockAdjustment adjustment)
        {
            var product = SampleProducts.FirstOrDefault(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }

            lock (product)
            {
                long newQuantity = (long)product.StockQuantity + adjustment.Amount;
                if (newQuantity < 0 || newQuantity > int.MaxValue)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid stock adjustment",
                        Detail = $"Adjusting stock of {product.StockQuantity} by {adjustment.Amount} would result in an invalid quantity.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                product.StockQuantity = (int)newQuantity;
            }

            return product;
        }
    }

    public class StockAdjustment
    {
        // Signed amount to add to the current stock (negative to remove).
        public int Amount { get; set; }
    }
}
