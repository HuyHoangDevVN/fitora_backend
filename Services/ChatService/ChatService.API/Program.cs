using System.Text;
using ChatService.API.Middleware;
using ChatService.Application;
using ChatService.Infrastructure;
using ChatService.Infrastructure.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration["ConnectionStrings:Redis"]!);

builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Ping every 15s
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30); // Timeout after 30s without ping
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var certPath = builder.Environment.IsDevelopment()
    ? builder.Configuration["CertificateSettings:DevPath"]
    : builder.Configuration["CertificateSettings:ProdPath"];

var certPassword = builder.Configuration["CertificateSettings:Password"];

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5008, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps(certPath!, certPassword);
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://fitora-api.aiotlab.edu.vn")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationAuthentication(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseMiddleware<HybridAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
// Map SignalR Hub
app.MapHub<ChatHub>("/hubs/chat");

app.MapControllers();
app.Run();