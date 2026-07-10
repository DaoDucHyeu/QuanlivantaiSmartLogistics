using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Models;
using SmartLogistics.Models.ViewModels;
using SmartLogistics.Helpers;

namespace SmartLogistics.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _UserManager;
        private readonly SignInManager<AppUser> _SignInManager;
        private readonly IEmailSender _EmailSender;

        public AccountController(UserManager<AppUser> UserManager, SignInManager<AppUser> SignInManager, IEmailSender EmailSender)
        {
            _UserManager = UserManager;
            _SignInManager = SignInManager;
            _EmailSender = EmailSender;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Nâng cấp Multi-Tenant: Bất kỳ ai đăng ký mới Tự Động là Admin (Lập công ty mới)
                var user = new Admin
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    UserType = UserType.Admin
                };

                var result = await _UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Sinh mã OTP 6 số
                    var token = await _UserManager.GenerateTwoFactorTokenAsync(user, "Email");
                    
                    // Gửi email OTP
                    var subject = "Xác nhận đăng ký tài khoản SmartLogistics";
                    var message = $"Xin chào {user.FullName},<br><br>Mã xác nhận (OTP) của bạn là: <strong>{token}</strong><br><br>Mã này dùng để kích hoạt tài khoản của bạn.";
                    await _EmailSender.SendEmailAsync(user.Email, subject, message);

                    // Chuyển hướng sang trang xác nhận OTP
                    return RedirectToAction("VerifyOTP", new { email = user.Email });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/VerifyOTP
        [HttpGet]
        public IActionResult VerifyOTP(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Register");
            var model = new VerifyOTPViewModel { Email = email };
            return View(model);
        }

        // POST: /Account/VerifyOTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(VerifyOTPViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại.");
                return View(model);
            }

            var isValid = await _UserManager.VerifyTwoFactorTokenAsync(user, "Email", model.Code);
            if (isValid)
            {
                user.EmailConfirmed = true;
                await _UserManager.UpdateAsync(user);
                
                await _SignInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Admin"); // Đăng ký Admin xong chuyển vào Admin
            }

            ModelState.AddModelError("Code", "Mã OTP không chính xác hoặc đã hết hạn.");
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng nhập username hay email
                var user = await _UserManager.FindByNameAsync(model.UserNameOrEmail);
                if (user == null)
                {
                    user = await _UserManager.FindByEmailAsync(model.UserNameOrEmail);
                }

                if (user != null)
                {
                    var result = await _SignInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        
                        // Switch redirect based on user role
                        return user.UserType switch
                        {
                            UserType.Admin => RedirectToAction("Index", "Admin"),
                            UserType.DieuHanh => RedirectToAction("Index", "OrderManagement"),
                            UserType.KeToan => RedirectToAction("Index", "Accounting"),
                            UserType.TaiXe => RedirectToAction("Index", "Driver"),
                            _ => RedirectToAction("Index", "Home")
                        };
                    }

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _SignInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Role = user.UserType.ToString()
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            
            var result = await _UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Role = user.UserType.ToString();
            return View(model);
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _UserManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                // Tạo mật khẩu ngẫu nhiên
                string newPassword = "Smart" + new Random().Next(10000, 99999).ToString() + "@"; 
                
                // Reset mật khẩu ngay lập tức bằng Token
                var token = await _UserManager.GeneratePasswordResetTokenAsync(user);
                var result = await _UserManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    var subject = "Cấp lại mật khẩu mới - SmartLogistics";
                    var message = $"Xin chào {user.FullName},<br><br>Hệ thống đã tự động cấp lại mật khẩu mới cho tài khoản của bạn.<br><br>Mật khẩu mới của bạn là: <strong style='font-size:20px; color:red;'>{newPassword}</strong><br><br>Vui lòng dùng mật khẩu này để đăng nhập và tiến hành đổi lại mật khẩu trong phần Profile để đảm bảo an toàn.";
                    await _EmailSender.SendEmailAsync(model.Email, subject, message);
                }

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return BadRequest("Invalid password reset token.");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("Login");
            }

            var result = await _UserManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Mật khẩu của bạn đã được đặt lại thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
