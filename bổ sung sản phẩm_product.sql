USE LaptopStoreDB_Final;
GO

-- ============================================================
-- BƯỚC 1: BỔ SUNG DANH MỤC & NHÀ CUNG CẤP (ĐỂ KHÔNG BỊ LỖI KHÓA NGOẠI)
-- ============================================================

-- Thêm thêm danh mục nếu chưa đủ
INSERT INTO Categories (CategoryName, Description) VALUES 
(N'MacBook', N'Các dòng máy tính Apple'),
(N'Workstation', N'Máy trạm đồ họa chuyên nghiệp'),
(N'Ultrabook', N'Máy mỏng nhẹ cao cấp');

-- Thêm nhà cung cấp phổ biến
INSERT INTO Suppliers (CompanyName, ContactName, Phone, Email, Address) VALUES 
(N'Dell Vietnam', N'Mr. John', '0909111222', 'dell@vn.com', N'Hà Nội'),
(N'HP Vietnam', N'Ms. Sara', '0909333444', 'hp@vn.com', N'TP.HCM'),
(N'Apple Dist.', N'Mr. Cook', '0909555666', 'apple@vn.com', N'Singapore'),
(N'MSI Gaming', N'Mr. Dragon', '0909777888', 'msi@vn.com', N'Đài Loan'),
(N'Lenovo VN', N'Mr. Think', '0909999000', 'lenovo@vn.com', N'Hà Nội');
GO

-- ============================================================
-- BƯỚC 2: THÊM 15 SẢN PHẨM (DÙNG BIẾN ĐỂ TỰ LIÊN KẾT)
-- ============================================================

DECLARE @NewProductID INT; -- Biến lưu ID vừa tạo

-- 1. Dell XPS 13 Plus (Ultrabook)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'Dell XPS 13 Plus 9320', N'Thiết kế tương lai, màn hình OLED 3.5K', 45000000, 42990000, '/images/products/dell-xps-13.jpg', 15, 4, 2);
SET @NewProductID = SCOPE_IDENTITY(); -- Lấy ID vừa sinh ra
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i7-1260P', '16GB LPDDR5', '512GB SSD', '13.4 OLED 3.5K', 'Intel Iris Xe', 'Windows 11 Home', '55Wh', 1.23, N'12 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/dell-xps-13-1.jpg'), (@NewProductID, '/images/products/dell-xps-13-2.jpg');

-- 2. MacBook Air M2 (MacBook)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'MacBook Air M2 2022', N'Siêu mỏng nhẹ, Chip M2 mạnh mẽ', 27000000, 26500000, '/images/products/macbook-air-m2.jpg', 20, 3, 4);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Apple M2 8-core', '8GB', '256GB SSD', '13.6 Liquid Retina', '8-core GPU', 'macOS', '52.6Wh', 1.24, N'12 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/mac-air-m2-1.jpg');

-- 3. HP Spectre x360 (Ultrabook)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'HP Spectre x360 14', N'Xoay gập 360 độ, màn hình cảm ứng', 38000000, NULL, '/images/products/hp-spectre.jpg', 8, 4, 3);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i7-1355U', '16GB', '1TB SSD', '13.5 3K2K OLED', 'Intel Iris Xe', 'Windows 11 Pro', '66Wh', 1.36, N'24 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/hp-spectre-1.jpg');

-- 4. ASUS ROG Strix G16 (Gaming)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'ASUS ROG Strix G16', N'Chiến thần Gaming, màn hình 165Hz', 32000000, 30990000, '/images/products/rog-strix-g16.jpg', 12, 1, 1);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i7-13650HX', '16GB DDR5', '512GB SSD', '16 inch FHD+ 165Hz', 'RTX 4060 8GB', 'Windows 11 Home', '90Wh', 2.50, N'24 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/rog-strix-1.jpg');

-- 5. MSI Raider GE78 (Gaming)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'MSI Raider GE78 HX', N'Hiệu năng tối thượng, dải led Matrix', 65000000, 59990000, '/images/products/msi-raider.jpg', 5, 1, 5);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i9-13980HX', '32GB DDR5', '2TB SSD', '17 inch QHD+ 240Hz', 'RTX 4080 12GB', 'Windows 11 Pro', '99Wh', 3.10, N'24 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/msi-raider-1.jpg');

-- 6. Lenovo ThinkPad X1 Carbon (Office)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'Lenovo ThinkPad X1 Carbon Gen 10', N'Biểu tượng doanh nhân, siêu bền bỉ', 41000000, NULL, '/images/products/thinkpad-x1.jpg', 10, 2, 6);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i7-1260P', '16GB', '512GB SSD', '14 inch IPS Anti-glare', 'Intel Iris Xe', 'Windows 11 Pro', '57Wh', 1.12, N'36 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/thinkpad-x1-1.jpg');

