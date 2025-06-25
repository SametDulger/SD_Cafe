using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDCafe.Business;
using SDCafe.Entities;
using SDCafe.Web.Models;

namespace SDCafe.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public class TablesController : Controller
    {
        private readonly IService<Table> _tableService;
        private readonly IOrderService _orderService;

        public TablesController(IService<Table> tableService, IOrderService orderService)
        {
            _tableService = tableService;
            _orderService = orderService;
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index()
        {
            var tables = await _tableService.GetAllAsync();
            return View(tables);
        }

        [Authorize(Roles = "Admin,Manager,Cashier")]
        public async Task<IActionResult> Details(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(Table table)
        {
            if (ModelState.IsValid)
            {
                await _tableService.AddAsync(table);
                return RedirectToAction(nameof(Index));
            }

            return View(table);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, Table table)
        {
            if (id != table.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                table.UpdatedDate = DateTime.Now;
                await _tableService.UpdateAsync(table);
                return RedirectToAction(nameof(Index));
            }

            return View(table);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table != null)
            {
                await _tableService.DeleteAsync(table);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Available()
        {
            var availableTables = await _tableService.FindAsync(t => !t.IsOccupied && !t.IsDeleted);
            return View(availableTables);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Occupied()
        {
            var occupiedTables = await _tableService.FindAsync(t => t.IsOccupied && !t.IsDeleted);
            return View(occupiedTables);
        }

        [Authorize(Roles = "Admin,Manager,Cashier")]
        public async Task<IActionResult> Status()
        {
            var tables = await _tableService.GetAllAsync();
            var orders = await _orderService.GetAllWithIncludesAsync(o => o.Payments, o => o.Table, o => o.Waiter);
            
            var tableStatuses = new List<TableStatusViewModel>();
            
            foreach (var table in tables)
            {
                // Aktif sipariş: Tamamlanmamış veya tamamlanmış ama ödemesi alınmamış
                var activeOrder = orders.FirstOrDefault(o => 
                    o.TableId == table.Id && 
                    o.Status != OrderStatus.Cancelled &&
                    (o.Status != OrderStatus.Completed || !o.Payments.Any()));
                
                tableStatuses.Add(new TableStatusViewModel
                {
                    Table = table,
                    ActiveOrder = activeOrder,
                    IsOccupied = activeOrder != null
                });
            }
            
            return View(tableStatuses);
        }
    }
} 