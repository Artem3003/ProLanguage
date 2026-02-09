using AutoMapper;
using Payments.Application.DTOs.Payment;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Entities.Enums;
using Payments.Domain.Interfaces;

namespace Payments.Application.Services;

/// <summary>
/// Service implementation for payment operations.
/// </summary>
/// <param name="paymentRepository">The payment repository.</param>
/// <param name="transactionRepository">The transaction repository.</param>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public class PaymentService(
    IPaymentRepository paymentRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    /// <inheritdoc/>
    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
    {
        var payment = _mapper.Map<Payment>(createPaymentDto);
        payment.Status = PaymentStatus.Pending;
        payment.CreatedAt = DateTime.UtcNow;

        await _paymentRepository.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    /// <inheritdoc/>
    public async Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetPaymentWithTransactionsAsync(paymentId);
        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(Guid userId)
    {
        var payments = await _paymentRepository.GetPaymentsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PaymentDto>> GetPaymentsByCourseIdAsync(Guid courseId)
    {
        var payments = await _paymentRepository.GetPaymentsByCourseIdAsync(courseId);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto processPaymentDto)
    {
        var payment = await _paymentRepository.GetByIdAsync(processPaymentDto.PaymentId);
        
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Payment is not in pending status");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Update payment status
            payment.Status = PaymentStatus.Processing;
            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            // Simulate payment processing (in real scenario, integrate with payment gateway)
            var isPaymentSuccessful = await SimulatePaymentProcessing(payment, processPaymentDto);

            if (isPaymentSuccessful)
            {
                payment.Status = PaymentStatus.Completed;
                payment.CompletedAt = DateTime.UtcNow;
                payment.TransactionId = Guid.NewGuid().ToString();
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = "Payment processing failed";
            }

            _paymentRepository.Update(payment);

            // Create transaction record
            var transaction = new Transaction
            {
                PaymentId = payment.Id!,
                TransactionType = "Charge",
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                GatewayTransactionId = payment.TransactionId,
                CreatedAt = DateTime.UtcNow,
                Notes = isPaymentSuccessful ? "Payment processed successfully" : "Payment failed"
            };

            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<PaymentDto>(payment);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> UpdatePaymentStatusAsync(UpdatePaymentStatusDto updateStatusDto)
    {
        var payment = await _paymentRepository.GetByIdAsync(updateStatusDto.PaymentId);
        
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        payment.Status = updateStatusDto.Status;
        payment.TransactionId = updateStatusDto.TransactionId ?? payment.TransactionId;
        payment.FailureReason = updateStatusDto.FailureReason;
        payment.PaymentGatewayResponse = updateStatusDto.PaymentGatewayResponse;

        if (updateStatusDto.Status == PaymentStatus.Completed)
        {
            payment.CompletedAt = DateTime.UtcNow;
        }

        _paymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    /// <inheritdoc/>
    public async Task<PaymentDto> RefundPaymentAsync(RefundPaymentDto refundDto)
    {
        var payment = await _paymentRepository.GetByIdAsync(refundDto.PaymentId);
        
        if (payment == null)
        {
            throw new InvalidOperationException("Payment not found");
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            throw new InvalidOperationException("Only completed payments can be refunded");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var refundAmount = refundDto.RefundAmount ?? payment.Amount;

            // Update payment status
            payment.Status = PaymentStatus.Refunded;
            _paymentRepository.Update(payment);

            // Create refund transaction
            var transaction = new Transaction
            {
                PaymentId = payment.Id!,
                TransactionType = "Refund",
                Amount = refundAmount,
                Status = "Completed",
                GatewayTransactionId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                Notes = refundDto.Reason ?? "Payment refunded"
            };

            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<PaymentDto>(payment);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await _paymentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePaymentAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        
        if (payment == null)
        {
            return false;
        }

        _paymentRepository.Delete(payment);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static Task<bool> SimulatePaymentProcessing(Payment payment, ProcessPaymentDto processPaymentDto)
    {
        // In a real scenario, integrate with payment gateway (Stripe, PayPal, etc.)
        // For now, simulate success
        return Task.FromResult(true);
    }
}
