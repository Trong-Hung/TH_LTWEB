using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; // Cần cho IEmailSender
using Microsoft.EntityFrameworkCore;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Models; // Đảm bảo có using này cho MomoOptionModel
using VoTrongHung2280601119.Repositories;
using VoTrongHung2280601119.Repositories; // Đảm bảo có using này cho IMomoService
using VoTrongHung2280601119.SeedData; // Cần cho ApplicationDbInitializer
using VoTrongHung2280601119.Services; // Cần cho EmailSender (giả)

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Cấu hình ASP.NET Core Identity (tắt xác nhận email để đơn giản)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cấu hình cookie xác thực và đường dẫn truy cập bị từ chối
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages(); // Kích hoạt Razor Pages (cho Identity UI)

// CẤU HÌNH SESSION CHO GIỎ HÀNG
builder.Services.AddDistributedMemoryCache(); // Đăng ký một Distributed Cache (Memory Cache là đơn giản nhất)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian sống của session
    options.Cookie.HttpOnly = true; // Cookie không thể truy cập bởi client-side script
    options.Cookie.IsEssential = true; // Cookie là cần thiết cho ứng dụng
});


// Đăng ký Repository Pattern cho tất cả các Models
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IWarehouseRepository, EFWarehouseRepository>();
builder.Services.AddScoped<IOrderDistributionRepository, EFOrderDistributionRepository>();
builder.Services.AddScoped<IOrderItemRepository, EFOrderItemRepository>();


// Đăng ký EmailSender giả (để giải quyết lỗi IEmailSender)
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Momo API Payment - ĐÃ DI CHUYỂN PHẦN NÀY LÊN TRƯỚC app.Build()
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();


// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình xác thực
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; //Xác thực tùy chọn đăng nhập
    options.DefaultChallengeScheme = "Google"; //đăng nhập bằng Google
})
.AddCookie() //lưu phiên đăng nhập
.AddGoogle("Google", options => //cấu hình  đăng nhập với Google OAuth 2.0.
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    //lấy thông tin từ appsettings.json
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.SaveTokens = true;

    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.NameIdentifier, "sub");
    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Name, "name");
    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Email, "email");
});
var app = builder.Build();

// Seed Data: Chạy khi ứng dụng khởi động để tạo Roles và Admin gốc
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await ApplicationDbInitializer.SeedRolesAndAdminUser(serviceProvider);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles(); // Phục vụ các file tĩnh (CSS, JS, hình ảnh)

app.UseSession(); // Kích hoạt Session Middleware (phải trước UseRouting)

app.UseRouting();     // Định tuyến các request đến endpoint phù hợp

// Middleware xác thực và ủy quyền (phải đặt sau UseRouting)
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Mapping các Razor Pages của Identity UI

// Route cho Admin Area (Controller và Views của Admin sẽ nằm trong đây)
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Products}/{action=Index}/{id?}");

//Thêm route thân thiện vào Program.cs
app.MapControllerRoute(
    name: "friendlyProduct",
    pattern: "{slug}-{id}.html",
    defaults: new { controller = "Product", action = "Display" });

// Route mặc định cho Customer (Controller và Views của Customer sẽ nằm ở gốc)
// Trang chủ sẽ hiển thị danh sách sản phẩm cho Customer
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();