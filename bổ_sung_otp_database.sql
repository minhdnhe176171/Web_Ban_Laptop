USE LaptopStoreDB_Final;
GO

-- Thêm cột lưu mã OTP và thời gian hết hạn OTP vào bảng Users
ALTER TABLE Users ADD OtpCode VARCHAR(10) NULL;
ALTER TABLE Users ADD OtpExpiry DATETIME NULL;
GO