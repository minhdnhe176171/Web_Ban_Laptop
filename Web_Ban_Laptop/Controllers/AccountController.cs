using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Web_Ban_Laptop.Models;
using Web_Ban_Laptop.Models.ViewModels;
using Web_Ban_Laptop.Services;

namespace Web_Ban_Laptop.Controllers
{
    public class AccountController : Controller
    {
        private readonly LaptopStoreDbFinalContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(LaptopStoreDbFinalContext context, EmailService emailService, ILogger<AccountController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra email đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng sử dụng email khác hoặc đăng nhập.");
                    return View(model);
                }

                // Kiểm tra email đã được dùng làm username
                if (await _context.Users.AnyAsync(u => u.Username == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng sử dụng email khác hoặc đăng nhập.");
                    return View(model);
                }

                // Lấy Role Customer
                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
                if (customerRole == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy role Customer. Vui lòng liên hệ quản trị viên.");
                    _logger.LogError("Customer role not found in database");
                    return View(model);
                }

                // Tạo User mới - dùng Email làm Username
                var user = new User
                {
                    Username = model.Email, // Dùng email làm username
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Address = model.Address,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RoleId = customerRole.RoleId,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ New user registered: Email: {Email}", user.Email);

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập bằng email của bạn.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during user registration");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập, redirect về trang chủ
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ email và mật khẩu");
                return View();
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                ModelState.AddModelError("", "Email không hợp lệ");
                return View();
            }

            try
            {
                // Tìm user bằng Email (vì Username = Email)
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => (u.Email == email || u.Username == email) && u.IsActive == true);

                if (user == null)
                {
                    _logger.LogWarning("⚠️ Login attempt with non-existent email: {Email}", email);
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                    return View();
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("⚠️ Invalid password for email: {Email}", email);
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                    return View();
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    ModelState.AddModelError("", "Tài khoản chưa có email. Vui lòng liên hệ quản trị viên để cập nhật email.");
                    return View();
                }

                // Tạo mã OTP
                var otpCode = new Random().Next(100000, 999999).ToString();
                user.OtpCode = otpCode;
                user.OtpExpiry = DateTime.Now.AddMinutes(10);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("🔑 OTP generated for user: Email: {Email}, OTP: {OTP}", user.Email, otpCode);

                // Log OTP ra console ngay
                var separator = new string('=', 70);
                Console.WriteLine($"\n{separator}");
                Console.WriteLine($"🔑 LOGIN OTP - Email: {user.Email}");
                Console.WriteLine($"🔑 OTP CODE: {otpCode}");
                Console.WriteLine($"⏰ Expires at: {user.OtpExpiry:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"{separator}\n");

                // Gửi OTP qua email - ĐẢM BẢO GỬI VỀ ĐÚNG EMAIL
                var emailSent = await _emailService.SendOTPEmailAsync(user.Email, otpCode, "đăng nhập");
                
                if (emailSent)
                {
                    _logger.LogInformation("✅ OTP email sent successfully to {Email}", user.Email);
                    TempData["SuccessMessage"] = $"✅ Mã OTP đã được gửi đến email {MaskEmail(user.Email)}. Vui lòng kiểm tra email và nhập mã OTP.";
                }
                else
                {
                    _logger.LogWarning("⚠️ OTP email may not have been sent to {Email}. Check console/logs for OTP code.", user.Email);
                    TempData["ErrorMessage"] = $"⚠️ Không thể gửi email đến {MaskEmail(user.Email)}. Vui lòng kiểm tra Console/Logs để lấy mã OTP, hoặc cấu hình EmailSettings trong appsettings.json.";
                }

                // Lưu thông tin vào Session để giữ giữa các request
                HttpContext.Session.SetString("OTP_UserId", user.UserId.ToString());
                HttpContext.Session.SetString("OTP_Email", user.Email);
                HttpContext.Session.SetString("OTP_Username", user.Username);

                return RedirectToAction("VerifyOTP");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during login");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại sau.");
                return View();
            }
        }

