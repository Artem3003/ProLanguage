# Payments Microservice

A comprehensive payment processing microservice for the ProLanguage Platform, built with ASP.NET Core and Entity Framework Core.

## Overview

This microservice handles all payment-related operations including:
- Payment creation and processing
- Transaction tracking
- Refund management
- Payment status monitoring
- Multi-payment method support (Credit Card, Debit Card, PayPal, Bank Transfer, Stripe, Cash)

## Architecture

The microservice follows Clean Architecture principles with the following layers:

### Payments.Domain
- **Entities**: Payment, Transaction
- **Enums**: PaymentStatus, PaymentMethod
- **Repositories**: Payment and Transaction repositories with specialized queries
- **DbContext**: PaymentsDbContext with EF Core configuration

### Payments.Application
- **DTOs**: Data transfer objects for requests and responses
- **Services**: Business logic for payment processing
- **Interfaces**: Service contracts
- **Mappings**: AutoMapper profiles

### Payments.Infrastructure
- **Middleware**: 
  - GlobalExceptionHandlingMiddleware
  - RequestLoggingMiddleware

### Payments.API
- **Controllers**: RESTful API endpoints
- **Configuration**: Swagger, CORS, Logging

### Payments.Migrations
- EF Core migrations for database schema management

## Features

### Payment Management
- Create new payments
- Process payments with gateway simulation
- Update payment status
- Refund completed payments
- Track payment history by user or course

### Transaction Tracking
- Automatic transaction logging
- Support for multiple transaction types (Charge, Refund, Authorization)
- Gateway response tracking

### Payment Status
- Pending
- Processing
- Completed
- Failed
- Refunded
- Cancelled

## API Endpoints

### Payments
- `GET /api/payments` - Get all payments
- `GET /api/payments/{id}` - Get payment by ID
- `GET /api/payments/user/{userId}` - Get payments by user
- `GET /api/payments/course/{courseId}` - Get payments by course
- `POST /api/payments` - Create new payment
- `POST /api/payments/{id}/process` - Process payment
- `PATCH /api/payments/{id}/status` - Update payment status
- `POST /api/payments/{id}/refund` - Refund payment
- `DELETE /api/payments/{id}` - Delete payment

### Transactions
- `GET /api/transactions` - Get all transactions
- `GET /api/transactions/{id}` - Get transaction by ID
- `GET /api/transactions/payment/{paymentId}` - Get transactions by payment
- `POST /api/transactions` - Create new transaction

## Configuration

### Connection String
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "PaymentsConnection": "Server=(localdb)\\mssqllocaldb;Database=PaymentsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Ports
- HTTP: `http://localhost:5100`
- HTTPS: `https://localhost:7100`

## Database Setup

1. Navigate to the Migrations project:
```bash
cd Payments.Migrations
```

2. Create initial migration:
```bash
dotnet ef migrations add InitialCreate --startup-project ../Payments.API
```

3. Update database:
```bash
dotnet ef database update --startup-project ../Payments.API
```

## Running the Microservice

1. Navigate to the API project:
```bash
cd Payments.API
```

2. Run the application:
```bash
dotnet run
```

3. Access Swagger UI at: `https://localhost:7100` or `http://localhost:5100`

## Technologies

- **ASP.NET Core 9.0**
- **Entity Framework Core 9.0**
- **SQL Server**
- **AutoMapper**
- **FluentValidation**
- **Serilog**
- **Swagger/OpenAPI**

## Integration

This microservice is designed to work alongside the main ProLanguage Platform. It can be called by:
- The main Web API for course enrollment payments
- Frontend applications for payment processing
- Other microservices requiring payment functionality

## Payment Gateway Integration

Currently, the payment processing is simulated. To integrate with real payment gateways:

1. Implement gateway-specific services (Stripe, PayPal, etc.)
2. Update the `ProcessPaymentAsync` method in `PaymentService`
3. Add necessary NuGet packages for gateway SDKs
4. Configure API keys in `appsettings.json`

## Security Considerations

- All sensitive payment data should be encrypted
- Implement authentication and authorization
- Use HTTPS in production
- Store API keys securely (Azure Key Vault, etc.)
- Implement rate limiting
- Add PCI DSS compliance measures

## Logging

Logs are written to:
- Console (Development)
- File: `logs/payments-{Date}.log`

## Error Handling

The microservice includes global exception handling that returns structured error responses:
```json
{
  "error": {
    "message": "Error description",
    "statusCode": 400,
    "timestamp": "2026-01-07T12:00:00Z"
  }
}
```

## Future Enhancements

- [ ] Payment gateway integration (Stripe, PayPal)
- [ ] Webhook support for payment notifications
- [ ] Payment plans and subscriptions
- [ ] Multi-currency support
- [ ] Payment analytics and reporting
- [ ] Fraud detection
- [ ] Payment scheduling
- [ ] Invoice generation

## License

This project is part of the ProLanguage Platform.
