using Application.Constants;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Domain.Data;
using Domain.Interfaces;
using Domain.Repositories;
using Infrastructure.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Serilog from appsettings.json
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

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

// Application Services
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IHomeworkAssignmentService, HomeworkAssignmentService>();
builder.Services.AddScoped<IHomeworkService, HomeworkService>();
builder.Services.AddScoped<ILessonService, LessonService>();

// AutoMapper
builder.Services.AddAutoMapper(_ => { }, typeof(MappingProfile));

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
        Title = "English School Platform API",
        Version = "v1",
        Description = "Comprehensive API for English School Management System including lessons, homework, calendar events, and assignments management.",
        Contact = new OpenApiContact
        {
            Name = "English School Platform Support",
            Email = "support@englishschool.com",
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "English School Platform API v1.0.0");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "English School Platform API Documentation";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Middlewares
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<TotalLessonsHeaderMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
