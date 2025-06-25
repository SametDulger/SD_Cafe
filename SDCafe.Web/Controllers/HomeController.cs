using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDCafe.Business;
using SDCafe.Entities;
using SDCafe.Web.Models;

namespace SDCafe.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;
        private readonly IService<Table> _tableService;

        public HomeController(
            IProductService productService,
            IOrderService orderService,
            ICategoryService categoryService,
            IService<Table> tableService)
        {
            _productService = productService;
            _orderService = orderService;
            _categoryService = categoryService;
            _tableService = tableService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                var dashboardViewModel = new DashboardViewModel
                {
                    TotalProducts = await _productService.CountAsync(p => !p.IsDeleted),
                    ActiveOrders = await _orderService.CountAsync(o => o.Status != OrderStatus.Completed && !o.IsDeleted),
                    AvailableTables = await _tableService.CountAsync(t => !t.IsOccupied)
                };
                return View(dashboardViewModel);
            }
            
            if (User.IsInRole("Waiter"))
                return RedirectToAction("Index", "Orders");
            if (User.IsInRole("Kitchen"))
                return RedirectToAction("Index", "Kitchen");
            if (User.IsInRole("Cashier"))
                return RedirectToAction("Index", "Orders");
            if (User.IsInRole("Accounting"))
                return RedirectToAction("Index", "Accounting");
            
            return RedirectToAction("AccessDenied", "Auth");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}
