-- =============================================================
-- PHẦN 0: LÀM SẠCH (XÓA DB CŨ ĐỂ TRÁNH LỖI TRÙNG LẶP)
-- =============================================================
USE master;
GO

-- Nếu Database đã tồn tại, đá tất cả kết nối ra và xóa nó đi
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'LaptopStoreDB_Final')
BEGIN
    ALTER DATABASE LaptopStoreDB_Final SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LaptopStoreDB_Final;
END
GO

-- Tạo lại Database mới
CREATE DATABASE LaptopStoreDB_Final;
GO

USE LaptopStoreDB_Final;
GO

-- =============================================================
-- PHẦN 1: TẠO BẢNG (CORE SYSTEM)
-- =============================================================

-- 1. Roles
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE, 
    Description NVARCHAR(200)
);
GO

-- 2. Users (Sửa lỗi: Phải tạo bảng này chuẩn trước thì các bảng khác mới tham chiếu được)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Email VARCHAR(100) UNIQUE,
    Phone VARCHAR(15),
    Address NVARCHAR(255),
    RoleID INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    AvatarURL VARCHAR(MAX),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

-- 3. Suppliers
CREATE TABLE Suppliers (
    SupplierID INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(100) NOT NULL,
    ContactName NVARCHAR(100),
    Phone VARCHAR(20),
    Email VARCHAR(100),
    Address NVARCHAR(255),
    IsActive BIT DEFAULT 1
);
GO

-- 4. Categories
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX)
);
GO

-- 5. Products
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(200) NOT NULL,
    ShortDescription NVARCHAR(500),
    Price DECIMAL(18, 2) NOT NULL,
    DiscountPrice DECIMAL(18, 2),
    Thumbnail VARCHAR(MAX),
    StockQuantity INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CategoryID INT NOT NULL,
    SupplierID INT,
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID)
);
GO

