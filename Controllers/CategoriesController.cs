using Inventory.Api.Dtos;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public CategoriesController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        // Returns all categories
        [HttpGet(Name = "GetCategories")]
        public IEnumerable<CategoryDto> Get()
        {
            return _categoryService.GetAll();
        }

        // Returns a single category by Id, or 404 if not found
        [HttpGet("{id:int}", Name = "GetCategoryById")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CategoryDto> GetById(int id)
        {
            var category = _categoryService.GetById(id);
            if (category is null)
            {
                return NotFound();
            }

            return category;
        }

        // Creates a category. Returns 409 if the name is already used.
        [HttpPost(Name = "CreateCategory")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<CategoryDto> Create([FromBody] CategoryRequest request)
        {
            var result = _categoryService.Create(request);
            if (!result.IsSuccess)
            {
                return this.Failure(result);
            }

            return CreatedAtRoute("GetCategoryById", new { id = result.Value!.Id }, result.Value);
        }

        // Renames a category
        [HttpPut("{id:int}", Name = "UpdateCategory")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<CategoryDto> Update(int id, [FromBody] CategoryRequest request)
        {
            var result = _categoryService.Update(id, request);
            if (!result.IsSuccess)
            {
                return this.Failure(result);
            }

            return result.Value!;
        }

        // Deletes a category. Returns 409 if products still belong to it.
        [HttpDelete("{id:int}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Delete(int id)
        {
            var hasProducts = _productService.GetByCategory(id).Count > 0;
            var result = _categoryService.Delete(id, hasProducts);
            return result.IsSuccess ? NoContent() : this.Failure(result);
        }

        // Returns the products in a category, or 404 if the category is not found
        [HttpGet("{id:int}/products", Name = "GetCategoryProducts")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProductDto>> GetProducts(int id)
        {
            if (_categoryService.GetById(id) is null)
            {
                return NotFound();
            }

            return Ok(_productService.GetByCategory(id));
        }
    }
}
