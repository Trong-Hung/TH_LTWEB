// File: Areas/Identity/Pages/Account/Register.cshtml.cs (Đã sửa lỗi)
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; // QUAN TRỌNG: Cần using này
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
// Giả sử lớp SD của bạn nằm ở đây, nếu không hãy thêm using phù hợp
// using VoTrongHung2280601119.Utilities; 

namespace VoTrongHung2280601119.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender; // <-- 1. KHAI BÁO FIELD

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender) // <-- 2. INJECT VÀO CONSTRUCTOR
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender; // <-- 3. GÁN GIÁ TRỊ
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Họ tên")]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.FullName = Input.FullName;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Giữ lại logic gán Role mặc định của bạn
                    await _userManager.AddToRoleAsync(user, SD.Role_Customer);

                    // ===================================================================
                    // === BẮT ĐẦU ĐOẠN CODE GỬI EMAIL XÁC NHẬN ===
                    // ===================================================================
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _userManager.AddToRoleAsync(user, SD.Role_Customer);

                    await _emailSender.SendEmailAsync(Input.Email, "Xác nhận địa chỉ email của bạn",
                        $"Cảm ơn bạn đã đăng ký. Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhấn vào đây</a>.");

                    // === KẾT THÚC ĐOẠN CODE GỬI EMAIL XÁC NHẬN ===

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        // Nếu BẮT BUỘC xác thực, chuyển hướng đến trang thông báo
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        // Nếu KHÔNG bắt buộc, đăng nhập người dùng luôn
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try { return Activator.CreateInstance<ApplicationUser>(); }
            catch { throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " + $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " + $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml"); }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail) { throw new NotSupportedException("The default UI requires a user store with email support."); }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}