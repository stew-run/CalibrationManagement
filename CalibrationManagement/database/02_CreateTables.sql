/* ============================================================================
   02_CreateTables.sql  -  Tables, constraints and foreign keys. Re-runnable.
   ============================================================================ */
USE [CalibrationManagement];
GO
SET QUOTED_IDENTIFIER ON;   -- required by filtered indexes; stored in each procedure's metadata
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO

/* ---------- Roles ---------- */
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
CREATE TABLE dbo.Roles (
    RoleId       TINYINT       NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    RoleName     NVARCHAR(50)  NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE,
    Description  NVARCHAR(250) NULL
);
GO

/* ---------- Users ---------- */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    UserId            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Username          NVARCHAR(50)  NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    Email             NVARCHAR(256) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    FullName          NVARCHAR(150) NOT NULL,
    PasswordHash      NVARCHAR(200) NOT NULL,           -- BCrypt hash only, never a plain password
    RoleId            TINYINT       NOT NULL CONSTRAINT FK_Users_Roles REFERENCES dbo.Roles(RoleId),
    IsActive          BIT           NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    FailedLoginCount  INT           NOT NULL CONSTRAINT DF_Users_Failed DEFAULT (0),
    LockoutUntil      DATETIME2(0)  NULL,
    LastLoginAt       DATETIME2(0)  NULL,
    PasswordChangedAt DATETIME2(0)  NULL,
    CreatedAt         DATETIME2(0)  NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy         INT           NULL,
    UpdatedAt         DATETIME2(0)  NULL,
    UpdatedBy         INT           NULL
);
GO

