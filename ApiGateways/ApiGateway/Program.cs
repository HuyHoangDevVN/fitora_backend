using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load cấu hình Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Đăng ký Ocelot **trước khi gọi builder.Build()**
builder.Services.AddOcelot(builder.Configuration);

// CORS hoặc các dịch vụ khác cũng đăng ký trước builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://192.168.161.84:5173")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build(); // Build sau khi đăng ký xong

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

// Dùng await cho UseOcelot vì nó trả về Task
await app.UseOcelot();

app.Run();
