using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using MMLib.SwaggerForOcelot;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Load cấu hình Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSignalR();

// Configure CORS: Cho phép gửi cookies từ frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
        [
            "http://localhost:5173",
            "http://192.168.161.84:5173",
            "https://fitora.fitdnu.id.vn"
        ];

        policy.WithOrigins(allowedOrigins)
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Thêm Swagger hợp nhất từ tất cả downstream service (MMLib.SwaggerForOcelot) —
// đọc cấu hình SwaggerEndPoints trong ocelot.json để gộp docs của 5 service vào 1 UI.
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build(); // Build sau khi đăng ký xong

app.UseForwardedHeaders();
app.UseCors("AllowSpecificOrigin");

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    if (context.Request.IsHttps)
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }
    await next();
});

app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.StatusCode = 204;
        context.Response.Headers["Access-Control-Allow-Origin"] = context.Request.Headers["Origin"];
        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
        context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
        context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
        await context.Response.CompleteAsync();
        return;
    }

    await next();
});

app.UseHttpsRedirection();
app.UseWebSockets();

// Swagger hợp nhất: UI tại /swagger, dropdown chọn giữa 5 service (auth/user/interact/chat/notification)
app.UseSwaggerForOcelotUI();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }

    await next();
});

// Dùng await cho UseOcelot vì nó trả về Task
await app.UseOcelot();

app.Run();
