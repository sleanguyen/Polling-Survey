using Microsoft.EntityFrameworkCore;
using PollingSurvey.Infrastructure.Data;
using PollingSurvey.API.Hubs;
using PollingSurvey.API.Realtime;
using PollingSurvey.Application.Interfaces;
using PollingSurvey.Application.Repositories;
using PollingSurvey.Application.Services;
using PollingSurvey.Infrastructure.Repositories;

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

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.MapHub<PollHub>("/pollHub");

app.Run();

public partial class Program { }