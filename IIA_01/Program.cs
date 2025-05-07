using System.Net.WebSockets;
using IIA_01.HubSignalR;

using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Bỏ bớt log từ Microsoft
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.File(
        path: "Logs/log-.txt",               // thư mục Logs và tên file
        rollingInterval: RollingInterval.Day, // tạo file mới mỗi ngày
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
        rollOnFileSizeLimit: true,            // tự động chia file nếu quá giới hạn
        retainedFileCountLimit: 10,           // giữ tối đa 10 file log
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
    )
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<CameraReposotory>();
builder.Host.UseSerilog();
builder.Services.AddHostedService<BackgroundReportService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7126") // địa chỉ frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ⚠️ Quan trọng khi dùng SignalR
    });
});
builder.Services.AddSignalR();

var services = builder.Services;
// Add services to the container.
builder.Services.AddControllersWithViews();
// In ConfigureServices
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(360);  // Thời gian hết hạn sau 60 phút không hoạt động
    options.Cookie.HttpOnly = true;  // Cấu hình session chỉ có thể truy cập qua HTTP
    options.Cookie.IsEssential = true;  // Cấu hình session là cần thiết cho hoạt động của ứng dụng
});
services.AddDistributedMemoryCache(); // Or another session storage provider
var app = builder.Build();
app.UseWebSockets();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// In Configure
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws/camera")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            WebSocketManager.Clients.Add(socket);

            // Lắng nghe hoặc giữ kết nối
            var buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    WebSocketManager.Clients.Remove(socket);
                }
            }
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<LogHub>("/logHub");
app.Run();




