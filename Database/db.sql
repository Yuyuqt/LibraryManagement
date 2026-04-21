-- Library Management System Database Schema (MSSQL)
CREATE DATABASE LibraryManagement;
GO

USE LibraryManagement;
GO

-- 1. Categories Table
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Categories_Name UNIQUE (Name)
);
GO

-- 2. Books Table
CREATE TABLE Books (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    ISBN NVARCHAR(20) NOT NULL,
    Author NVARCHAR(100) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT (GETDATE()),
    Description NVARCHAR(MAX) NULL,
    TotalCopies INT NOT NULL DEFAULT 1,
    AvailableCopies INT NOT NULL DEFAULT 1,
    UpdatedAt DATETIME NULL,
    CONSTRAINT UQ_Books_ISBN UNIQUE (ISBN)
);
GO

-- 3. Users Table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Member',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT (GETDATE()),
    UpdatedAt DATETIME NULL,
    StudentId NVARCHAR(20) NULL,
    BanStatus BIT NULL,
    SuspensionEndDate DATETIME NULL,
    Address NVARCHAR(255) NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

-- 4. Memberships Table
CREATE TABLE Memberships (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Type NVARCHAR(50) NOT NULL,
    MaxBooks INT NOT NULL,
    BorrowingDays INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    DurationMonths INT NOT NULL,
    LoyaltyRewardId NVARCHAR(50) NULL,
    CONSTRAINT IX_Memberships_Type UNIQUE (Type)
);
GO

-- 5. BookCategories (Many-to-Many Join Table)
CREATE TABLE BookCategories (
    BookId INT NOT NULL,
    CategoryId INT NOT NULL,
    CONSTRAINT PK_BookCategories PRIMARY KEY (BookId, CategoryId),
    CONSTRAINT FK_BookCategories_Books FOREIGN KEY (BookId) REFERENCES Books(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BookCategories_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE
);
GO

-- 6. Borrowings Table
CREATE TABLE Borrowings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    BorrowDate DATETIME NOT NULL DEFAULT (GETDATE()),
    DueDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    FineAmount DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    IsFinePaid BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Borrowings_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Borrowings_Books FOREIGN KEY (BookId) REFERENCES Books(Id)
);
GO

-- 7. UserSubscriptions Table
CREATE TABLE UserSubscriptions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    MembershipId INT NOT NULL,
    StartDate DATETIME NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_UserSubscriptions_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserSubscriptions_Memberships FOREIGN KEY (MembershipId) REFERENCES Memberships(Id)
);
GO

-- 8. Wishlists Table
CREATE TABLE Wishlists (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    CONSTRAINT FK_Wishlists_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Wishlists_Books FOREIGN KEY (BookId) REFERENCES Books(Id)
);
GO
