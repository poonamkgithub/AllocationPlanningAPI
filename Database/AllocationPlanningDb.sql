CREATE DATABASE AllocationPlanningDb;
GO
USE AllocationPlanningDb;
GO

CREATE TABLE StockBalances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(50) NOT NULL,
    LocationCode NVARCHAR(50) NOT NULL,
    OnHandQuantity INT NOT NULL CHECK (OnHandQuantity >= 0),
    ReservedQuantity INT NOT NULL CHECK (ReservedQuantity >= 0),
    SafetyStockQuantity INT NOT NULL DEFAULT 0 CHECK (SafetyStockQuantity >= 0),
    UpdatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_StockBalances_Product_Location UNIQUE(ProductCode, LocationCode),
    CONSTRAINT CK_StockBalances_Reserved CHECK (ReservedQuantity <= OnHandQuantity)
);

CREATE TABLE AllocationRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RequestNumber NVARCHAR(50) NOT NULL UNIQUE,
    ProductCode NVARCHAR(50) NOT NULL,
    RequestedQuantity INT NOT NULL CHECK (RequestedQuantity > 0),
    RequiredDate DATETIME2 NOT NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal',
    Status NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE StockAllocations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RequestNumber NVARCHAR(50) NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL,
    LocationCode NVARCHAR(50) NOT NULL,
    AllocatedQuantity INT NOT NULL CHECK (AllocatedQuantity > 0),
    AllocationDateUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Allocated',
    CONSTRAINT UQ_StockAllocations_Request_Product_Location UNIQUE(RequestNumber, ProductCode, LocationCode)
);

INSERT INTO StockBalances(ProductCode, LocationCode, OnHandQuantity, ReservedQuantity, SafetyStockQuantity)
VALUES
('SKU-1001', 'MUM-WH1', 100, 20, 10),
('SKU-1001', 'PUNE-DC', 60, 5, 5),
('SKU-2001', 'MUM-WH1', 25, 0, 5);

INSERT INTO AllocationRequests(RequestNumber, ProductCode, RequestedQuantity, RequiredDate, Priority, Status)
VALUES ('ALC-1001', 'SKU-1001', 80, DATEADD(day, 2, CAST(GETDATE() AS date)), 'High', 'Pending');
GO
