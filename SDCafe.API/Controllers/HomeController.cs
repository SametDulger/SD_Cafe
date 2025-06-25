using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
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

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardResponse>> GetDashboard()
        {
            var dashboard = new DashboardResponse
            {
                TotalProducts = await _productService.CountAsync(p => !p.IsDeleted),
                ActiveOrders = await _orderService.CountAsync(o => o.Status != OrderStatus.Completed && !o.IsDeleted),
                AvailableTables = await _tableService.CountAsync(t => !t.IsOccupied)
            };

            return Ok(dashboard);
        }
    }

    public class DashboardResponse
    {
        public int TotalProducts { get; set; }
        public int ActiveOrders { get; set; }
        public int AvailableTables { get; set; }
    }
} 