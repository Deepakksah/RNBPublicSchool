-- ============================================================================
-- SQL SERVER DATABASE CREATION, SCHEMA, INDEXES, VIEWS & SEED SCRIPT
-- DATABASE: SchoolDB
-- ============================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SchoolDB')
BEGIN
    CREATE DATABASE [SchoolDB];
END
GO

USE [SchoolDB];
GO

-- 1. ASP.NET IDENTITY TABLES
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(256) NULL,
        [NormalizedName] NVARCHAR(256) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        [Description] NVARCHAR(250) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserName] NVARCHAR(256) NULL,
        [NormalizedUserName] NVARCHAR(256) NULL,
        [Email] NVARCHAR(256) NULL,
        [NormalizedEmail] NVARCHAR(256) NULL,
        [EmailConfirmed] BIT NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NULL,
        [SecurityStamp] NVARCHAR(MAX) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        [PhoneNumber] NVARCHAR(MAX) NULL,
        [PhoneNumberConfirmed] BIT NOT NULL,
        [TwoFactorEnabled] BIT NOT NULL,
        [LockoutEnd] DATETIMEOFFSET NULL,
        [LockoutEnabled] BIT NOT NULL,
        [AccessFailedCount] INT NOT NULL,
        [FullName] NVARCHAR(100) NOT NULL,
        [SchoolId] INT NULL,
        [ProfilePicture] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [LastLoginDate] DATETIME2 NULL,
        [StudentId] INT NULL,
        [TeacherId] INT NULL,
        [ParentId] INT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] NVARCHAR(128) NOT NULL,
        [ProviderKey] NVARCHAR(128) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId] NVARCHAR(450) NOT NULL,
        [LoginProvider] NVARCHAR(128) NOT NULL,
        [Name] NVARCHAR(128) NOT NULL,
        [Value] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

-- 2. SCHOOLS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Schools')
BEGIN
    CREATE TABLE [dbo].[Schools] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(150) NOT NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [RegistrationNumber] NVARCHAR(100) NULL,
        [Logo] NVARCHAR(255) NULL,
        [Banner] NVARCHAR(255) NULL,
        [Address] NVARCHAR(250) NOT NULL,
        [City] NVARCHAR(100) NOT NULL,
        [State] NVARCHAR(100) NOT NULL,
        [PinCode] NVARCHAR(20) NOT NULL,
        [Phone] NVARCHAR(20) NOT NULL,
        [Email] NVARCHAR(150) NOT NULL,
        [Website] NVARCHAR(200) NULL,
        [EstablishedYear] INT NOT NULL,
        [Status] INT NOT NULL DEFAULT (1),
        [About] NVARCHAR(MAX) NULL,
        [Vision] NVARCHAR(MAX) NULL,
        [Mission] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        CONSTRAINT [PK_Schools] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_Schools_Code] UNIQUE ([Code])
    );
END
GO

-- 3. PRINCIPALS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Principals')
BEGIN
    CREATE TABLE [dbo].[Principals] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Photo] NVARCHAR(255) NULL,
        [Qualification] NVARCHAR(150) NULL,
        [Experience] NVARCHAR(100) NULL,
        [Phone] NVARCHAR(20) NULL,
        [Email] NVARCHAR(150) NULL,
        [Message] NVARCHAR(MAX) NULL,
        [Vision] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        CONSTRAINT [PK_Principals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Principals_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE CASCADE
    );
END
GO

-- 4. ACADEMIC YEARS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicYears')
BEGIN
    CREATE TABLE [dbo].[AcademicYears] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [Name] NVARCHAR(50) NOT NULL,
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NOT NULL,
        [IsCurrent] BIT NOT NULL DEFAULT (0),
        [IsActive] BIT NOT NULL DEFAULT (1),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_AcademicYears] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AcademicYears_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id])
    );
END
GO

-- 5. CLASSES & SECTIONS TABLES
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Classes')
BEGIN
    CREATE TABLE [dbo].[Classes] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [Name] NVARCHAR(50) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT (1),
        [IsActive] BIT NOT NULL DEFAULT (1),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Classes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Classes_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teachers')
