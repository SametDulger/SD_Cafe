using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using SDCafe.Business;
using SDCafe.Entities;
using SDCafe.Web.Models;

namespace SDCafe.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager,Waiter,Cashier")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IService<Table> _tableService;
        private readonly IService<User> _userService;

        public OrdersController(
            IOrderService orderService,
            IProductService productService,
            IService<Table> tableService,
            IService<User> userService)
        {
            _orderService = orderService;
            _productService = productService;
            _tableService = tableService;
            _userService = userService;
        }

        [Authorize(Roles = "Admin,Manager,Waiter,Cashier")]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Waiter"))
            {
                var currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var orders = await _orderService.FindAsync(o => o.Waiter.Email == currentUserEmail);
                var waiterOrders = await _orderService.GetAllWithIncludesAsync(o => o.Table, o => o.Waiter, o => o.Payments);
                var waiterUnpaidOrders = waiterOrders.Where(o => o.Waiter.Email == currentUserEmail && !o.Payments.Any()).ToList();
                return View(waiterUnpaidOrders);
            }
            else
            {
                var orders = await _orderService.GetAllWithIncludesAsync(o => o.Table, o => o.Waiter, o => o.Payments);
                var unpaidOrders = orders.Where(o => !o.Payments.Any()).ToList();
                return View(unpaidOrders);
            }
        }

        [Authorize(Roles = "Admin,Manager,Waiter,Cashier")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetByIdWithNestedIncludesAsync(id, 
                "Table", 
                "Waiter", 
                "OrderItems.Product");
            
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [Authorize(Roles = "Admin,Manager,Waiter")]
        public async Task<IActionResult> Create(int? tableId = null)
        {
            var tables = await _tableService.FindAsync(t => !t.IsOccupied);
            var products = await _productService.GetAllAsync();

            // Kullanıcının rolüne göre garson seçimi
            List<SelectListItem> waiters;
            if (User.IsInRole("Waiter"))
            {
                var currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var currentUser = await _userService.FindAsync(u => u.Email == currentUserEmail);
                var currentWaiter = currentUser.FirstOrDefault();
                
                waiters = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = currentWaiter?.Id.ToString() ?? "0",
                        Text = $"{currentWaiter?.FirstName} {currentWaiter?.LastName}",
                        Selected = true
                    }
                };
            }
            else
            {
                var allWaiters = await _userService.FindAsync(u => u.Role == UserRole.Waiter);
                waiters = allWaiters.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.FirstName} {w.LastName}"
                }).ToList();
            }

            var viewModel = new OrderCreateViewModel
            {
                Tables = tables.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"Masa {t.TableNumber} ({t.Capacity} kişilik)"
                }).ToList(),
                Waiters = waiters,
                Products = products.Select(p => new ProductSelectItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name,
                    Price = p.Price
                }).ToList()
            };

            if (tableId.HasValue)
            {
                viewModel.TableId = tableId.Value;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Waiter")]
        public async Task<IActionResult> Create(OrderCreateViewModel viewModel, List<int> ProductIds, List<int> Quantities)
        {
            if (ModelState.IsValid && ProductIds != null && ProductIds.Any())
            {
                try
                {
                    var orderItems = new List<OrderItemDto>();
                    for (int i = 0; i < ProductIds.Count; i++)
                    {
                        if (ProductIds[i] > 0 && Quantities[i] > 0)
                        {
                            orderItems.Add(new OrderItemDto
                            {
                                ProductId = ProductIds[i],
                                Quantity = Quantities[i]
                            });
                        }
                    }

                    if (orderItems.Any())
                    {
                        await _orderService.CreateOrderAsync(viewModel.TableId, viewModel.WaiterId, orderItems, viewModel.Notes);
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "En az bir ürün seçilmelidir.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            else if (ProductIds == null || !ProductIds.Any())
            {
                ModelState.AddModelError("", "En az bir ürün seçilmelidir.");
            }

            // Hata durumunda view model'i yeniden doldur
            var tables = await _tableService.FindAsync(t => !t.IsOccupied);
            var products = await _productService.GetAllAsync();

            // Kullanıcının rolüne göre garson seçimi
            List<SelectListItem> waiters;
            if (User.IsInRole("Waiter"))
            {
                // Garson ise sadece kendi hesabını göster
                var currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var currentUser = await _userService.FindAsync(u => u.Email == currentUserEmail);
                var currentWaiter = currentUser.FirstOrDefault();
                
                waiters = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = currentWaiter?.Id.ToString() ?? "0",
                        Text = $"{currentWaiter?.FirstName} {currentWaiter?.LastName}",
                        Selected = true
                    }
                };
            }
            else
            {
                // Admin/Manager ise tüm garsonları göster
                var allWaiters = await _userService.FindAsync(u => u.Role == UserRole.Waiter);
                waiters = allWaiters.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.FirstName} {w.LastName}"
                }).ToList();
            }

            viewModel.Tables = tables.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"Masa {t.TableNumber} ({t.Capacity} kişilik)"
            }).ToList();
            viewModel.Waiters = waiters;
            viewModel.Products = products.Select(p => new ProductSelectItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Price = p.Price
            }).ToList();

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetByIdWithIncludesAsync(id, o => o.Table, o => o.Waiter);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, OrderStatus status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, status);
            if (success)
            {
                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }

        public async Task<IActionResult> Active()
        {
            var activeOrders = await _orderService.FindAsync(o => o.Status != OrderStatus.Completed && !o.IsDeleted);
            return View(activeOrders);
        }

        public async Task<IActionResult> ByStatus(OrderStatus status)
        {
            var orders = await _orderService.GetByStatusAsync(status);
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Waiter")]
        public async Task<IActionResult> DeliverOrder(int orderId)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Completed);
            if (success)
            {
                TempData["Success"] = "Sipariş başarıyla teslim edildi.";
            }
            else
            {
                TempData["Error"] = "Sipariş teslim edilirken hata oluştu.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
} 