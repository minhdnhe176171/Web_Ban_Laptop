# 🔧 HƯỚNG DẪN CẤU HÌNH EMAIL ĐỂ GỬI OTP

## ⚠️ QUAN TRỌNG: Đọc kỹ hướng dẫn này để email hoạt động 100%

### 📋 Bước 1: Kiểm tra Console Output

**Trước khi cấu hình email, hãy test xem OTP có được tạo không:**

1. Chạy ứng dụng: `dotnet run`
2. Đăng nhập hoặc quên mật khẩu
3. Xem Console Output - bạn sẽ thấy:
   ```
   ======================================================================
   🔑 LOGIN OTP - Email: your-email@gmail.com
   🔑 OTP CODE: 123456
   ⏰ Expires at: 2025-01-XX XX:XX:XX
   ======================================================================
   ```
4. **Copy OTP từ Console để test ngay!**

---

### 📧 Bước 2: Cấu hình Gmail (Khuyến nghị)

#### 2.1. Bật Xác thực 2 lớp
1. Đăng nhập Gmail → [myaccount.google.com](https://myaccount.google.com)
2. Vào **Bảo mật** (Security)
3. Tìm **Xác minh 2 bước** (2-Step Verification)
4. Bật xác thực 2 lớp nếu chưa bật

#### 2.2. Tạo Mật khẩu ứng dụng (App Password)
1. Vào [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
2. Chọn **Ứng dụng**: Mail
3. Chọn **Thiết bị**: Windows Computer (hoặc Other nếu không có)
4. Nhấn **Tạo** (Generate)
5. **Copy mật khẩu 16 ký tự** (ví dụ: `abcd efgh ijkl mnop`)

#### 2.3. Cập nhật appsettings.json

Mở file `appsettings.json` và cập nhật:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-actual-email@gmail.com",
    "SenderPassword": "abcd efgh ijkl mnop",
    "SenderName": "Laptop Store"
  }
}
```

**Lưu ý:**
- `SenderEmail`: Email Gmail thật của bạn (ví dụ: `myemail@gmail.com`)
- `SenderPassword`: Mật khẩu ứng dụng 16 ký tự (bỏ khoảng trắng hoặc giữ nguyên đều được)
- **KHÔNG** dùng mật khẩu Gmail thông thường!

---

### 🔍 Bước 3: Kiểm tra Logs

Sau khi cấu hình, khi đăng nhập/quên mật khẩu, xem Console Output:

#### ✅ Thành công:
```
📧 Connecting to SMTP server smtp.gmail.com:587...
📧 Connected. Authenticating...
📧 Authenticated. Sending email...
📧 Email sent. Disconnecting...
✅ Email sent successfully to user@gmail.com
🔑 OTP CODE (sent via email): 123456
```

#### ❌ Lỗi Authentication:
```
❌ Authentication failed!
❌ Please check your email and app password in appsettings.json
🔑 OTP CODE (auth failed): 123456
```
→ **Giải pháp**: Kiểm tra lại App Password, đảm bảo đã bật 2-Step Verification

#### ❌ Lỗi Connection:
```
❌ SMTP Error: Connection timeout
🔑 OTP CODE (email failed): 123456
```
→ **Giải pháp**: Kiểm tra firewall, internet connection

---

### 🧪 Bước 4: Test Email

1. **Test với email của chính bạn trước**
2. Kiểm tra:
   - ✅ Hộp thư đến (Inbox)
   - ✅ Thư mục Spam/Junk
   - ✅ Promotions (nếu dùng Gmail)
3. Nếu không thấy email:
   - Xem Console để lấy OTP
   - Kiểm tra logs để xem lỗi cụ thể

---

### 🔧 Troubleshooting

#### Vấn đề 1: "Authentication failed"
**Nguyên nhân:**
- Chưa bật 2-Step Verification
- Dùng mật khẩu Gmail thông thường thay vì App Password
- App Password sai

**Giải pháp:**
1. Đảm bảo đã bật 2-Step Verification
2. Tạo App Password mới
3. Copy chính xác 16 ký tự (có thể bỏ khoảng trắng)

#### Vấn đề 2: "Connection timeout"
**Nguyên nhân:**
- Firewall chặn port 587
- Internet không ổn định
- SMTP server không khả dụng

**Giải pháp:**
1. Tắt firewall tạm thời để test
2. Kiểm tra internet connection
3. Thử port 465 với SSL (cần sửa code)

#### Vấn đề 3: Email vào Spam
**Giải pháp:**
- Kiểm tra thư mục Spam
- Đánh dấu "Not Spam"
- Thêm sender email vào danh sách liên hệ

#### Vấn đề 4: Không nhận được email
**Giải pháp:**
1. **Luôn kiểm tra Console Output trước** - OTP luôn được log ra đó
2. Kiểm tra logs để xem lỗi cụ thể
3. Test với email khác
4. Đảm bảo email đã được cấu hình đúng trong appsettings.json

---

### 📝 Lưu ý quan trọng

1. **OTP luôn được log ra Console** - dù email có gửi được hay không
2. **Không commit appsettings.json** có chứa mật khẩu thật lên Git
3. **Sử dụng User Secrets** cho production:
   ```bash
   dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
   dotnet user-secrets set "EmailSettings:SenderPassword" "your-app-password"
   ```
4. **App Password chỉ dùng cho ứng dụng này** - không dùng cho mục đích khác

---

### ✅ Checklist

Trước khi báo lỗi, hãy kiểm tra:

- [ ] Đã bật 2-Step Verification trên Gmail
- [ ] Đã tạo App Password (16 ký tự)
- [ ] Đã cập nhật appsettings.json với email và password thật
- [ ] Đã kiểm tra Console Output để lấy OTP
- [ ] Đã kiểm tra thư mục Spam
- [ ] Đã xem logs để biết lỗi cụ thể

---

### 🆘 Nếu vẫn không được

1. **Xem Console Output** - OTP luôn có ở đó
2. **Copy toàn bộ error message** từ Console
3. **Kiểm tra logs** trong Console
4. **Test với email khác** để loại trừ vấn đề từ phía email nhận

**Nhớ: OTP luôn được log ra Console, bạn có thể dùng OTP đó để test ngay!**

