# Hướng dẫn cấu hình Email để gửi OTP

## Cấu hình Email Settings

Để hệ thống có thể gửi email OTP, bạn cần cấu hình trong file `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "Laptop Store"
  }
}
```

## Cấu hình Gmail

### Bước 1: Bật xác thực 2 lớp
1. Đăng nhập vào tài khoản Gmail của bạn
2. Vào **Quản lý Tài khoản Google** → **Bảo mật**
3. Bật **Xác minh 2 bước**

### Bước 2: Tạo Mật khẩu ứng dụng
1. Vào **Quản lý Tài khoản Google** → **Bảo mật**
2. Tìm mục **Mật khẩu ứng dụng** (App passwords)
3. Chọn **Ứng dụng**: Mail
4. Chọn **Thiết bị**: Windows Computer (hoặc Other)
5. Nhấn **Tạo**
6. Copy mật khẩu 16 ký tự được tạo

### Bước 3: Cập nhật appsettings.json
- `SenderEmail`: Email Gmail của bạn (ví dụ: `yourname@gmail.com`)
- `SenderPassword`: Mật khẩu ứng dụng 16 ký tự vừa tạo

## Cấu hình Email khác (Outlook, Yahoo, etc.)

### Outlook/Hotmail
```json
{
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587
}
```

### Yahoo Mail
```json
{
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": 587
}
```

## Chế độ Development

Nếu bạn chưa cấu hình email hoặc đang trong môi trường development:
- Hệ thống sẽ tự động log nội dung email ra Console
- OTP sẽ được hiển thị trong Console output
- Bạn có thể copy OTP từ Console để test

## Kiểm tra Email đã gửi

Sau khi cấu hình xong, khi đăng nhập hoặc quên mật khẩu:
1. Kiểm tra hộp thư đến (Inbox) của email đã đăng ký
2. Nếu không thấy, kiểm tra thư mục Spam/Junk
3. Email OTP có tiêu đề: "Mã OTP đăng nhập - Laptop Store" hoặc "Mã OTP đặt lại mật khẩu - Laptop Store"

## Troubleshooting

### Lỗi: "Authentication failed"
- Kiểm tra lại mật khẩu ứng dụng (App Password)
- Đảm bảo đã bật xác thực 2 lớp
- Không sử dụng mật khẩu Gmail thông thường

### Lỗi: "Connection timeout"
- Kiểm tra kết nối internet
- Kiểm tra firewall có chặn port 587 không
- Thử đổi sang port 465 với SSL

### Email không nhận được
- Kiểm tra thư mục Spam
- Kiểm tra email đã nhập đúng chưa
- Xem log trong Console để kiểm tra OTP

## Lưu ý bảo mật

⚠️ **QUAN TRỌNG**: 
- Không commit file `appsettings.json` có chứa mật khẩu thật lên Git
- Sử dụng User Secrets hoặc Environment Variables cho production
- Mật khẩu ứng dụng chỉ dùng cho ứng dụng này, không dùng cho mục đích khác

