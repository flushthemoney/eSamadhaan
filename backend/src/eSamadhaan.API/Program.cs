using eSamadhaan.API.Middleware;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Application.Services;
using eSamadhaan.Infrastructure.Data;
using eSamadhaan.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. Database Configuration (Azure SQL)
// ========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);

// ========================================
// 2. Repository Registration
// ========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IGrievanceCategoryRepository, GrievanceCategoryRepository>();
builder.Services.AddScoped<IGrievanceRepository, GrievanceRepository>();
builder.Services.AddScoped<IGrievanceAssignmentRepository, GrievanceAssignmentRepository>();
builder.Services.AddScoped<IGrievanceStatusHistoryRepository, GrievanceStatusHistoryRepository>();
builder.Services.AddScoped<IGrievanceResolutionRepository, GrievanceResolutionRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

// ========================================
// 3. Application Service Registration
// ========================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGrievanceService, GrievanceService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IResolutionService, ResolutionService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ========================================
// 4. JWT Authentication Configuration
// ========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

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
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// ========================================
// 5. Authorization Policies
// ========================================
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("SystemAdminOnly", policy =>
        policy.RequireRole("SystemAdmin"));

    options.AddPolicy("DepartmentOfficerOnly", policy =>
        policy.RequireRole("DepartmentOfficer"));

    options.AddPolicy("SupervisoryOfficerOnly", policy =>
        policy.RequireRole("SupervisoryOfficer"));

    options.AddPolicy("CitizenOnly", policy =>
        policy.RequireRole("Citizen"));

    // Combined policies for multiple roles
    options.AddPolicy("AdminOrSupervisor", policy =>
        policy.RequireRole("SystemAdmin", "SupervisoryOfficer"));

    options.AddPolicy("OfficerOrSupervisor", policy =>
        policy.RequireRole("DepartmentOfficer", "SupervisoryOfficer"));

    options.AddPolicy("AllOfficers", policy =>
        policy.RequireRole("SystemAdmin", "DepartmentOfficer", "SupervisoryOfficer"));

    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// ========================================
// 6. Controllers and API Configuration
// ========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========================================
// 7. Swagger/OpenAPI Configuration
// ========================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "eSamadhaan API",
        Version = "v1",
        Description = "E-Governance Grievance Redressal System API",
        Contact = new OpenApiContact
        {
            Name = "eSamadhaan Support",
            Email = "support@esamadhaan.gov.in"
        }
    });

    // JWT Bearer authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// 8. CORS Configuration (Optional)
// ========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ========================================
// Middleware Pipeline Configuration
// ========================================

// Global Exception Handling (must be first)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "eSamadhaan API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAngularApp");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

app.Run();