-- 7. Acer Nitro 5 Tiger (Gaming)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'Acer Nitro 5 Tiger', N'Laptop Gaming quốc dân, tản nhiệt tốt', 21000000, 19990000, '/images/products/acer-nitro-5.jpg', 25, 1, 1); -- Giả sử Acer chung NPP với Asus
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i5-12500H', '8GB', '512GB SSD', '15.6 FHD 144Hz', 'RTX 3050 Ti', 'Windows 11 Home', '57Wh', 2.50, N'12 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/acer-nitro-1.jpg');

-- 8. MacBook Pro 14 M2 Pro (MacBook)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'MacBook Pro 14 inch M2 Pro', N'Dành cho dân đồ họa, dựng phim chuyên nghiệp', 47000000, NULL, '/images/products/macbook-pro-14.jpg', 15, 3, 4);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Apple M2 Pro 10-core', '16GB', '512GB SSD', '14.2 Liquid Retina XDR', '16-core GPU', 'macOS', '70Wh', 1.60, N'12 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/mac-pro-14-1.jpg');

-- 9. Dell Alienware M15 R7 (Gaming)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'Dell Alienware M15 R7', N'Thiết kế phi thuyền, tản nhiệt Cryo-tech', 52000000, NULL, '/images/products/alienware-m15.jpg', 4, 1, 2);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Ryzen 7 6800H', '16GB DDR5', '1TB SSD', '15.6 QHD 240Hz', 'RTX 3070 Ti', 'Windows 11 Home', '86Wh', 2.69, N'12 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/alienware-1.jpg');

-- 10. Asus Zenbook 14 OLED (Office)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'Asus Zenbook 14 OLED', N'Mỏng nhẹ thời trang, màn OLED rực rỡ', 24000000, 22990000, '/images/products/zenbook-14.jpg', 18, 2, 1);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i5-1240P', '8GB', '512GB SSD', '14 inch 2.8K OLED', 'Intel Iris Xe', 'Windows 11 Home', '75Wh', 1.39, N'24 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/zenbook-1.jpg');

-- 11. LG Gram 16 (Ultrabook)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'LG Gram 16 2023', N'Nhẹ vô địch thế giới, pin trâu', 35000000, NULL, '/images/products/lg-gram-16.jpg', 7, 4, 2);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i7-1360P', '16GB', '512GB SSD', '16 inch WQXGA', 'Intel Iris Xe', 'Windows 11 Home', '80Wh', 1.19, N'12 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/lg-gram-1.jpg');

-- 12. HP Victus 16 (Gaming)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'HP Victus 16', N'Thiết kế đơn giản, hiệu năng mạnh mẽ', 19000000, 18500000, '/images/products/hp-victus.jpg', 30, 1, 3);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i5-12450H', '16GB', '512GB SSD', '16.1 FHD 144Hz', 'GTX 1650', 'Windows 11 Home', '70Wh', 2.46, N'12 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/victus-1.jpg');

-- 13. Lenovo Legion 5 Pro (Gaming)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'Lenovo Legion 5 Pro', N'Màn hình 16 inch 2K, tản nhiệt tốt nhất phân khúc', 36000000, 34990000, '/images/products/legion-5-pro.jpg', 15, 1, 6);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Ryzen 7 7745HX', '16GB DDR5', '1TB SSD', '16 WQXGA 240Hz', 'RTX 4060', 'Windows 11 Home', '80Wh', 2.50, N'24 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/legion-1.jpg');

-- 14. Surface Laptop 5 (Office)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'Surface Laptop 5 13.5"', N'Sang trọng, màn hình cảm ứng PixelSense', 29000000, NULL, '/images/products/surface-5.jpg', 10, 2, 2);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i5-1235U', '8GB', '256GB SSD', '13.5 PixelSense', 'Intel Iris Xe', 'Windows 11 Home', '47Wh', 1.29, N'12 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/surface-1.jpg');

-- 15. Dell Precision 5570 (Workstation)
INSERT INTO Products (ProductName, ShortDescription, Price, DiscountPrice, Thumbnail, StockQuantity, CategoryID, SupplierID)
VALUES (N'Dell Precision 5570', N'Máy trạm di động mỏng nhẹ nhất', 55000000, NULL, '/images/products/precision-5570.jpg', 3, 5, 2);
SET @NewProductID = SCOPE_IDENTITY();
INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA, OS, Battery, Weight, Warranty)
VALUES (@NewProductID, 'Core i7-12800H', '32GB DDR5', '1TB SSD', '15.6 UHD+ Touch', 'RTX A2000 8GB', 'Windows 11 Pro', '86Wh', 1.84, N'36 Tháng');
INSERT INTO ProductImages (ProductID, ImageURL) VALUES (@NewProductID, '/images/products/precision-1.jpg');

GO