BEGIN
    CREATE TABLE [dbo].[Teachers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [EmployeeId] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Photo] NVARCHAR(255) NULL,
        [Gender] NVARCHAR(20) NOT NULL,
        [DateOfBirth] DATETIME2 NOT NULL,
        [Qualification] NVARCHAR(150) NULL,
        [Experience] NVARCHAR(100) NULL,
        [Subject] NVARCHAR(100) NULL,
        [Designation] NVARCHAR(100) NOT NULL,
        [Mobile] NVARCHAR(20) NOT NULL,
        [Email] NVARCHAR(150) NOT NULL,
        [Address] NVARCHAR(250) NOT NULL,
        [JoiningDate] DATETIME2 NOT NULL,
        [Status] INT NOT NULL DEFAULT (1),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        [UserId] NVARCHAR(450) NULL,
        CONSTRAINT [PK_Teachers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Teachers_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]),
        CONSTRAINT [UQ_Teachers_School_EmpId] UNIQUE ([SchoolId], [EmployeeId])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sections')
BEGIN
    CREATE TABLE [dbo].[Sections] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [ClassId] INT NOT NULL,
        [Name] NVARCHAR(20) NOT NULL,
        [RoomNumber] NVARCHAR(50) NULL,
        [Capacity] INT NOT NULL DEFAULT (40),
        [ClassTeacherId] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Sections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sections_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Sections_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]),
        CONSTRAINT [FK_Sections_Teachers_ClassTeacherId] FOREIGN KEY ([ClassTeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE SET NULL
    );
END
GO

-- 6. STUDENTS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE [dbo].[Students] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [AcademicYearId] INT NOT NULL,
        [ClassId] INT NOT NULL,
        [SectionId] INT NOT NULL,
        [AdmissionNumber] NVARCHAR(50) NOT NULL,
        [RollNumber] INT NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Photo] NVARCHAR(255) NULL,
        [DateOfBirth] DATETIME2 NOT NULL,
        [Gender] NVARCHAR(20) NOT NULL,
        [BloodGroup] NVARCHAR(10) NULL,
        [FatherName] NVARCHAR(100) NOT NULL,
        [MotherName] NVARCHAR(100) NOT NULL,
        [GuardianName] NVARCHAR(100) NULL,
        [FatherMobile] NVARCHAR(20) NOT NULL,
        [MotherMobile] NVARCHAR(20) NULL,
        [Email] NVARCHAR(150) NULL,
        [Address] NVARCHAR(250) NOT NULL,
        [City] NVARCHAR(100) NOT NULL,
        [State] NVARCHAR(100) NOT NULL,
        [PinCode] NVARCHAR(20) NOT NULL,
        [AdmissionDate] DATETIME2 NOT NULL,
        [Status] INT NOT NULL DEFAULT (1),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        [UserId] NVARCHAR(450) NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Students_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]),
        CONSTRAINT [FK_Students_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]),
        CONSTRAINT [FK_Students_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]),
        CONSTRAINT [FK_Students_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]),
        CONSTRAINT [UQ_Students_School_Admission] UNIQUE ([SchoolId], [AdmissionNumber])
    );
END
GO

-- 7. ATTENDANCE TABLES
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StudentAttendances')
BEGIN
    CREATE TABLE [dbo].[StudentAttendances] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [AcademicYearId] INT NOT NULL,
        [ClassId] INT NOT NULL,
        [SectionId] INT NOT NULL,
        [StudentId] INT NOT NULL,
        [AttendanceDate] DATE NOT NULL,
        [Status] INT NOT NULL DEFAULT (1),
        [Remarks] NVARCHAR(255) NULL,
        [RecordedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RecordedBy] NVARCHAR(100) NULL,
        CONSTRAINT [PK_StudentAttendances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentAttendances_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]),
        CONSTRAINT [FK_StudentAttendances_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_StudentAttendance_UniqueDaily] UNIQUE ([StudentId], [AttendanceDate], [AcademicYearId])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TeacherAttendances')
BEGIN
    CREATE TABLE [dbo].[TeacherAttendances] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [AcademicYearId] INT NOT NULL,
        [TeacherId] INT NOT NULL,
        [AttendanceDate] DATE NOT NULL,
        [Status] INT NOT NULL DEFAULT (1),
        [Remarks] NVARCHAR(255) NULL,
        [RecordedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [RecordedBy] NVARCHAR(100) NULL,
        CONSTRAINT [PK_TeacherAttendances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TeacherAttendances_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]),
        CONSTRAINT [FK_TeacherAttendances_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_TeacherAttendance_UniqueDaily] UNIQUE ([TeacherId], [AttendanceDate], [AcademicYearId])
    );
