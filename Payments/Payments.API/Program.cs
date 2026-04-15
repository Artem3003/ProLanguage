using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Payments.Application.Interfaces;
using Payments.Application.Mappings;
using Payments.Application.Services;
using Payments.Domain.Data;
using Payments.Domain.Interfaces;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentsConnection"),
        b => b.MigrationsAssembly("Payments.Migrations")));

// Repository and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Application Services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// AutoMapper
builder.Services.AddAutoMapper(_ => { }, typeof(MappingProfile).Assembly);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payments Microservice API",
        Version = "v1",
        Description = "Payment processing microservice for English School Platform including payment management, transaction tracking, and refund operations.",
        Contact = new OpenApiContact
        {
            Name = "Payments Support",
            Email = "payments@englishschool.com",
        },
    });

    // Enable XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.TagActionsBy(api =>
    {
        if (api.ActionDescriptor.RouteValues["controller"] != null)
        {
            var controller = api.ActionDescriptor.RouteValues["controller"];
            return [controller switch
            {
                "Payments" => "Payments Management",
                "Transactions" => "Transaction Management",
                _ => controller,
            },
        ];
        }

        return ["General"];
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

Log.Information("Starting Payments Microservice");

app.Run();

Log.CloseAndFlush();
