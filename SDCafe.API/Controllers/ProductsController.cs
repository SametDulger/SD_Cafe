using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

#pragma warning disable CS8603

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(string searchTerm, int? categoryId)
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(searchTerm) && categoryId.HasValue)
            {
                var searchResults = await _productService.SearchAsync(searchTerm);
                products = (searchResults ?? Enumerable.Empty<Product>()).Where(p => p.CategoryId == categoryId.Value);
            }
            else if (!string.IsNullOrEmpty(searchTerm))
            {
                products = await _productService.SearchAsync(searchTerm) ?? Enumerable.Empty<Product>();
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetByCategoryAsync(categoryId.Value) ?? Enumerable.Empty<Product>();
            }
            else
            {
                products = await _productService.GetAllAsync() ?? Enumerable.Empty<Product>();
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetByIdWithIncludesAsync(id, p => p.Category);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product!);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdProduct = await _productService.AddAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            product.UpdatedDate = DateTime.Now;
            await _productService.UpdateAsync(product);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.IsDeleted = true;
            product.UpdatedDate = DateTime.Now;
            await _productService.UpdateAsync(product);

            return NoContent();
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _categoryService.GetAllAsync() ?? Enumerable.Empty<Category>();
            return Ok(categories);
        }
    }
} 