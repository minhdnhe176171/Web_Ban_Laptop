using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Web_Ban_Laptop.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, string otpCode = "")
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
            var smtpPortStr = emailSettings["SmtpPort"] ?? "587";
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];
            var senderName = emailSettings["SenderName"] ?? "Laptop Store";

            // Extract OTP từ body nếu chưa có
            if (string.IsNullOrEmpty(otpCode))
            {
                try
                {
                    var otpMatch = System.Text.RegularExpressions.Regex.Match(body, @"<div class='otp-code'>(\d{6})</div>");
                    if (otpMatch.Success)
                    {
                        otpCode = otpMatch.Groups[1].Value;
                    }
                }
                catch { }
            }

            // Log OTP ngay lập tức - LUÔN LUÔN LOG
            var separator = new string('=', 70);
            Console.WriteLine($"\n{separator}");
            Console.WriteLine($"📧 EMAIL OTP NOTIFICATION");
            Console.WriteLine($"{separator}");
            Console.WriteLine($"To: {toEmail}");
            Console.WriteLine($"Subject: {subject}");
            if (!string.IsNullOrEmpty(otpCode))
            {
                Console.WriteLine($"🔑 OTP CODE: {otpCode}");
            }
            Console.WriteLine($"{separator}\n");

            // Kiểm tra email settings - kiểm tra kỹ hơn
            bool isEmailConfigured = !string.IsNullOrWhiteSpace(senderEmail) && 
                                     !string.IsNullOrWhiteSpace(senderPassword) &&
                                     senderEmail != "your-email@gmail.com" && 
                                     senderPassword != "your-app-password" &&
                                     senderEmail.Contains("@") &&
                                     !senderEmail.Contains("your-email") &&
                                     !senderEmail.Contains("example") &&
                                     (senderEmail.Contains("gmail.com") || 
                                      senderEmail.Contains("outlook.com") || 
                                      senderEmail.Contains("yahoo.com") ||
                                      senderEmail.Contains("hotmail.com"));

            if (!isEmailConfigured)
            {
                _logger.LogWarning("⚠️ Email settings not configured properly.");
                _logger.LogWarning("⚠️ SenderEmail: {SenderEmail}", senderEmail ?? "NULL");
                _logger.LogWarning("⚠️ SenderPassword: {HasPassword}", string.IsNullOrEmpty(senderPassword) ? "NULL" : "SET (but may be placeholder)");
                _logger.LogInformation("📧 Email would be sent to: {Email}", toEmail);
                _logger.LogInformation("📧 Email Subject: {Subject}", subject);
                
                if (!string.IsNullOrEmpty(otpCode))
                {
                    var testSeparator = new string('=', 70);
                    Console.WriteLine($"\n{testSeparator}");
                    Console.WriteLine($"⚠️  EMAIL SETTINGS NOT CONFIGURED");
                    Console.WriteLine($"{testSeparator}");
                    Console.WriteLine($"🔑 OTP CODE FOR TESTING: {otpCode}");
                    Console.WriteLine($"📧 Email: {toEmail}");
                    Console.WriteLine($"");
                    Console.WriteLine($"ℹ️  Để gửi email tự động, vui lòng cấu hình EmailSettings trong appsettings.json:");
                    Console.WriteLine($"   1. Mở file appsettings.json");
                    Console.WriteLine($"   2. Cập nhật SenderEmail với email Gmail của bạn (ví dụ: myemail@gmail.com)");
                    Console.WriteLine($"   3. Tạo App Password trên Gmail và cập nhật SenderPassword");
                    Console.WriteLine($"   4. Xem file HUONG_DAN_CAU_HINH_EMAIL.md để biết chi tiết");
                    Console.WriteLine($"{testSeparator}\n");
                }
                
                return false; // Return false để biết email chưa được gửi
            }

            // Thử gửi email thật với retry mechanism
            int smtpPort = 587;
            if (!int.TryParse(smtpPortStr, out smtpPort))
            {
                smtpPort = 587;
            }

            _logger.LogInformation("📧 Attempting to send email to {Email} via {Server}:{Port}", toEmail, smtpServer, smtpPort);
            _logger.LogInformation("📧 From: {SenderEmail}", senderEmail);

            // Retry 3 lần nếu lần đầu fail
            int maxRetries = 3;
            Exception lastException = null;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(senderName, senderEmail));
                    message.To.Add(new MailboxAddress("", toEmail));
                    message.Subject = subject;
                    message.Body = new TextPart("html") { Text = body };

                    using var client = new SmtpClient();
                    
                    // Set timeout
                    client.Timeout = 30000; // 30 seconds
                    
                    _logger.LogInformation("📧 [Attempt {Attempt}/{MaxRetries}] Connecting to SMTP server {Server}:{Port}...", attempt, maxRetries, smtpServer, smtpPort);
                    
                    // Thử kết nối với StartTls
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    
                    _logger.LogInformation("📧 Connected. Authenticating with {SenderEmail}...", senderEmail);
                    
                    // Authenticate
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    
                    _logger.LogInformation("📧 Authenticated successfully. Sending email to {ToEmail}...", toEmail);
                    
                    // Send email
                    await client.SendAsync(message);
                    
                    _logger.LogInformation("📧 Email sent. Disconnecting...");
                    
                    await client.DisconnectAsync(true);

                    _logger.LogInformation("✅ Email sent successfully to {Email} (Attempt {Attempt})", toEmail, attempt);
                    Console.WriteLine($"\n✅ Email sent successfully to {toEmail}");
                    if (!string.IsNullOrEmpty(otpCode))
                    {
                        Console.WriteLine($"🔑 OTP CODE (sent via email): {otpCode}\n");
                    }
                    
                    return true; // Thành công
                }
                catch (SmtpCommandException smtpEx)
                {
                    lastException = smtpEx;
                    _logger.LogError(smtpEx, "❌ SMTP Command Error (Attempt {Attempt}/{MaxRetries}): {Message}", attempt, maxRetries, smtpEx.Message);
                    _logger.LogError("❌ Status Code: {StatusCode}", smtpEx.StatusCode);
                    
                    if (attempt == maxRetries)
                    {
                        Console.WriteLine($"\n❌ SMTP Error after {maxRetries} attempts: {smtpEx.Message}");
                        Console.WriteLine($"❌ Status Code: {smtpEx.StatusCode}");
                        if (!string.IsNullOrEmpty(otpCode))
                        {
                            Console.WriteLine($"🔑 OTP CODE (email failed): {otpCode}");
                            Console.WriteLine($"📧 Email: {toEmail}\n");
                        }
                        return false;
                    }
                    
                    // Đợi 2 giây trước khi retry
                    await Task.Delay(2000);
                }
                catch (SmtpProtocolException smtpEx)
                {
                    lastException = smtpEx;
                    _logger.LogError(smtpEx, "❌ SMTP Protocol Error (Attempt {Attempt}/{MaxRetries}): {Message}", attempt, maxRetries, smtpEx.Message);
                    
                    if (attempt == maxRetries)
                    {
                        Console.WriteLine($"\n❌ SMTP Protocol Error after {maxRetries} attempts: {smtpEx.Message}");
                        if (!string.IsNullOrEmpty(otpCode))
                        {
                            Console.WriteLine($"🔑 OTP CODE (email failed): {otpCode}");
                            Console.WriteLine($"📧 Email: {toEmail}\n");
                        }
                        return false;
                    }
                    
                    await Task.Delay(2000);
                }
                catch (AuthenticationException authEx)
                {
                    lastException = authEx;
                    _logger.LogError(authEx, "❌ Authentication failed (Attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                    
                    Console.WriteLine($"\n❌ Authentication failed!");
                    Console.WriteLine($"❌ Please check your email and app password in appsettings.json");
                    Console.WriteLine($"❌ SenderEmail: {senderEmail}");
                    Console.WriteLine($"❌ Error: {authEx.Message}");
                    
                    if (!string.IsNullOrEmpty(otpCode))
                    {
                        Console.WriteLine($"🔑 OTP CODE (auth failed): {otpCode}");
                        Console.WriteLine($"📧 Email: {toEmail}\n");
                    }
                    
                    return false; // Không retry nếu lỗi authentication
                }
                catch (Exception smtpEx)
                {
                    lastException = smtpEx;
                    _logger.LogError(smtpEx, "❌ Failed to send email (Attempt {Attempt}/{MaxRetries}): {Message}", attempt, maxRetries, smtpEx.Message);
                    
                    if (attempt == maxRetries)
                    {
                        Console.WriteLine($"\n❌ Email sending failed after {maxRetries} attempts!");
                        Console.WriteLine($"❌ Error Type: {smtpEx.GetType().Name}");
                        Console.WriteLine($"❌ Error Message: {smtpEx.Message}");
                        
                        if (smtpEx.InnerException != null)
                        {
                            Console.WriteLine($"❌ Inner Exception: {smtpEx.InnerException.Message}");
                        }
                        
                        if (!string.IsNullOrEmpty(otpCode))
                        {
                            Console.WriteLine($"🔑 OTP CODE (email failed): {otpCode}");
                            Console.WriteLine($"📧 Email: {toEmail}\n");
                        }
                        
                        return false;
                    }
                    
                    await Task.Delay(2000);
                }
            }

            // Nếu đến đây nghĩa là tất cả retry đều fail
            if (lastException != null)
            {
                _logger.LogError(lastException, "❌ All {MaxRetries} attempts failed", maxRetries);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Critical error in email service");
            Console.WriteLine($"\n❌ Critical error in email service!");
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            
            if (!string.IsNullOrEmpty(otpCode))
            {
                Console.WriteLine($"\n🔑 OTP CODE (extracted): {otpCode}");
                Console.WriteLine($"📧 Email: {toEmail}\n");
            }
            
            return false;
        }
    }

    public async Task<bool> SendOTPEmailAsync(string toEmail, string otpCode, string purpose = "đăng nhập")
    {
        var subject = $"Mã OTP {purpose} - Laptop Store";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 10px;
            padding: 30px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 28px;
            font-weight: bold;
            color: #2563eb;
            margin-bottom: 10px;
        }}
        .otp-box {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 8px;
            text-align: center;
            margin: 30px 0;
        }}
        .otp-code {{
            font-size: 48px;
            font-weight: bold;
            letter-spacing: 12px;
            margin: 20px 0;
            color: #ffffff;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
        }}
        .otp-label {{
            font-size: 16px;
            margin-bottom: 15px;
            opacity: 0.95;
        }}
        .otp-expiry {{
            font-size: 14px;
            margin-top: 15px;
            opacity: 0.9;
        }}
        .warning {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            text-align: center;
            color: #666;
            font-size: 12px;
        }}
        .copy-instruction {{
            background-color: #e7f3ff;
            border: 1px solid #2563eb;
            border-radius: 6px;
            padding: 15px;
            margin: 20px 0;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🖥️ LaptopStore</div>
            <h2 style='color: #2563eb; margin: 0;'>Mã OTP {purpose}</h2>
        </div>
        
        <p>Xin chào,</p>
        <p>Bạn đã yêu cầu mã OTP để <strong>{purpose}</strong> vào hệ thống Laptop Store.</p>
        
        <div class='otp-box'>
            <div class='otp-label'>Mã OTP của bạn:</div>
            <div class='otp-code'>{otpCode}</div>
            <div class='otp-expiry'>Mã này có hiệu lực trong <strong>10 phút</strong></div>
        </div>
        
        <div class='copy-instruction'>
            <strong>📋 Hướng dẫn:</strong><br>
            Copy mã OTP ở trên và dán vào form xác thực trên website.
        </div>
        
        <div class='warning'>
            <strong>⚠️ Lưu ý bảo mật:</strong>
            <ul style='margin: 10px 0 0 20px; padding: 0;'>
                <li>Không chia sẻ mã OTP với bất kỳ ai</li>
                <li>Mã OTP chỉ có hiệu lực trong 10 phút</li>
                <li>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này</li>
                <li>Mã OTP chỉ sử dụng được một lần</li>
            </ul>
        </div>
        
        <p>Nếu bạn gặp vấn đề, vui lòng liên hệ với chúng tôi qua email: <a href='mailto:support@laptopstore.com'>support@laptopstore.com</a></p>
        
        <div class='footer'>
            <p>Trân trọng,<br><strong>Đội ngũ Laptop Store</strong></p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, otpCode);
    }

    public async Task<bool> SendPasswordResetOTPEmailAsync(string toEmail, string otpCode)
    {
        return await SendOTPEmailAsync(toEmail, otpCode, "đặt lại mật khẩu");
    }
}
