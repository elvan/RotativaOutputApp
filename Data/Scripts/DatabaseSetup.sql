-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ReportDB')
BEGIN
    CREATE DATABASE ReportDB;
END
GO

USE ReportDB;
GO

-- Create Reports table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reports')
BEGIN
    CREATE TABLE Reports (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(255) NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Date DATETIME NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END
GO

-- Seed data if the table is empty
IF NOT EXISTS (SELECT TOP 1 1 FROM Reports)
BEGIN
    INSERT INTO Reports (Name, Description, Amount, Date)
    VALUES 
        ('Laptop', 'Dell XPS 15', 1500.00, DATEADD(DAY, -10, GETDATE())),
        ('Mouse', 'Logitech MX Master', 99.99, DATEADD(DAY, -8, GETDATE())),
        ('Keyboard', 'Keychron K2', 89.99, DATEADD(DAY, -8, GETDATE())),
        ('Monitor', 'LG 27-inch 4K', 349.99, DATEADD(DAY, -5, GETDATE())),
        ('Headphones', 'Sony WH-1000XM4', 279.99, DATEADD(DAY, -2, GETDATE())),
        ('Docking Station', 'CalDigit TS4', 399.99, DATEADD(DAY, -15, GETDATE())),
        ('External SSD', 'Samsung T7', 129.99, DATEADD(DAY, -7, GETDATE())),
        ('Webcam', 'Logitech C920', 79.99, DATEADD(DAY, -3, GETDATE())),
        ('USB Hub', 'Anker 7-Port', 29.99, DATEADD(DAY, -1, GETDATE())),
        ('Wireless Charger', 'Belkin 3-in-1', 149.99, DATEADD(DAY, -4, GETDATE())),
        ('Tablet', 'iPad Pro 12.9"', 1099.99, DATEADD(DAY, -12, GETDATE())),
        ('Smartphone', 'Samsung Galaxy S22', 899.99, DATEADD(DAY, -18, GETDATE())),
        ('Printer', 'HP LaserJet Pro', 349.99, DATEADD(DAY, -20, GETDATE())),
        ('Router', 'TP-Link Archer AX6000', 249.99, DATEADD(DAY, -25, GETDATE())),
        ('Camera', 'Sony Alpha a7 III', 1999.99, DATEADD(DAY, -30, GETDATE())),
        ('Smartwatch', 'Apple Watch Series 7', 399.99, DATEADD(DAY, -22, GETDATE())),
        ('Gaming Console', 'PlayStation 5', 499.99, DATEADD(DAY, -45, GETDATE())),
        ('Desk Chair', 'Herman Miller Aeron', 1499.99, DATEADD(DAY, -60, GETDATE())),
        ('Standing Desk', 'Uplift V2', 699.99, DATEADD(DAY, -75, GETDATE())),
        ('Monitor Arm', 'Ergotron LX', 199.99, DATEADD(DAY, -14, GETDATE())),
        ('Microphone', 'Blue Yeti', 129.99, DATEADD(DAY, -9, GETDATE())),
        ('Network Switch', 'Cisco 8-Port', 89.99, DATEADD(DAY, -35, GETDATE())),
        ('UPS Battery Backup', 'APC 1500VA', 219.99, DATEADD(DAY, -40, GETDATE())),
        ('Drawing Tablet', 'Wacom Intuos Pro', 379.99, DATEADD(DAY, -15, GETDATE())),
        ('External GPU', 'Razer Core X', 399.99, DATEADD(DAY, -27, GETDATE())),
        ('Mechanical Keyboard', 'Das Keyboard 4', 169.99, DATEADD(DAY, -33, GETDATE())),
        ('NAS Device', 'Synology DS220+', 299.99, DATEADD(DAY, -48, GETDATE())),
        ('Portable Monitor', 'ASUS ZenScreen', 249.99, DATEADD(DAY, -21, GETDATE())),
        ('Wireless Earbuds', 'AirPods Pro', 249.99, DATEADD(DAY, -17, GETDATE())),
        ('Smart Speaker', 'Amazon Echo', 99.99, DATEADD(DAY, -19, GETDATE()));
END
GO
