-- =============================================
-- CMCS Database Setup Script
-- Student: ST10442156
-- Run this script to set up the database locally
-- =============================================

-- Create and use database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CMCS_Database')
BEGIN
    CREATE DATABASE CMCS_Database;
END
GO

USE CMCS_Database;
GO

-- Drop existing tables if they exist (clean slate)
IF OBJECT_ID('AspNetUserTokens', 'U') IS NOT NULL DROP TABLE AspNetUserTokens;
IF OBJECT_ID('AspNetUserRoles', 'U') IS NOT NULL DROP TABLE AspNetUserRoles;
IF OBJECT_ID('AspNetUserLogins', 'U') IS NOT NULL DROP TABLE AspNetUserLogins;
IF OBJECT_ID('AspNetUserClaims', 'U') IS NOT NULL DROP TABLE AspNetUserClaims;
IF OBJECT_ID('AspNetRoleClaims', 'U') IS NOT NULL DROP TABLE AspNetRoleClaims;
IF OBJECT_ID('Approvals', 'U') IS NOT NULL DROP TABLE Approvals;
IF OBJECT_ID('Documents', 'U') IS NOT NULL DROP TABLE Documents;
IF OBJECT_ID('ClaimLines', 'U') IS NOT NULL DROP TABLE ClaimLines;
IF OBJECT_ID('Claims', 'U') IS NOT NULL DROP TABLE Claims;
IF OBJECT_ID('AspNetUsers', 'U') IS NOT NULL DROP TABLE AspNetUsers;
IF OBJECT_ID('Lecturers', 'U') IS NOT NULL DROP TABLE Lecturers;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('AspNetRoles', 'U') IS NOT NULL DROP TABLE AspNetRoles;
GO

-- Create Lecturers table
CREATE TABLE Lecturers (
    Id int IDENTITY(1,1) PRIMARY KEY,
    FullName nvarchar(200) NOT NULL,
    Email nvarchar(200) NOT NULL,
    PhoneNumber nvarchar(50) NULL,
    Department nvarchar(100) NULL,
    EmployeeNumber nvarchar(50) NULL,
    BankName nvarchar(100) NULL,
    AccountNumber nvarchar(50) NULL,
    TaxNumber nvarchar(50) NULL,
    DefaultHourlyRate decimal(18,2) NOT NULL,
    IsActive bit NOT NULL,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NOT NULL,
    CONSTRAINT UQ_Lecturers_Email UNIQUE (Email),
    CONSTRAINT UQ_Lecturers_EmployeeNumber UNIQUE (EmployeeNumber)
);
GO

