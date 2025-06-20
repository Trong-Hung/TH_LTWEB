using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.SeedData
{
    public static class ApplicationDbInitializer
    {
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Tạo Roles nếu chưa tồn tại
            string[] roleNames = { "ROLE_ADMIN", "ROLE_CUSTOMER" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tạo tài khoản Admin gốc nếu chưa tồn tại
            string adminEmail = "admin@dist.com";
            string adminPassword = "Admin@123"; // Mật khẩu đáp ứng yêu cầu Identity

            var user = await userManager.FindByEmailAsync(adminEmail);
            if (user == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin Hệ Thống"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "ROLE_ADMIN");
                }
                else
                {
                    // In lỗi nếu không tạo được user (hữu ích cho debug)
                    Console.WriteLine("Failed to create admin user in SeedData:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"- {error.Description}");
                    }
                }
            }
        }
    }
}