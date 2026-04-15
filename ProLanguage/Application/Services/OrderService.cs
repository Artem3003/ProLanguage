using System.Net.Http.Json;
using System.Text;
using Application.DTOs.Order;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IOrderCourseRepository orderCourseRepository,
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OrderService> logger) : IOrderService
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IOrderCourseRepository _orderCourseRepository = orderCourseRepository;
    private readonly ICourseRepository _courseRepository = courseRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<OrderService> _logger = logger;

    // Stub customer ID as per epic requirements
    private static readonly Guid StubCustomerId = Guid.Parse("5aa1c97e-e6b3-497c-8e00-270e96aa0b63");

    public async Task AddToCartAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId)
            ?? throw new KeyNotFoundException($"Course with ID {courseId} not found");

        // Get or create open order (cart)
        var cart = await _orderRepository.GetOpenOrderByCustomerIdAsync(StubCustomerId);

        if (cart == null)
        {
            cart = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = StubCustomerId,
                Status = OrderStatus.Open,
                Date = DateTime.UtcNow,
            };
            await _orderRepository.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        // Check if course already in cart
        var existingItem = await _orderCourseRepository.GetByOrderAndCourseAsync(cart.Id, courseId);

        if (existingItem != null)
        {
            // Increment quantity
            existingItem.Quantity++;
            _orderCourseRepository.Update(existingItem);
        }
        else
        {
            // Add new item
            var orderCourse = new OrderCourse
            {
                Id = Guid.NewGuid(),
                OrderId = cart.Id,
                CourseId = courseId,
                Price = course.Price,
                Quantity = 1,
                Discount = 0,
            };
            await _orderCourseRepository.AddAsync(orderCourse);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(Guid courseId)
    {
        var cart = await _orderRepository.GetOpenOrderByCustomerIdAsync(StubCustomerId)
            ?? throw new KeyNotFoundException("Cart not found");

        await _orderCourseRepository.DeleteByOrderAndCourseAsync(cart.Id, courseId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CartItemDto>> GetCartAsync()
    {
        var cart = await _orderRepository.GetOpenOrderByCustomerIdAsync(StubCustomerId);

        if (cart == null)
        {
            return [];
        }

        var items = await _orderCourseRepository.GetByOrderIdAsync(cart.Id);
        return _mapper.Map<IEnumerable<CartItemDto>>(items);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersAsync()
    {
        var orders = await _orderRepository.GetPaidAndCancelledOrdersByCustomerIdAsync(StubCustomerId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order == null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync(Guid orderId)
    {
        var items = await _orderCourseRepository.GetByOrderIdAsync(orderId);
        return _mapper.Map<IEnumerable<OrderDetailDto>>(items);
    }

    public PaymentMethodsResponseDto GetPaymentMethods()
    {
        return new PaymentMethodsResponseDto
        {
            PaymentMethods =
            [
                new PaymentMethodDto
                {
                    ImageUrl = "https://img.icons8.com/color/48/bank-building.png",
                    Title = "Bank",
                    Description = "Payment via bank transfer. Invoice provided.",
                },
                new PaymentMethodDto
                {
                    ImageUrl = "https://img.icons8.com/color/48/atm.png",
                    Title = "IBox terminal",
                    Description = "Pay easily through a terminal near you.",
                },
                new PaymentMethodDto
                {
                    ImageUrl = "https://img.icons8.com/color/48/visa.png",
                    Title = "Visa",
                    Description = "Instant online card payment.",
                },
            ],
        };
    }

    public async Task<object> ProcessPaymentAsync(PaymentRequestDto request)
    {
        var cart = await _orderRepository.GetOpenOrderByCustomerIdAsync(StubCustomerId)
            ?? throw new KeyNotFoundException("Cart not found");

        if (cart.OrderCourses.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty");
        }

        // Change status to Checkout
        cart.Status = OrderStatus.Checkout;
        _orderRepository.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        var totalSum = cart.OrderCourses.Sum(oc =>
            oc.Price * oc.Quantity * (1 - (oc.Discount / 100.0)));

        try
        {
            object result = request.Method switch
            {
                "Bank" => await ProcessBankPaymentAsync(cart, totalSum),
                "IBox terminal" => await ProcessIBoxPaymentAsync(cart, totalSum),
                "Visa" => await ProcessVisaPaymentAsync(cart, totalSum, request.Model!),
                _ => throw new ArgumentException($"Unknown payment method: {request.Method}"),
            };

            // Mark as paid
            cart.Status = OrderStatus.Paid;
            cart.Date = DateTime.UtcNow;
            _orderRepository.Update(cart);
            await _unitOfWork.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment failed for order {OrderId}", cart.Id);

            // Mark as cancelled
            cart.Status = OrderStatus.Cancelled;
            cart.Date = DateTime.UtcNow;
            _orderRepository.Update(cart);
            await _unitOfWork.SaveChangesAsync();

            throw;
        }
    }

    private async Task<byte[]> ProcessBankPaymentAsync(Order cart, double totalSum)
    {
        var invoiceValidityDays = _configuration.GetValue("PaymentSettings:BankInvoiceValidityDays", 30);
        var validityDate = DateTime.UtcNow.AddDays(invoiceValidityDays);

        // Generate PDF invoice
        var pdfContent = GenerateInvoicePdf(cart, totalSum, validityDate);

        await Task.CompletedTask;
        return pdfContent;
    }

    private async Task<PaymentResponseDto> ProcessIBoxPaymentAsync(Order cart, double totalSum)
    {
        var paymentServiceUrl = _configuration["PaymentSettings:PaymentServiceUrl"]
            ?? "http://localhost:5100";

        var client = _httpClientFactory.CreateClient();

        // Retry logic for fault tolerance (up to 3 attempts)
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var paymentRequest = new
                {
                    UserId = StubCustomerId,
                    CourseId = cart.OrderCourses.FirstOrDefault()?.CourseId,
                    Amount = (decimal)totalSum,
                    Currency = "USD",
                    PaymentMethod = 5, // Cash/Terminal
                    Description = $"Order {cart.Id} - IBox Terminal Payment",
                };

                var response = await client.PostAsJsonAsync(
                    $"{paymentServiceUrl}/api/payments",
                    paymentRequest);

                if (response.IsSuccessStatusCode)
                {
                    var paymentResult = await response.Content.ReadFromJsonAsync<PaymentServiceResponse>();

                    if (paymentResult != null)
                    {
                        // Process the payment
                        var processRequest = new
                        {
                            PaymentId = paymentResult.Id,
                            PaymentMethodToken = "ibox-terminal-token",
                            AdditionalData = new Dictionary<string, string>
                            {
                                { "terminal_id", "IBOX-001" },
                            },
                        };

                        var processResponse = await client.PostAsJsonAsync(
                            $"{paymentServiceUrl}/api/payments/{paymentResult.Id}/process",
                            processRequest);

                        if (processResponse.IsSuccessStatusCode)
                        {
                            return new PaymentResponseDto
                            {
                                UserId = StubCustomerId,
                                OrderId = cart.Id,
                                PaymentDate = DateTime.UtcNow,
                                Sum = totalSum,
                            };
                        }
                    }
                }

                _logger.LogWarning(
                    "IBox payment attempt {Attempt} failed with status {Status}",
                    attempt,
                    response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IBox payment attempt {Attempt} failed", attempt);
            }

            if (attempt < maxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt));
            }
        }

        throw new InvalidOperationException("Payment service unavailable after multiple retries");
    }

    private async Task<PaymentResponseDto> ProcessVisaPaymentAsync(
        Order cart,
        double totalSum,
        VisaPaymentModelDto visaModel)
    {
        var paymentServiceUrl = _configuration["PaymentSettings:PaymentServiceUrl"]
            ?? "http://localhost:5100";

        var client = _httpClientFactory.CreateClient();

        // Retry logic for fault tolerance (up to 3 attempts)
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var paymentRequest = new
                {
                    UserId = StubCustomerId,
                    CourseId = cart.OrderCourses.FirstOrDefault()?.CourseId,
                    Amount = (decimal)totalSum,
                    Currency = "USD",
                    PaymentMethod = 0, // CreditCard
                    Description = $"Order {cart.Id} - Visa Payment",
                };

                var response = await client.PostAsJsonAsync(
                    $"{paymentServiceUrl}/api/payments",
                    paymentRequest);

                if (response.IsSuccessStatusCode)
                {
                    var paymentResult = await response.Content.ReadFromJsonAsync<PaymentServiceResponse>();

                    if (paymentResult != null)
                    {
                        // Process the payment with card details
                        var processRequest = new
                        {
                            PaymentId = paymentResult.Id,
                            PaymentMethodToken = $"visa-{visaModel.CardNumber[^4..]}",
                            AdditionalData = new Dictionary<string, string>
                            {
                                { "card_holder", visaModel.Holder },
                                { "card_last_four", visaModel.CardNumber[^4..] },
                                { "expiry_month", visaModel.MonthExpire.ToString() },
                                { "expiry_year", visaModel.YearExpire.ToString() },
                            },
                        };

                        var processResponse = await client.PostAsJsonAsync(
                            $"{paymentServiceUrl}/api/payments/{paymentResult.Id}/process",
                            processRequest);

                        if (processResponse.IsSuccessStatusCode)
                        {
                            return new PaymentResponseDto
                            {
                                UserId = StubCustomerId,
                                OrderId = cart.Id,
                                PaymentDate = DateTime.UtcNow,
                                Sum = totalSum,
                            };
                        }
                    }
                }

                _logger.LogWarning(
                    "Visa payment attempt {Attempt} failed with status {Status}",
                    attempt,
                    response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Visa payment attempt {Attempt} failed", attempt);
            }

            if (attempt < maxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt));
            }
        }

        throw new InvalidOperationException("Payment service unavailable after multiple retries");
    }

    private static byte[] GenerateInvoicePdf(Order cart, double totalSum, DateTime validityDate)
    {
        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");
        sb.AppendLine("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj");
        sb.AppendLine("2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj");
        sb.AppendLine("3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj");
        sb.AppendLine("5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj");

        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 24 Tf");
        content.AppendLine("50 750 Td");
        content.AppendLine("(INVOICE) Tj");
        content.AppendLine("/F1 12 Tf");
        content.AppendLine("0 -30 Td");
        content.AppendLine($"(Invoice Number: INV-{cart.Id.ToString()[..8].ToUpper(System.Globalization.CultureInfo.CurrentCulture)}) Tj");
        content.AppendLine("0 -20 Td");
        content.AppendLine($"(User ID: {StubCustomerId}) Tj");
        content.AppendLine("0 -20 Td");
        content.AppendLine($"(Order ID: {cart.Id}) Tj");
        content.AppendLine("0 -20 Td");
        content.AppendLine($"(Creation Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC) Tj");
        content.AppendLine("0 -20 Td");
        content.AppendLine($"(Validity Date: {validityDate:yyyy-MM-dd HH:mm:ss} UTC) Tj");
        content.AppendLine("0 -40 Td");
        content.AppendLine("/F1 14 Tf");
        content.AppendLine("(ORDER DETAILS) Tj");
        content.AppendLine("/F1 12 Tf");
        content.AppendLine("0 -25 Td");

        foreach (var item in cart.OrderCourses)
        {
            var itemTotal = item.Price * item.Quantity * (1 - (item.Discount / 100.0));
            content.AppendLine($"(Course: {item.CourseId} - Qty: {item.Quantity} x ${item.Price:F2} = ${itemTotal:F2}) Tj");
            content.AppendLine("0 -15 Td");
        }

        content.AppendLine("0 -20 Td");
        content.AppendLine("/F1 16 Tf");
        content.AppendLine($"(TOTAL: ${totalSum:F2}) Tj");
        content.AppendLine("0 -40 Td");
        content.AppendLine("/F1 10 Tf");
        content.AppendLine("(Please pay within the validity period.) Tj");
        content.AppendLine("0 -15 Td");
        content.AppendLine("(Bank Account: IBAN UA123456789012345678901234567) Tj");
        content.AppendLine("ET");

        var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
        sb.AppendLine($"4 0 obj << /Length {contentBytes.Length} >> stream");
        sb.Append(content);
        sb.AppendLine("endstream endobj");
        sb.AppendLine("xref");
        sb.AppendLine("0 6");
        sb.AppendLine("0000000000 65535 f ");
        sb.AppendLine("0000000009 00000 n ");
        sb.AppendLine("0000000058 00000 n ");
        sb.AppendLine("0000000115 00000 n ");
        sb.AppendLine("0000000270 00000 n ");
        sb.AppendLine("0000000380 00000 n ");
        sb.AppendLine("trailer << /Size 6 /Root 1 0 R >>");
        sb.AppendLine("startxref");
        sb.AppendLine("459");
        sb.AppendLine("%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private class PaymentServiceResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public Guid Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public int Status { get; set; }
    }
}