/* ---------- Reference tables (Excel: Issue DEPT, Calibration Agency, Calibration Frequency) ---------- */
IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
CREATE TABLE dbo.Departments (
    DepartmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departments PRIMARY KEY,
    Name         NVARCHAR(150) NOT NULL CONSTRAINT UQ_Departments_Name UNIQUE,
    IsActive     BIT NOT NULL CONSTRAINT DF_Departments_Active DEFAULT (1),
    CreatedAt    DATETIME2(0) NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO
IF OBJECT_ID(N'dbo.CalibrationAgencies', N'U') IS NULL
CREATE TABLE dbo.CalibrationAgencies (
    AgencyId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CalibrationAgencies PRIMARY KEY,
    Name       NVARCHAR(200) NOT NULL CONSTRAINT UQ_CalibrationAgencies_Name UNIQUE,
    IsActive   BIT NOT NULL CONSTRAINT DF_Agencies_Active DEFAULT (1),
    CreatedAt  DATETIME2(0) NOT NULL CONSTRAINT DF_Agencies_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO
IF OBJECT_ID(N'dbo.CalibrationFrequencies', N'U') IS NULL
CREATE TABLE dbo.CalibrationFrequencies (
    FrequencyId INT NOT NULL CONSTRAINT PK_CalibrationFrequencies PRIMARY KEY,
    Label       NVARCHAR(50) NOT NULL CONSTRAINT UQ_CalFreq_Label UNIQUE,
    Months      INT NOT NULL CONSTRAINT CK_CalFreq_Months CHECK (Months BETWEEN 1 AND 120)
);
GO

/* ---------- Equipment (QA.FMT-27 list + QA.FMT-28 plan + QA.FMT-29 card header) ---------- */
IF OBJECT_ID(N'dbo.Equipment', N'U') IS NULL
CREATE TABLE dbo.Equipment (
    EquipmentId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Equipment PRIMARY KEY,
    EquipmentCode         NVARCHAR(50)  NOT NULL,            -- "Instrument Code No"; NOT unique: legacy data contains duplicates
    SourceSerialNo        INT           NULL,                -- "SI.No" in the 2026 calibration plan (unique when present)
    EquipmentName         NVARCHAR(250) NOT NULL,            -- "Instrument Name" / "Equipment Name"
    RangeLeastCount       NVARCHAR(150) NULL,                -- "Range & least count"
    Make                  NVARCHAR(100) NULL,
    SerialNo              NVARCHAR(100) NULL,                -- history card (old format) "Serial No"
    AcceptableErrorLimit  NVARCHAR(80)  NULL,                -- "Acceptable Error Limit" / "Deviation Allowed"
    DepartmentId          INT NULL CONSTRAINT FK_Equipment_Departments REFERENCES dbo.Departments(DepartmentId),
    Location              NVARCHAR(150) NULL,                -- history card "Location"
    CalibrationType       NVARCHAR(20)  NOT NULL CONSTRAINT DF_Equipment_CalType DEFAULT (N'External'),
    AgencyId              INT NULL CONSTRAINT FK_Equipment_Agencies REFERENCES dbo.CalibrationAgencies(AgencyId),
    FrequencyId           INT NULL CONSTRAINT FK_Equipment_Frequencies REFERENCES dbo.CalibrationFrequencies(FrequencyId),
    ResponsiblePerson     NVARCHAR(100) NULL,                -- "RESPONSIBLE PERSONAL"
    GroupName             NVARCHAR(150) NULL,                -- section heading rows in the plan, e.g. "AIR GAUGE - MASTERS"
    Status                NVARCHAR(20)  NOT NULL CONSTRAINT DF_Equipment_Status DEFAULT (N'Active'),
    SourceDocument        NVARCHAR(100) NULL,                -- e.g. "QA.FMT-28 plan 2026 row 12"
    Remarks               NVARCHAR(500) NULL,
    IsDeleted             BIT NOT NULL CONSTRAINT DF_Equipment_IsDeleted DEFAULT (0),
    CreatedAt             DATETIME2(0) NOT NULL CONSTRAINT DF_Equipment_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy             INT NULL CONSTRAINT FK_Equipment_CreatedBy REFERENCES dbo.Users(UserId),
    UpdatedAt             DATETIME2(0) NULL,
    UpdatedBy             INT NULL CONSTRAINT FK_Equipment_UpdatedBy REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Equipment_Status  CHECK (Status IN (N'Active', N'Inactive', N'OutOfService', N'Retired')),
    CONSTRAINT CK_Equipment_CalType CHECK (CalibrationType IN (N'Internal', N'External', N'Customer')),
    CONSTRAINT CK_Equipment_Code    CHECK (LEN(LTRIM(RTRIM(EquipmentCode))) > 0)
);
GO

/* ---------- Calibration schedules (calibration plan) ---------- */
IF OBJECT_ID(N'dbo.CalibrationSchedules', N'U') IS NULL
CREATE TABLE dbo.CalibrationSchedules (
    ScheduleId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CalibrationSchedules PRIMARY KEY,
    EquipmentId   INT NOT NULL CONSTRAINT FK_Schedules_Equipment REFERENCES dbo.Equipment(EquipmentId),
    PlanYear      SMALLINT NOT NULL,
    DueDate       DATE NOT NULL,                              -- "Next Due Date"
    AssignedTo    NVARCHAR(100) NULL,
    Status        NVARCHAR(20) NOT NULL CONSTRAINT DF_Schedules_Status DEFAULT (N'Planned'),
    Notes         NVARCHAR(500) NULL,
    CompletedAt   DATETIME2(0) NULL,
    CreatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_Schedules_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy     INT NULL CONSTRAINT FK_Schedules_CreatedBy REFERENCES dbo.Users(UserId),
    UpdatedAt     DATETIME2(0) NULL,
    UpdatedBy     INT NULL CONSTRAINT FK_Schedules_UpdatedBy REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Schedules_Status CHECK (Status IN (N'Planned', N'Completed', N'Cancelled')),
    CONSTRAINT CK_Schedules_Year   CHECK (PlanYear BETWEEN 2000 AND 2100)
);
GO

/* ---------- Calibration results (QA.FMT-29 history card rows) ---------- */
IF OBJECT_ID(N'dbo.CalibrationResults', N'U') IS NULL
CREATE TABLE dbo.CalibrationResults (
    ResultId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CalibrationResults PRIMARY KEY,
    EquipmentId           INT NOT NULL CONSTRAINT FK_Results_Equipment REFERENCES dbo.Equipment(EquipmentId),
    ScheduleId            INT NULL CONSTRAINT FK_Results_Schedules REFERENCES dbo.CalibrationSchedules(ScheduleId),
    CalibrationDate       DATE NOT NULL,                      -- "Calibration Date" / "Date of Calibration"
    CertificateReference  NVARCHAR(100) NULL,                 -- "Calibration Cert Ref" / "Certificate reference"
    AgencyId              INT NULL CONSTRAINT FK_Results_Agencies REFERENCES dbo.CalibrationAgencies(AgencyId),
    ErrorDetails          NVARCHAR(500) NULL,                 -- "Error Details"
    Result                NVARCHAR(12) NOT NULL,              -- OK / NOTOK / Unverified
    ActionTaken           NVARCHAR(500) NULL,                 -- "If Not OK, Action Taken"
    DueDate               DATE NOT NULL,                      -- "Due Date" / "Next Due Date"
    Remarks               NVARCHAR(500) NULL,
    SignedBy              NVARCHAR(100) NULL,                 -- "Sign"
    CreatedAt             DATETIME2(0) NOT NULL CONSTRAINT DF_Results_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy             INT NULL CONSTRAINT FK_Results_CreatedBy REFERENCES dbo.Users(UserId),
    UpdatedAt             DATETIME2(0) NULL,
    UpdatedBy             INT NULL CONSTRAINT FK_Results_UpdatedBy REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Results_Result CHECK (Result IN (N'OK', N'NOTOK', N'Unverified')),
    CONSTRAINT CK_Results_Dates  CHECK (DueDate >= CalibrationDate OR Result = N'Unverified'),
    CONSTRAINT CK_Results_Action CHECK (Result <> N'NOTOK' OR LEN(ISNULL(ActionTaken, N'')) > 0)
);
GO

/* ---------- Certificates (file metadata only; the file itself is stored outside the database) ---------- */
IF OBJECT_ID(N'dbo.Certificates', N'U') IS NULL
CREATE TABLE dbo.Certificates (
    CertificateId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Certificates PRIMARY KEY,
    EquipmentId       INT NOT NULL CONSTRAINT FK_Certificates_Equipment REFERENCES dbo.Equipment(EquipmentId),
    ResultId          INT NULL CONSTRAINT FK_Certificates_Results REFERENCES dbo.CalibrationResults(ResultId),
    CertificateNumber NVARCHAR(100) NOT NULL,
    AgencyId          INT NULL CONSTRAINT FK_Certificates_Agencies REFERENCES dbo.CalibrationAgencies(AgencyId),
    IssueDate         DATE NOT NULL,
    ExpiryDate        DATE NULL,
    FileName          NVARCHAR(260) NULL,
    ContentType       NVARCHAR(100) NULL,
    FileSizeBytes     BIGINT NULL CONSTRAINT CK_Certificates_Size CHECK (FileSizeBytes IS NULL OR FileSizeBytes >= 0),
    StorageLocation   NVARCHAR(500) NULL,                    -- path / URL / document-system reference
    Notes             NVARCHAR(500) NULL,
    CreatedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_Certificates_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy         INT NULL CONSTRAINT FK_Certificates_CreatedBy REFERENCES dbo.Users(UserId),
    UpdatedAt         DATETIME2(0) NULL,
    UpdatedBy         INT NULL CONSTRAINT FK_Certificates_UpdatedBy REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Certificates_Dates CHECK (ExpiryDate IS NULL OR ExpiryDate >= IssueDate)
);
GO

/* ---------- Maintenance ---------- */
IF OBJECT_ID(N'dbo.MaintenanceRecords', N'U') IS NULL
CREATE TABLE dbo.MaintenanceRecords (
    MaintenanceId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MaintenanceRecords PRIMARY KEY,
    EquipmentId         INT NOT NULL CONSTRAINT FK_Maintenance_Equipment REFERENCES dbo.Equipment(EquipmentId),
    MaintenanceType     NVARCHAR(20) NOT NULL,
    MaintenanceDate     DATE NOT NULL,
    PerformedBy         NVARCHAR(100) NULL,
    Description         NVARCHAR(1000) NOT NULL,
    Cost                DECIMAL(12,2) NULL CONSTRAINT CK_Maintenance_Cost CHECK (Cost IS NULL OR Cost >= 0),
    NextMaintenanceDate DATE NULL,
    Status              NVARCHAR(20) NOT NULL CONSTRAINT DF_Maintenance_Status DEFAULT (N'Open'),
    CreatedAt           DATETIME2(0) NOT NULL CONSTRAINT DF_Maintenance_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy           INT NULL CONSTRAINT FK_Maintenance_CreatedBy REFERENCES dbo.Users(UserId),
    UpdatedAt           DATETIME2(0) NULL,
    UpdatedBy           INT NULL CONSTRAINT FK_Maintenance_UpdatedBy REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Maintenance_Type   CHECK (MaintenanceType IN (N'Preventive', N'Corrective', N'Repair', N'Inspection')),
    CONSTRAINT CK_Maintenance_Status CHECK (Status IN (N'Open', N'InProgress', N'Completed')),
    CONSTRAINT CK_Maintenance_Next   CHECK (NextMaintenanceDate IS NULL OR NextMaintenanceDate >= MaintenanceDate)
);
GO

/* ---------- Notifications (one row per recipient user) ---------- */
IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
CREATE TABLE dbo.Notifications (
    NotificationId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
    UserId          INT NOT NULL CONSTRAINT FK_Notifications_Users REFERENCES dbo.Users(UserId),
    EquipmentId     INT NULL CONSTRAINT FK_Notifications_Equipment REFERENCES dbo.Equipment(EquipmentId),
    NotificationType NVARCHAR(30) NOT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Message         NVARCHAR(1000) NOT NULL,
    DedupeKey       NVARCHAR(200) NULL,
    IsRead          BIT NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT (0),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT (SYSUTCDATETIME()),
    ReadAt          DATETIME2(0) NULL,
    CONSTRAINT CK_Notifications_Type CHECK (NotificationType IN (N'DueSoon', N'Overdue', N'Failed', N'Maintenance', N'System'))
);
GO

/* ---------- Audit log ---------- */
IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
CREATE TABLE dbo.AuditLog (
    AuditId     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
    UserId      INT NULL,
    Action      NVARCHAR(50)  NOT NULL,
    EntityType  NVARCHAR(50)  NOT NULL,
    EntityId    NVARCHAR(50)  NULL,
    Details     NVARCHAR(1000) NULL,
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLog_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

/* ---------- Data quality issues found while importing the Excel templates ---------- */
IF OBJECT_ID(N'dbo.DataQualityIssues', N'U') IS NULL
CREATE TABLE dbo.DataQualityIssues (
    IssueId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DataQualityIssues PRIMARY KEY,
    SourceFile     NVARCHAR(200) NOT NULL,
    SourceRow      INT NULL,
    IssueType      NVARCHAR(50)  NOT NULL,
    Severity       NVARCHAR(10)  NOT NULL CONSTRAINT CK_DQI_Severity CHECK (Severity IN (N'Info', N'Warning', N'Error')),
    EquipmentCode  NVARCHAR(50)  NULL,
    EquipmentId    INT NULL CONSTRAINT FK_DQI_Equipment REFERENCES dbo.Equipment(EquipmentId),
    Details        NVARCHAR(1000) NOT NULL,
    IsResolved     BIT NOT NULL CONSTRAINT DF_DQI_Resolved DEFAULT (0),
    CreatedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_DQI_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO
