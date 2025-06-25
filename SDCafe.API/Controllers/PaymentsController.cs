using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentsController(IPaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            var payments = await _paymentService.GetAllAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByOrder(int orderId)
        {
            var payments = await _paymentService.GetByOrderAsync(orderId);
            return Ok(payments);
        }

        [HttpGet("cashier/{cashierId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByCashier(int cashierId)
        {
            var payments = await _paymentService.GetByCashierAsync(cashierId);
            return Ok(payments);
        }

        [HttpGet("order/{orderId}/total")]
        public async Task<ActionResult<decimal>> GetTotalPaymentsByOrder(int orderId)
        {
            var total = await _paymentService.GetTotalPaymentsByOrderAsync(orderId);
            return Ok(total);
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment(CreatePaymentRequest request)
        {
            try
            {
                var payment = await _paymentService.CreatePaymentAsync(
                    request.OrderId, 
                    request.CashierId, 
                    request.Amount, 
                    request.PaymentMethod);
                
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompletePayment(int id)
        {
            var success = await _paymentService.CompletePaymentAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, Payment payment)
        {
            if (id != payment.Id)
            {
                return BadRequest();
            }

            payment.UpdatedDate = DateTime.Now;
            await _paymentService.UpdateAsync(payment);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            payment.IsDeleted = true;
            payment.UpdatedDate = DateTime.Now;
            await _paymentService.UpdateAsync(payment);
            return NoContent();
        }
    }

    public class CreatePaymentRequest
    {
        public int OrderId { get; set; }
        public int CashierId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
} 