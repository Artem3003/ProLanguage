using AutoMapper;
using Payments.Application.DTOs.Transaction;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Interfaces;

namespace Payments.Application.Services;

/// <summary>
/// Service implementation for transaction operations.
/// </summary>
/// <param name="transactionRepository">The transaction repository.</param>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public class TransactionService(
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    /// <inheritdoc/>
    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto)
    {
        var transaction = _mapper.Map<Transaction>(createTransactionDto);
        transaction.CreatedAt = DateTime.UtcNow;

        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TransactionDto>(transaction);
    }

    /// <inheritdoc/>
    public async Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        return transaction == null ? null : _mapper.Map<TransactionDto>(transaction);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TransactionDto>> GetTransactionsByPaymentIdAsync(Guid paymentId)
    {
        var transactions = await _transactionRepository.GetTransactionsByPaymentIdAsync(paymentId);
        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }
}
