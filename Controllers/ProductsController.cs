using Inventory.Api.Dtos;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // Returns all products
        [HttpGet(Name = "GetProducts")]
        public IEnumerable<ProductDto> Get()
        {
            return _productService.GetAll();
        }

        // Returns products with stock below 10
        [HttpGet("low-stock", Name = "GetLowStockProducts")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public IEnumerable<ProductDto> GetLowStock()
        {
            return _productService.GetLowStock();
        }

        // Returns a single product by Id, or 404 if not found
        [HttpGet("{id:int}", Name = "GetProductById")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProductDto> GetById(int id)
        {
            var product = _productService.GetById(id);
            if (product is null)
            {
                return NotFound();
            }

            return product;
        }

        // Creates a product. Returns 400 if the category or supplier does not exist.
        [HttpPost(Name = "CreateProduct")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ProductDto> Create([FromBody] CreateProductRequest request)
        {
            var result = _productService.Create(request);
            if (!result.IsSuccess)
            {
                return this.Failure(result);
            }

            return CreatedAtRoute("GetProductById", new { id = result.Value!.Id }, result.Value);
        }

        // Updates a product's details and category/supplier (not its stock).
        [HttpPut("{id:int}", Name = "UpdateProduct")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProductDto> Update(int id, [FromBody] UpdateProductRequest request)
        {
            var result = _productService.Update(id, request);
            if (!result.IsSuccess)
            {
                return this.Failure(result);
            }

            return result.Value!;
        }

        // Deletes a product and its stock history
        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var result = _productService.Delete(id);
            return result.IsSuccess ? NoContent() : this.Failure(result);
        }

        // Adjusts a product's stock by a signed amount (positive adds, negative removes).
        // Returns 404 if the product is missing, 400 if the result would be negative.
        [HttpPatch("{id:int}/stock", Name = "AdjustProductStock")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProductDto> AdjustStock(int id, [FromBody] StockAdjustmentRequest adjustment)
        {
            var result = _productService.AdjustStock(id, adjustment.Amount);
            if (!result.IsSuccess)
            {
                return this.Failure(result);
            }

            return result.Value!;
        }

        // Returns the recorded stock adjustments for a product, oldest first, or 404 if the product is not found
        [HttpGet("{id:int}/stock-history", Name = "GetProductStockHistory")]
        [ProducesResponseType(typeof(IEnumerable<StockAdjustmentRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StockAdjustmentRecordDto>> GetStockHistory(int id)
        {
            var history = _productService.GetStockHistory(id);
            if (history is null)
            {
                return NotFound();
            }

            return Ok(history);
        }
    }
}