        [HttpGet]
        public IActionResult VerifyOTP()
        {
            // Kiểm tra từ Session thay vì TempData
            var userId = HttpContext.Session.GetString("OTP_UserId");
            var email = HttpContext.Session.GetString("OTP_Email");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Phiên xác thực đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            ViewBag.UserId = int.Parse(userId);
            ViewBag.Username = email; // Hiển thị email
            
            // Log OTP từ database để dễ test
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
                if (user != null && !string.IsNullOrEmpty(user.OtpCode))
                {
                    Console.WriteLine($"\n🔑 Current OTP for {email}: {user.OtpCode}");
                    Console.WriteLine($"⏰ Expires at: {user.OtpExpiry:yyyy-MM-dd HH:mm:ss}\n");
                }
            }
            catch { }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(int userId, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(otpCode) || otpCode.Length != 6)
            {
                ModelState.AddModelError("", "Mã OTP phải có 6 chữ số");
                ViewBag.UserId = userId;
                ViewBag.Username = HttpContext.Session.GetString("OTP_Email");
                return View();
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    ModelState.AddModelError("", "Người dùng không tồn tại");
                    HttpContext.Session.Remove("OTP_UserId");
                    HttpContext.Session.Remove("OTP_Email");
                    HttpContext.Session.Remove("OTP_Username");
                    return RedirectToAction("Login");
                }

                if (string.IsNullOrEmpty(user.OtpCode))
                {
                    ModelState.AddModelError("", "Mã OTP không hợp lệ. Vui lòng đăng nhập lại.");
                    ViewBag.UserId = userId;
                    ViewBag.Username = user.Email;
                    return View();
                }

                _logger.LogInformation("🔍 Verifying OTP: User entered: {EnteredOTP}, Expected: {ExpectedOTP}", otpCode, user.OtpCode);

                if (user.OtpCode != otpCode)
                {
                    _logger.LogWarning("❌ Invalid OTP entered for user: {Email}", user.Email);
                    ModelState.AddModelError("", "Mã OTP không đúng. Vui lòng kiểm tra lại email hoặc Console/Logs.");
                    ViewBag.UserId = userId;
                    ViewBag.Username = user.Email;
                    return View();
                }

                if (user.OtpExpiry == null || user.OtpExpiry < DateTime.Now)
                {
                    _logger.LogWarning("⏰ Expired OTP used for user: {Email}", user.Email);
                    ModelState.AddModelError("", "Mã OTP đã hết hạn. Vui lòng đăng nhập lại để nhận mã mới.");
                    ViewBag.UserId = userId;
                    ViewBag.Username = user.Email;
                    return View();
                }

                // Xóa OTP sau khi xác thực thành công
                user.OtpCode = null;
                user.OtpExpiry = null;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Xóa Session OTP
                HttpContext.Session.Remove("OTP_UserId");
                HttpContext.Session.Remove("OTP_Email");
                HttpContext.Session.Remove("OTP_Username");

                // Lưu thông tin vào Session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("FullName", user.FullName ?? "");
                HttpContext.Session.SetString("RoleName", user.Role?.RoleName ?? "Customer");

                _logger.LogInformation("✅ User logged in successfully: Email: {Email}", user.Email);

