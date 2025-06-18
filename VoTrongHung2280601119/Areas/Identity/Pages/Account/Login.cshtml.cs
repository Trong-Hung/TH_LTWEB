// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Ghi nhớ tôi")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    if (user != null && await _userManager.IsInRoleAsync(user, SD.Role_Admin))
                    {
                        return RedirectToRoute("Admin", new { controller = "Products", action = "Index", area = "Admin" });
                    }
                    else
                    {
                        return LocalRedirect(returnUrl);
                    }
                }
                if (result.RequiresTwoFactor) { return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe }); }
                if (result.IsLockedOut) { _logger.LogWarning("User account locked out."); return RedirectToPage("./Lockout"); }
                else { ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Vui lòng kiểm tra lại Email và Mật khẩu."); return Page(); }
            }
            return Page();
        }
    }
}