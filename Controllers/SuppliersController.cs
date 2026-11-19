using Inventory.Api.Dtos;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public SuppliersController(ISupplierService supplierService, IProductService productService)
        {
            _supplierService = supplierService;
            _productService = productService;
        }

        // Returns all suppliers
        [HttpGet(Name = "GetSuppliers")]
        public IEnumerable<SupplierDto> Get()
        {
            return _supplierService.GetAll();
        }

        // Returns a single supplier by Id, or 404 if not found
        [HttpGet("{id:int}", Name = "GetSupplierById")]
        [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<SupplierDto> GetById(int id)
        {
            var supplier = _supplierService.GetById(id);
            if (supplier is null)
            {
                return NotFound();
            }

            return supplier;
        }

        // Creates a supplier. Returns 409 if the name is already used.
        [HttpPost(Name = "CreateSupplier")]
        [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<SupplierDto> Create([FromBody] SupplierRequest request)
        {
            var result = _supplierService.Create(request);
            if (!result.IsSuccess)
            {
                return this.Failure(result);
            }

            return CreatedAtRoute("GetSupplierById", new { id = result.Value!.Id }, result.Value);
        }

        // Updates a supplier's name and contact email
        [HttpPut("{id:int}", Name = "UpdateSupplier")]
        [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<SupplierDto> Update(int id, [FromBody] SupplierRequest request)
        {
            var result = _supplierService.Update(id, request);
            if (!result.IsSuccess)
            {
                return this.Failure(result);
            }

            return result.Value!;
        }

        // Deletes a supplier. Returns 409 if products are still supplied by it.
        [HttpDelete("{id:int}", Name = "DeleteSupplier")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Delete(int id)
        {
            var hasProducts = _productService.GetBySupplier(id).Count > 0;
            var result = _supplierService.Delete(id, hasProducts);
            return result.IsSuccess ? NoContent() : this.Failure(result);
        }

        // Returns the products from a supplier, or 404 if the supplier is not found
        [HttpGet("{id:int}/products", Name = "GetSupplierProducts")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProductDto>> GetProducts(int id)
        {
            if (_supplierService.GetById(id) is null)
            {
                return NotFound();
            }

            return Ok(_productService.GetBySupplier(id));
        }
    }
}
