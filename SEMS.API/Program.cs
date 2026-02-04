using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SEMS.Core.Common;
using SEMS.Infrastructure.Caching;
using SEMS.Infrastructure.Messaging;
using SEMS.Infrastructure.Persistence;
using SEMS.Infrastructure.Services;
using FluentValidation;
using SEMS.Infrastructure.Events;
using SEMS.Application.Events;
using SEMS.Application.Abstractions;
using SEMS.API.Services;
using SEMS.Infrastructure.Identity;
using SEMS.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using SEMS.Application.Behaviors;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, log) =>
{
    log.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, SEMS.API.Services.TenantProvider>();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddDbContext<SemsDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IDomainEventHandler<SEMS.Core.DomainEvents.EmployeeCreated>, EmployeeCreatedHandler>();
builder.Services.AddScoped<IDomainEventHandler<SEMS.Core.DomainEvents.InvoicePaid>, InvoicePaidHandler>();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(SEMS.Application.Employees.CreateEmployee).Assembly);
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<SEMS.Application.Employees.CreateEmployee>();

if (builder.Configuration.GetValue<bool>("UseRedis"))
{
    builder.Services.AddSingleton<ICacheService>(_ => new RedisCacheService(builder.Configuration.GetSection("Redis")["ConnectionString"]!));
}
else
{
    builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
}

if (builder.Configuration.GetValue<bool>("UseRabbitMq"))
{
    builder.Services.AddSingleton<IMessageBus>(_ => new RabbitMqBus(builder.Configuration.GetSection("RabbitMq")["ConnectionString"]!));
}
else
{
    builder.Services.AddSingleton<IMessageBus, InMemoryBus>();
}

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ISmsService, SmsService>();
builder.Services.AddSingleton<IPdfGenerator, PdfGenerator>();
builder.Services.AddSingleton<IPaymentGateway, PaymentGateway>();
builder.Services.AddHostedService<DbSeederHostedService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddSingleton<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SEMS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));
builder.Services.AddHealthChecks().AddSqlite(builder.Configuration.GetConnectionString("Default")!);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<SEMS.API.Middlewares.GlobalExceptionMiddleware>();
app.UseCors();
app.UseRateLimiter();

// BYPASS AUTH FOR DEV
// app.Use(async (context, next) =>
// {
//     var claims = new[]
//     {
//         new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "DevAdmin"),
//         new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"),
//         new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "HR")
//     };
//     var identity = new System.Security.Claims.ClaimsIdentity(claims, "Dev");
//     context.User = new System.Security.Claims.ClaimsPrincipal(identity);
//     await next();
// });

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
