using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PollingSurvey.Infrastructure.Data;
using PollingSurvey.API.Hubs;
using PollingSurvey.API.Realtime;
using PollingSurvey.Application.Interfaces;
using PollingSurvey.Application.Repositories;
using PollingSurvey.Application.Services;
using PollingSurvey.Infrastructure.Repositories;
using FluentValidation;
using PollingSurvey.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// ✅ CHỈ REGISTER SQL SERVER KHI KHÔNG PHẢI TEST
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// ✅ Đăng ký toàn bộ Application layer
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IPollNotifier, SignalRPollNotifier>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreatePollRequestValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ✅ Phase 1 - Rate Limiting (global, per-client-IP, fixed window)
builder.Services.AddRateLimiter(options =>
{
    // Trả về 429 khi vượt giới hạn
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Áp dụng giới hạn cho toàn bộ API, partition theo IP của client
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PollHub>("/pollHub");

app.Run();

public partial class Program { }