using System.Text;
using System.Text.Json.Serialization;
using Application.Constants;
using Application.Filters;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Domain.Data;
using Domain.Interfaces;
using Domain.Repositories;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure Serilog from appsettings.json
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
{
    KeyId = "ProLanguageSigningKey",
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.Zero,
    };
});

// Authorization policies for role-based access control
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ContentManagement", policy =>
        policy.RequireRole("Admin", "Manager", "Teacher"))
    .AddPolicy("TeacherAccess", policy =>
        policy.RequireRole("Admin", "Manager", "Teacher"))
    .AddPolicy("GradingAccess", policy =>
        policy.RequireRole("Admin", "Teacher"))
    .AddPolicy("StudentAccess", policy =>
        policy.RequireRole("Admin", "Manager", "Teacher", "Student"))
    .AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IHomeworkAssignmentRepository, HomeworkAssignmentRepository>();
builder.Services.AddScoped<IHomeworkRepository, HomeworkRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IStudentLessonRepository, StudentLessonRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderCourseRepository, OrderCourseRepository>();

// Application Services
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IHomeworkAssignmentService, HomeworkAssignmentService>();
builder.Services.AddScoped<IHomeworkService, HomeworkService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Course Filter Pipeline
builder.Services.AddScoped<ICourseFilter, CourseFilter>();
builder.Services.AddScoped<ICourseSorter, CourseSorter>();
builder.Services.AddScoped<ICoursePaginator, CoursePaginator>();
builder.Services.AddScoped<ICourseFilterPipeline, CourseFilterPipeline>();

// HttpClient for Payment microservice
builder.Services.AddHttpClient();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Memory Cache
builder.Services.AddMemoryCache();
builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("CacheSettings"));

// Response Caching
builder.Services.AddResponseCaching();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProLanguage Platform API",
        Version = "v1",
        Description = "Comprehensive API for ProLanguage Management System including lessons, homework, calendar events, and assignments management.",
        Contact = new OpenApiContact
        {
            Name = "ProLanguage Platform Support",
            Email = "support@prolanguage.com",
        },
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });

    // Enable XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configure Swagger to include all controller actions
    c.DocInclusionPredicate((name, api) => true);

    // Add operation tags for better organization
    c.TagActionsBy(api =>
    {
        if (api.ActionDescriptor.RouteValues["controller"] != null)
        {
            var controller = api.ActionDescriptor.RouteValues["controller"];
            return [controller switch
            {
                "Lessons" => "Lessons Management",
                "Courses" => "Courses Management",
                "Homeworks" => "Homework Management",
                "Calendar" => "Calendar Events",
                "Assignments" => "Homework Assignments",
                _ => controller,
            },
        ];
        }

        return ["General"];
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("x-total-number-of-lessons");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProLanguage Platform API v1.0.0");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "ProLanguage Platform API Documentation";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Middlewares
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<TotalLessonsHeaderMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