                TempData["SuccessMessage"] = $"Đăng nhập thành công! Chào mừng {user.FullName ?? user.Email}.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during OTP verification");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình xác thực. Vui lòng thử lại.");
                ViewBag.UserId = userId;
                ViewBag.Username = HttpContext.Session.GetString("OTP_Email");
                return View();
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập email");
                return View();
            }

            if (!IsValidEmail(email))
            {
                ModelState.AddModelError("", "Email không hợp lệ");
                return View();
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => (u.Email == email || u.Username == email) && u.IsActive == true);

                if (user == null)
                {
                    // Không tiết lộ email có tồn tại hay không (bảo mật)
                    _logger.LogWarning("⚠️ Password reset requested for non-existent email: {Email}", email);
                    TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, mã OTP đã được gửi đến email của bạn.";
                    return RedirectToAction("ResetPassword");
                }

                // Tạo mã OTP
                var otpCode = new Random().Next(100000, 999999).ToString();
                user.OtpCode = otpCode;
                user.OtpExpiry = DateTime.Now.AddMinutes(10);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("🔑 Password reset OTP generated for user: Email: {Email}, OTP: {OTP}", user.Email, otpCode);

                // Log OTP ra console ngay
                var separator = new string('=', 70);
                Console.WriteLine($"\n{separator}");
                Console.WriteLine($"🔑 FORGOT PASSWORD OTP - Email: {user.Email}");
                Console.WriteLine($"🔑 OTP CODE: {otpCode}");
                Console.WriteLine($"⏰ Expires at: {user.OtpExpiry:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"{separator}\n");

                // Gửi OTP qua email - ĐẢM BẢO GỬI VỀ ĐÚNG EMAIL
                var emailSent = await _emailService.SendPasswordResetOTPEmailAsync(user.Email, otpCode);
                
                if (emailSent)
                {
                    _logger.LogInformation("✅ Password reset OTP email sent successfully to {Email}", user.Email);
                    TempData["SuccessMessage"] = $"✅ Mã OTP đã được gửi đến email {MaskEmail(user.Email)}. Vui lòng kiểm tra email và nhập mã OTP.";
                }
                else
                {
                    _logger.LogWarning("⚠️ Password reset OTP email may not have been sent to {Email}. Check console/logs for OTP code.", user.Email);
                    TempData["ErrorMessage"] = $"⚠️ Không thể gửi email đến {MaskEmail(user.Email)}. Vui lòng kiểm tra Console/Logs để lấy mã OTP, hoặc cấu hình EmailSettings trong appsettings.json.";
                }

                // Lưu thông tin vào Session
                HttpContext.Session.SetString("ResetPassword_UserId", user.UserId.ToString());
                HttpContext.Session.SetString("ResetPassword_Email", user.Email);

                return RedirectToAction("ResetPassword");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during forgot password");
                ModelState.AddModelError("", "Đã xảy ra lỗi. Vui lòng thử lại sau.");
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var userId = HttpContext.Session.GetString("ResetPassword_UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Phiên đặt lại mật khẩu đã hết hạn. Vui lòng thử lại.";
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.UserId = int.Parse(userId);
            
            // Log OTP từ database để dễ test
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
                if (user != null && !string.IsNullOrEmpty(user.OtpCode))
                {
                    Console.WriteLine($"\n🔑 Current OTP for password reset: {user.OtpCode}");
                    Console.WriteLine($"📧 Email: {user.Email}");
                    Console.WriteLine($"⏰ Expires at: {user.OtpExpiry:yyyy-MM-dd HH:mm:ss}\n");
                }
            }
            catch { }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userId, string otpCode, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(otpCode) || otpCode.Length != 6)
            {
                ModelState.AddModelError("", "Mã OTP phải có 6 chữ số");
                ViewBag.UserId = userId;
                return View();
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự");
                ViewBag.UserId = userId;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu không khớp");
                ViewBag.UserId = userId;
                return View();
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    ModelState.AddModelError("", "Người dùng không tồn tại");
                    HttpContext.Session.Remove("ResetPassword_UserId");
                    HttpContext.Session.Remove("ResetPassword_Email");
                    return RedirectToAction("ForgotPassword");
                }

                if (string.IsNullOrEmpty(user.OtpCode))
                {
                    ModelState.AddModelError("", "Mã OTP không hợp lệ. Vui lòng yêu cầu mã mới.");
                    ViewBag.UserId = userId;
                    return View();
                }

                _logger.LogInformation("🔍 Verifying password reset OTP: User entered: {EnteredOTP}, Expected: {ExpectedOTP}", otpCode, user.OtpCode);

                if (user.OtpCode != otpCode)
                {
                    _logger.LogWarning("❌ Invalid OTP entered for password reset: UserId {UserId}", userId);
                    ModelState.AddModelError("", "Mã OTP không đúng. Vui lòng kiểm tra lại email hoặc Console/Logs.");
                    ViewBag.UserId = userId;
                    return View();
                }

                if (user.OtpExpiry == null || user.OtpExpiry < DateTime.Now)
                {
                    _logger.LogWarning("⏰ Expired OTP used for password reset: UserId {UserId}", userId);
                    ModelState.AddModelError("", "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.");
                    ViewBag.UserId = userId;
                    return View();
                }

                // Cập nhật mật khẩu mới
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.OtpCode = null;
                user.OtpExpiry = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Xóa Session
                HttpContext.Session.Remove("ResetPassword_UserId");
                HttpContext.Session.Remove("ResetPassword_Email");

                _logger.LogInformation("✅ Password reset successfully for user: Email: {Email}", user.Email);

                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during password reset");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đặt lại mật khẩu. Vui lòng thử lại.");
                ViewBag.UserId = userId;
                return View();
            }
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Logout()
        {
            var email = HttpContext.Session.GetString("Email");
            var username = HttpContext.Session.GetString("Username");
            
            // Xóa tất cả session
            HttpContext.Session.Clear();
            
            _logger.LogInformation("👋 User logged out: Email: {Email}, Username: {Username}", email, username);
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        // Helper methods
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var username = parts[0];
            var domain = parts[1];

            if (username.Length <= 2)
                return $"{username[0]}***@{domain}";

            return $"{username.Substring(0, 2)}***@{domain}";
        }
    }
}
