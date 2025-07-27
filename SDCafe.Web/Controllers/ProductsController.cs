using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDCafe.Business;
using SDCafe.Entities;
using SDCafe.Web.Models;

namespace SDCafe.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string searchTerm, int? categoryId)
        {
            var categories = await _categoryService.GetAllAsync() ?? Enumerable.Empty<Category>();
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

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Categories = categories,
                SearchTerm = searchTerm,
                SelectedCategoryId = categoryId
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdWithIncludesAsync(id, p => p.Category);
            if (product == null)
            {
                return NotFound();
            }
            return View(product!);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories ?? Enumerable.Empty<Category>(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productService.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryService.GetAllAsync();
            ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories ?? Enumerable.Empty<Category>(), "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdWithIncludesAsync(id, p => p.Category);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryService.GetAllAsync();
            ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories ?? Enumerable.Empty<Category>(), "Id", "Name");
            return View(product!);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    var categories = await _categoryService.GetAllAsync();
                    ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories ?? Enumerable.Empty<Category>(), "Id", "Name");
                    return View(product);
                }

                product.UpdatedDate = DateTime.Now;
                await _productService.UpdateAsync(product);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Güncelleme sırasında hata oluştu: {ex.Message}");
                var categories = await _categoryService.GetAllAsync();
                ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories ?? Enumerable.Empty<Category>(), "Id", "Name");
                return View(product);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                product.UpdatedDate = DateTime.Now;
                await _productService.UpdateAsync(product);
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 