using Application.DTOs.Order;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Orders Management")]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    /// <summary>
    /// Get all paid and cancelled orders for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var orders = await _orderService.GetOrdersAsync(userId.Value);
        return Ok(orders);
    }

    /// <summary>
    /// Get one of the current user's orders by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var order = await _orderService.GetOrderByIdAsync(userId.Value, id);
        return order == null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Get order details (courses in the order) for one of the current user's orders.
    /// </summary>
    [HttpGet("{id:guid}/details")]
    [ProducesResponseType(typeof(IEnumerable<OrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetails(Guid id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var details = await _orderService.GetOrderDetailsAsync(userId.Value, id);
        return Ok(details);
    }

    /// <summary>
    /// Get the current user's cart (open order).
    /// </summary>
    [HttpGet("cart")]
    [ProducesResponseType(typeof(IEnumerable<CartItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await _orderService.GetCartAsync(userId.Value);
        return Ok(cart);
    }

    /// <summary>
    /// Remove course from the current user's cart.
    /// </summary>
    [HttpDelete("cart/{courseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(Guid courseId)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _orderService.RemoveFromCartAsync(userId.Value, courseId);
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
    /// Process payment for the current user's cart.
    /// </summary>
    [HttpPost("payment")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _orderService.ProcessPaymentAsync(userId.Value, request);

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
