using Application.DTOs.Order;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Orders Management")]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    /// <summary>
    /// Get all paid and cancelled orders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var orders = await _orderService.GetOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Get order by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return order == null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Get order details (courses in the order).
    /// </summary>
    [HttpGet("{id:guid}/details")]
    [ProducesResponseType(typeof(IEnumerable<OrderDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetails(Guid id)
    {
        var details = await _orderService.GetOrderDetailsAsync(id);
        return Ok(details);
    }

    /// <summary>
    /// Get cart (open order).
    /// </summary>
    [HttpGet("cart")]
    [ProducesResponseType(typeof(IEnumerable<CartItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
    {
        var cart = await _orderService.GetCartAsync();
        return Ok(cart);
    }

    /// <summary>
    /// Remove course from cart.
    /// </summary>
    [HttpDelete("cart/{courseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(Guid courseId)
    {
        try
        {
            await _orderService.RemoveFromCartAsync(courseId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get available payment methods.
    /// </summary>
    [HttpGet("payment-methods")]
    [ProducesResponseType(typeof(PaymentMethodsResponseDto), StatusCodes.Status200OK)]
    public ActionResult<PaymentMethodsResponseDto> GetPaymentMethods()
    {
        var methods = _orderService.GetPaymentMethods();
        return Ok(methods);
    }

    /// <summary>
    /// Process payment.
    /// </summary>
    [HttpPost("payment")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto request)
    {
        try
        {
            var result = await _orderService.ProcessPaymentAsync(request);

            // If Bank payment, return PDF
            return request.Method == "Bank" && result is byte[] pdfBytes
                ? File(pdfBytes, "application/pdf", $"invoice-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf")
                : Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
