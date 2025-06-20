using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VoTrongHung2280601119.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl = "/Product")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "Google");
        }
        //Challenge kích hoạt quá trình xác thực thông qua Google.
        //RedirectUri: Sau khi đăng nhập thành công, người dùng sẽ được chuyển đến /Product

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //xóa cookie xác thực
            return Redirect("/"); //trở về trang chủ
        }
        public IActionResult Profile()
        {
            var userName = User.Identity.Name;  // Tên người dùng
            var email = User.FindFirstValue(ClaimTypes.Email);  // Email người dùng
            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Google ID người dùng

            // Truyền thông tin người dùng vào View để hiển thị
            ViewBag.UserName = userName;
            ViewBag.Email = email;
            ViewBag.GoogleId = googleId;

            return View();
        }
    }
}