-- 6. ProductDetails
CREATE TABLE ProductDetails (
    DetailID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL UNIQUE,
    CPU NVARCHAR(100),       
    RAM NVARCHAR(100),       
    HardDrive NVARCHAR(100), 
    Screen NVARCHAR(100),    
    VGA NVARCHAR(100),       
    OS NVARCHAR(50),         
    Battery NVARCHAR(50),    
    Weight FLOAT,            
    Warranty NVARCHAR(50),   
    CONSTRAINT FK_ProductDetails_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- 7. ProductImages
CREATE TABLE ProductImages (
    ImageID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    ImageURL VARCHAR(MAX) NOT NULL,
    SortOrder INT DEFAULT 0,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- 8. ProductSerials
CREATE TABLE ProductSerials (
    SerialID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    SerialNumber VARCHAR(50) NOT NULL UNIQUE,
    Status INT DEFAULT 1, 
    CONSTRAINT FK_ProductSerials_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- 9. InventoryLogs
CREATE TABLE InventoryLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    ChangeAmount INT NOT NULL, 
    Reason NVARCHAR(255), 
    ActionDate DATETIME DEFAULT GETDATE(),
    UserID INT NULL,
    CONSTRAINT FK_InventoryLogs_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- 10. ImportOrders
CREATE TABLE ImportOrders (
    ImportID INT IDENTITY(1,1) PRIMARY KEY,
    SupplierID INT NOT NULL,
    ManagerID INT NOT NULL,
    ImportDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 2) DEFAULT 0,
    Note NVARCHAR(MAX),
    CONSTRAINT FK_Import_Supplier FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID),
    CONSTRAINT FK_Import_User FOREIGN KEY (ManagerID) REFERENCES Users(UserID)
);
GO

-- 11. ImportOrderDetails
CREATE TABLE ImportOrderDetails (
    ImportDetailID INT IDENTITY(1,1) PRIMARY KEY,
    ImportID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    ImportPrice DECIMAL(18, 2) NOT NULL,
    CONSTRAINT FK_ImportDetails_Import FOREIGN KEY (ImportID) REFERENCES ImportOrders(ImportID),
    CONSTRAINT FK_ImportDetails_Product FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- 12. Vouchers
CREATE TABLE Vouchers (
    VoucherID INT IDENTITY(1,1) PRIMARY KEY,
    Code VARCHAR(20) NOT NULL UNIQUE,
    DiscountType INT DEFAULT 1,
    DiscountValue DECIMAL(18, 2) NOT NULL, 
    MinOrderValue DECIMAL(18, 2) DEFAULT 0,
    StartDate DATETIME,
    EndDate DATETIME,
    UsageLimit INT DEFAULT 100,
    IsActive BIT DEFAULT 1
);
GO

-- 13. OrderStatuses
CREATE TABLE OrderStatuses (
    StatusID INT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL
);
GO

-- 14. Orders
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    StatusID INT NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    VoucherID INT NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Note NVARCHAR(MAX),
    PaymentMethod NVARCHAR(50) DEFAULT 'COD',
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Orders_Statuses FOREIGN KEY (StatusID) REFERENCES OrderStatuses(StatusID),
    CONSTRAINT FK_Orders_Vouchers FOREIGN KEY (VoucherID) REFERENCES Vouchers(VoucherID)
);
GO

-- 15. OrderDetails
CREATE TABLE OrderDetails (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Price DECIMAL(18, 2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- 16. Feedbacks
CREATE TABLE Feedbacks (
    FeedbackID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    ProductID INT NOT NULL,
    Rating INT CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(MAX),
    FeedbackDate DATETIME DEFAULT GETDATE(),
    Reply NVARCHAR(MAX),
    RepliedBy INT,
    RepliedDate DATETIME,
    CONSTRAINT FK_Feedbacks_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Feedbacks_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- 17. Wishlists
CREATE TABLE Wishlists (
    WishlistID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    ProductID INT NOT NULL,
    AddedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Wishlists_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Wishlists_Products FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    CONSTRAINT UK_Wishlist UNIQUE (UserID, ProductID)
);
GO

-- =============================================================
-- PHẦN 2: TRIGGERS
-- =============================================================

-- Trigger Import
CREATE TRIGGER trg_Import_UpdateStock
ON ImportOrderDetails
AFTER INSERT
AS
BEGIN
    UPDATE p
    SET p.StockQuantity = p.StockQuantity + i.Quantity
    FROM Products p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;

    INSERT INTO InventoryLogs (ProductID, ChangeAmount, Reason, ActionDate)
    SELECT ProductID, Quantity, CONCAT(N'Nhập kho phiếu #', ImportID), GETDATE()
    FROM inserted;
END;
GO

-- Trigger Order Insert
CREATE TRIGGER trg_Order_Insert_DeductStock
ON OrderDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET p.StockQuantity = p.StockQuantity - i.Quantity
    FROM Products p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;

    INSERT INTO InventoryLogs (ProductID, ChangeAmount, Reason, ActionDate)
    SELECT ProductID, -Quantity, CONCAT(N'Bán đơn hàng #', OrderID), GETDATE()
    FROM inserted;

    IF EXISTS (SELECT 1 FROM Products WHERE StockQuantity < 0)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50001, 'Lỗi: Hết hàng trong kho!', 1;
    END
END;
GO

-- Trigger Order Cancel
CREATE TRIGGER trg_Order_Cancel_ReturnStock
ON Orders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewStatus INT, @OldStatus INT, @OrderID INT;
    SELECT @NewStatus = StatusID, @OldStatus = (SELECT StatusID FROM deleted), @OrderID = OrderID FROM inserted;

    IF (@NewStatus = 5 AND @OldStatus != 5) 
    BEGIN
        UPDATE p
        SET p.StockQuantity = p.StockQuantity + od.Quantity
        FROM Products p
        INNER JOIN OrderDetails od ON p.ProductID = od.ProductID
        WHERE od.OrderID = @OrderID;

        INSERT INTO InventoryLogs (ProductID, ChangeAmount, Reason, ActionDate)
        SELECT ProductID, Quantity, CONCAT(N'Hoàn kho do hủy đơn #', @OrderID), GETDATE()
        FROM OrderDetails WHERE OrderID = @OrderID;
    END
END;
GO

-- =============================================================
-- PHẦN 3: DỮ LIỆU MẪU
-- =============================================================
INSERT INTO Roles VALUES ('Admin', 'Admin'), ('Manager', 'Quan ly'), ('Staff', 'Nhan vien'), ('Customer', 'Khach hang');
INSERT INTO OrderStatuses VALUES (N'Chờ xác nhận'), (N'Đang xử lý'), (N'Đang giao'), (N'Hoàn thành'), (N'Đã hủy');
INSERT INTO Categories VALUES (N'Gaming', N'Laptop choi game'), (N'Office', N'Laptop van phong');
INSERT INTO Suppliers VALUES (N'ASUS VN', N'Mr. A', '0123', 'asus@vn.com', N'HCM', 1);

INSERT INTO Users (Username, PasswordHash, FullName, Email, RoleID) 
VALUES ('admin', '123', 'Super Admin', 'admin@store.com', 1);

INSERT INTO Products (ProductName, Price, CategoryID, SupplierID, StockQuantity) 
VALUES (N'ASUS TUF Gaming F15', 20000000, 1, 1, 10);

INSERT INTO ProductDetails (ProductID, CPU, RAM, HardDrive, Screen, VGA)
VALUES (1, 'i5-11400H', '8GB', '512GB SSD', '15.6 144Hz', 'RTX 3050');

GO