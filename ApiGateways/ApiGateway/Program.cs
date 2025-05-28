using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load cấu hình Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);

// Configure CORS: Cho phép gửi cookies từ frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://192.168.161.84:5173",
            "https://fitora.aiotlab.edu.vn"
        )
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Thêm Swagger cho kiểm tra API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); // Build sau khi đăng ký xong

app.UseCors("AllowSpecificOrigin");

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

// Dùng await cho UseOcelot vì nó trả về Task
await app.UseOcelot();

// Thêm Swagger UI
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