END
GO

-- 8. GALLERY, NOTIFICATIONS, HOLIDAYS & AUDIT LOGS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SchoolImages')
BEGIN
    CREATE TABLE [dbo].[SchoolImages] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NOT NULL,
        [Title] NVARCHAR(150) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [ImagePath] NVARCHAR(255) NOT NULL,
        [Category] NVARCHAR(50) NOT NULL DEFAULT ('Campus'),
        [IsCoverImage] BIT NOT NULL DEFAULT (0),
        [UploadDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UploadedBy] NVARCHAR(100) NULL,
        CONSTRAINT [PK_SchoolImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SchoolImages_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Type] INT NOT NULL DEFAULT (1),
        [Audience] INT NOT NULL DEFAULT (1),
        [PublishDate] DATE NOT NULL,
        [ExpiryDate] DATE NULL,
        [AttachmentPath] NVARCHAR(255) NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [CreatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Holidays')
BEGIN
    CREATE TABLE [dbo].[Holidays] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NULL,
        [AcademicYearId] INT NOT NULL,
        [Name] NVARCHAR(150) NOT NULL,
        [HolidayDate] DATE NOT NULL,
        [EndDate] DATE NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT (1),
        [CreatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Holidays] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Holidays_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NULL,
        [UserId] NVARCHAR(100) NULL,
        [UserName] NVARCHAR(100) NOT NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [Entity] NVARCHAR(100) NOT NULL,
        [EntityId] NVARCHAR(50) NULL,
        [Details] NVARCHAR(1000) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [DateTime] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemSettings')
BEGIN
    CREATE TABLE [dbo].[SystemSettings] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SchoolId] INT NULL,
        [SettingKey] NVARCHAR(100) NOT NULL,
        [SettingValue] NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(250) NULL,
        [UpdatedDate] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END
GO

-- 9. PERFORMANCE INDEXES
CREATE NONCLUSTERED INDEX [IX_StudentAttendances_School_Date] ON [StudentAttendances] ([SchoolId], [AttendanceDate]);
CREATE NONCLUSTERED INDEX [IX_StudentAttendances_Class_Section] ON [StudentAttendances] ([ClassId], [SectionId]);
CREATE NONCLUSTERED INDEX [IX_TeacherAttendances_School_Date] ON [TeacherAttendances] ([SchoolId], [AttendanceDate]);
CREATE NONCLUSTERED INDEX [IX_Students_School_Class_Sec] ON [Students] ([SchoolId], [ClassId], [SectionId]);
CREATE NONCLUSTERED INDEX [IX_Teachers_SchoolId] ON [Teachers] ([SchoolId]);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_School_Date] ON [AuditLogs] ([SchoolId], [DateTime]);
GO

-- 10. ANALYTICAL VIEWS
IF OBJECT_ID('dbo.vw_DailySchoolAttendance', 'V') IS NOT NULL
    DROP VIEW dbo.vw_DailySchoolAttendance;
GO

CREATE VIEW dbo.vw_DailySchoolAttendance AS
SELECT 
    s.Id AS SchoolId,
    s.Name AS SchoolName,
    sa.AttendanceDate,
    COUNT(sa.Id) AS TotalMarked,
    SUM(CASE WHEN sa.Status = 1 THEN 1 ELSE 0 END) AS PresentStudents,
    SUM(CASE WHEN sa.Status = 2 THEN 1 ELSE 0 END) AS AbsentStudents,
    CAST(SUM(CASE WHEN sa.Status = 1 THEN 1.0 ELSE 0.0 END) / NULLIF(COUNT(sa.Id), 0) * 100 AS DECIMAL(5,2)) AS AttendancePercentage
FROM dbo.Schools s
LEFT JOIN dbo.StudentAttendances sa ON s.Id = sa.SchoolId
GROUP BY s.Id, s.Name, sa.AttendanceDate;
GO

PRINT 'MSSQL Database Schema and Tables created successfully on SchoolDB.';
GO
