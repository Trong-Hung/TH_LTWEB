// File: ~/Program.cs
using Microsoft.AspNetCore.Authorization; // Đảm bảo có
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc; // Đảm bảo có
using Microsoft.EntityFrameworkCore;
using VoTrongHung2280601119.Hubs;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;
using VoTrongHung2280601119.SeedData;
using VoTrongHung2280601119.Services;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Email
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IEmailSender, EmailService>();


// Cấu hình EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    // Cấu hình mật khẩu mạnh hơn nếu cần, ví dụ:
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 0;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cấu hình cookie xác thực
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// Razor Pages + Session
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Repository
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IWarehouseRepository, EFWarehouseRepository>();
builder.Services.AddScoped<IOrderDistributionRepository, EFOrderDistributionRepository>();
builder.Services.AddScoped<IOrderItemRepository, EFOrderItemRepository>();

// Momo
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

builder.Services.AddControllersWithViews();

// Cấu hình SignalR và Chat Service
builder.Services.AddSignalR();
// Đăng ký UserConnectionManager là Singleton vì nó quản lý trạng thái kết nối
builder.Services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
//UserManager thường đã được đăng ký bởi AddIdentity, không cần đăng ký lại

var app = builder.Build();

// Seed Roles + Admin
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await ApplicationDbInitializer.SeedRolesAndAdminUser(serviceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

// SỬA LỖI: Sửa lại quy tắc định tuyến cho khu vực Admin
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

//Thêm route thân thiện vào Program.cs
app.MapControllerRoute(
    name: "friendlyProduct",
    pattern: "{slug}-{id}.html",
    defaults: new { controller = "Product", action = "Display" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();