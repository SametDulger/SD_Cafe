using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly IService<Table> _tableService;
        private readonly IOrderService _orderService;

        public TablesController(IService<Table> tableService, IOrderService orderService)
        {
            _tableService = tableService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Table>>> GetTables()
        {
            var tables = await _tableService.GetAllAsync();
            return Ok(tables);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Table>> GetTable(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            return Ok(table);
        }

        [HttpPost]
        public async Task<ActionResult<Table>> CreateTable(Table table)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTable = await _tableService.AddAsync(table);
            return CreatedAtAction(nameof(GetTable), new { id = createdTable.Id }, createdTable);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, Table table)
        {
            if (id != table.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            table.UpdatedDate = DateTime.Now;
            await _tableService.UpdateAsync(table);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            await _tableService.DeleteAsync(table);

            return NoContent();
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Table>>> GetAvailableTables()
        {
            var availableTables = await _tableService.FindAsync(t => !t.IsOccupied && !t.IsDeleted);
            return Ok(availableTables);
        }

        [HttpGet("occupied")]
        public async Task<ActionResult<IEnumerable<Table>>> GetOccupiedTables()
        {
            var occupiedTables = await _tableService.FindAsync(t => t.IsOccupied && !t.IsDeleted);
            return Ok(occupiedTables);
        }

        [HttpGet("status")]
        public async Task<ActionResult<IEnumerable<TableStatusResponse>>> GetTableStatus()
        {
            var tables = await _tableService.GetAllAsync();
            var orders = await _orderService.GetAllWithIncludesAsync(o => o.Payments, o => o.Table, o => o.Waiter);
            
            var tableStatuses = new List<TableStatusResponse>();
            
            foreach (var table in tables)
            {
                var activeOrder = orders.FirstOrDefault(o => 
                    o.TableId == table.Id && 
                    o.Status != OrderStatus.Cancelled &&
                    (o.Status != OrderStatus.Completed || !o.Payments.Any()));
                
                tableStatuses.Add(new TableStatusResponse
                {
                    Table = table,
                    ActiveOrder = activeOrder,
                    IsOccupied = activeOrder != null
                });
            }
            
            return Ok(tableStatuses);
        }
    }

    public class TableStatusResponse
    {
        public Table Table { get; set; } = null!;
        public Order? ActiveOrder { get; set; }
        public bool IsOccupied { get; set; }
    }
} 