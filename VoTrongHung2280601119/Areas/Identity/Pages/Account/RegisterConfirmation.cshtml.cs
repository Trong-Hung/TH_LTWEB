    #nullable disable

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        public string Email { get; set; }
        public bool DisplayConfirmAccountLink { get; set; }
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            ReturnUrl = returnUrl;
            Email = email;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Không thể tải người dùng với email '{email}'.");
            }

            // Dòng này dùng để hiển thị link xác nhận trực tiếp trên trang
            // nếu bạn không có dịch vụ email thật. Nhưng vì bạn đã có, 
            // chúng ta có thể đặt nó thành false.
            DisplayConfirmAccountLink = false;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
            }

            return Page();
        }

        // Thêm thuộc tính này để giữ giá trị returnUrl
        public string ReturnUrl { get; set; }
    }
}