-- Create Users table (for old User model)
CREATE TABLE Users (
    Id int IDENTITY(1,1) PRIMARY KEY,
    FullName nvarchar(200) NOT NULL,
    Email nvarchar(200) NOT NULL,
    Role int NOT NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

-- Create Identity tables
CREATE TABLE AspNetRoles (
    Id nvarchar(450) PRIMARY KEY,
    Name nvarchar(256) NULL,
    NormalizedName nvarchar(256) NULL,
    ConcurrencyStamp nvarchar(max) NULL
);
GO

CREATE TABLE AspNetUsers (
    Id nvarchar(450) PRIMARY KEY,
    FullName nvarchar(200) NOT NULL,
    LecturerId int NULL,
    CreatedAt datetime2 NOT NULL,
    UserName nvarchar(256) NULL,
    NormalizedUserName nvarchar(256) NULL,
    Email nvarchar(256) NULL,
    NormalizedEmail nvarchar(256) NULL,
    EmailConfirmed bit NOT NULL,
    PasswordHash nvarchar(max) NULL,
    SecurityStamp nvarchar(max) NULL,
    ConcurrencyStamp nvarchar(max) NULL,
    PhoneNumber nvarchar(max) NULL,
    PhoneNumberConfirmed bit NOT NULL,
    TwoFactorEnabled bit NOT NULL,
    LockoutEnd datetimeoffset NULL,
    LockoutEnabled bit NOT NULL,
    AccessFailedCount int NOT NULL,
    CONSTRAINT FK_AspNetUsers_Lecturers FOREIGN KEY (LecturerId) REFERENCES Lecturers(Id) ON DELETE SET NULL
);
GO

CREATE TABLE AspNetRoleClaims (
    Id int IDENTITY(1,1) PRIMARY KEY,
    RoleId nvarchar(450) NOT NULL,
    ClaimType nvarchar(max) NULL,
    ClaimValue nvarchar(max) NULL,
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserClaims (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(450) NOT NULL,
    ClaimType nvarchar(max) NULL,
    ClaimValue nvarchar(max) NULL,
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserLogins (
    LoginProvider nvarchar(450) NOT NULL,
    ProviderKey nvarchar(450) NOT NULL,
    ProviderDisplayName nvarchar(max) NULL,
    UserId nvarchar(450) NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserRoles (
    UserId nvarchar(450) NOT NULL,
    RoleId nvarchar(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserTokens (
    UserId nvarchar(450) NOT NULL,
    LoginProvider nvarchar(450) NOT NULL,
    Name nvarchar(450) NOT NULL,
    Value nvarchar(max) NULL,
    PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- Create Claims table
CREATE TABLE Claims (
    Id int IDENTITY(1,1) PRIMARY KEY,
    LecturerId int NOT NULL,
    Month int NOT NULL,
    Year int NOT NULL,
    HourlyRate decimal(18,2) NOT NULL,
    TotalHours decimal(18,2) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    Status int NOT NULL,
    AdditionalNotes nvarchar(max) NULL,
    SubmittedDate datetime2 NOT NULL,
    CONSTRAINT FK_Claims_Lecturers FOREIGN KEY (LecturerId) REFERENCES Lecturers(Id) ON DELETE NO ACTION
);
GO

-- Create ClaimLines table
CREATE TABLE ClaimLines (
    Id int IDENTITY(1,1) PRIMARY KEY,
    ClaimId int NOT NULL,
    ActivityDescription nvarchar(500) NOT NULL,
    Hours decimal(18,2) NOT NULL,
    CONSTRAINT FK_ClaimLines_Claims FOREIGN KEY (ClaimId) REFERENCES Claims(Id) ON DELETE CASCADE
);
GO

-- Create Documents table
CREATE TABLE Documents (
    Id int IDENTITY(1,1) PRIMARY KEY,
    ClaimId int NOT NULL,
    FileName nvarchar(255) NOT NULL,
    FilePath nvarchar(1000) NOT NULL,
    FileSize bigint NOT NULL,
    UploadedDate datetime2 NOT NULL,
    CONSTRAINT FK_Documents_Claims FOREIGN KEY (ClaimId) REFERENCES Claims(Id) ON DELETE CASCADE
);
GO

-- Create Approvals table
CREATE TABLE Approvals (
    Id int IDENTITY(1,1) PRIMARY KEY,
    ClaimId int NOT NULL,
    ApproverId int NOT NULL,
    Decision nvarchar(50) NOT NULL,
    Comment nvarchar(1000) NULL,
    ApprovedDate datetime2 NOT NULL,
    CONSTRAINT FK_Approvals_Claims FOREIGN KEY (ClaimId) REFERENCES Claims(Id) ON DELETE CASCADE
);
GO

-- Create indexes for performance
CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims(RoleId);
CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;
CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims(UserId);
CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins(UserId);
CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles(RoleId);
CREATE INDEX EmailIndex ON AspNetUsers(NormalizedEmail);
CREATE INDEX IX_AspNetUsers_LecturerId ON AspNetUsers(LecturerId);
CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
GO

-- Insert migration history
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE __EFMigrationsHistory (
        MigrationId nvarchar(150) PRIMARY KEY,
        ProductVersion nvarchar(32) NOT NULL
    );
END
GO

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES 
('20251118162100_InitialCreate', '8.0.11'),
('20251118180100_AddIdentity', '8.0.11');
GO

-- Seed Lecturers
SET IDENTITY_INSERT Lecturers ON;
INSERT INTO Lecturers (Id, FullName, Email, PhoneNumber, Department, EmployeeNumber, BankName, AccountNumber, TaxNumber, DefaultHourlyRate, IsActive, CreatedAt, UpdatedAt) VALUES
(1, 'Dr. Sarah Johnson', 'sarah.johnson@university.ac.za', '011-234-5678', 'Computer Science', 'EMP001', 'First National Bank', '62********89', '9*******3', 500.00, 1, GETUTCDATE(), GETUTCDATE()),
(2, 'Prof. Michael Chen', 'michael.chen@university.ac.za', '011-234-5679', 'Information Technology', 'EMP002', 'Standard Bank', '25********67', '8*******2', 550.00, 1, GETUTCDATE(), GETUTCDATE()),
(3, 'Dr. Thandi Nkosi', 'thandi.nkosi@university.ac.za', '011-234-5680', 'Software Engineering', 'EMP003', 'ABSA', '40********12', '7*******5', 450.00, 1, GETUTCDATE(), GETUTCDATE()),
(4, 'Prof. James Anderson', 'james.anderson@university.ac.za', '011-234-5681', 'Data Science', 'EMP004', 'Nedbank', '18********34', '6*******8', 600.00, 1, GETUTCDATE(), GETUTCDATE()),
(5, 'Dr. Lerato Molefe', 'lerato.molefe@university.ac.za', '011-234-5682', 'Information Systems', 'EMP005', 'Capitec', '14********56', '5*******1', 480.00, 1, GETUTCDATE(), GETUTCDATE()),
(6, 'Prof. David Williams', 'david.williams@university.ac.za', '011-234-5683', 'Computer Science', 'EMP006', 'First National Bank', '62********91', '4*******9', 700.00, 1, GETUTCDATE(), GETUTCDATE()),
(7, 'Dr. Nomsa Dlamini', 'nomsa.dlamini@university.ac.za', '011-234-5684', 'Cybersecurity', 'EMP007', 'ABSA', '40********78', '3*******4', 650.00, 1, GETUTCDATE(), GETUTCDATE()),
(8, 'Dr. Kevin Patel', 'kevin.patel@university.ac.za', '011-234-5685', 'Software Engineering', 'EMP008', 'Standard Bank', '25********45', '2*******7', 520.00, 1, GETUTCDATE(), GETUTCDATE()),
(9, 'Prof. Amanda Brown', 'amanda.brown@university.ac.za', '011-234-5686', 'Web Development', 'EMP009', 'Nedbank', '18********92', '1*******6', 490.00, 1, GETUTCDATE(), GETUTCDATE()),
(10, 'Dr. Sipho Khumalo', 'sipho.khumalo@university.ac.za', '011-234-5687', 'Database Management', 'EMP010', 'Capitec', '14********23', '9*******0', 530.00, 1, GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT Lecturers OFF;
GO

-- Seed old Users table
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, FullName, Email, Role) VALUES
(1, 'Admin User', 'admin@university.ac.za', 2),
(2, 'Coordinator User', 'coordinator@university.ac.za', 1);
SET IDENTITY_INSERT Users OFF;
GO

-- Seed Roles
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
(NEWID(), 'Lecturer', 'LECTURER', NEWID()),
(NEWID(), 'ProgrammeCoordinator', 'PROGRAMMECOORDINATOR', NEWID()),
(NEWID(), 'AcademicManager', 'ACADEMICMANAGER', NEWID()),
(NEWID(), 'HR', 'HR', NEWID());
GO

-- Note: Users will be created automatically by the application on first run
-- The application seeds these users with proper password hashing:
-- - lecturer@university.ac.za / Lecturer123 (Lecturer role)
-- - coordinator@university.ac.za / Coordinator123 (ProgrammeCoordinator role)
-- - manager@university.ac.za / Manager123 (AcademicManager role)
-- - hr@university.ac.za / HrStaff123 (HR role)
-- - michael.chen@university.ac.za / Lecturer123 (Lecturer role, linked to Lecturer ID 2)
-- - thandi.nkosi@university.ac.za / Lecturer123 (Lecturer role, linked to Lecturer ID 3)

PRINT 'Database setup complete!';
PRINT 'Start the application - it will create demo users automatically.';
GO

 
