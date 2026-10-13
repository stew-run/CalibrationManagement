/* ============================================================================
   04_CreateStoredProcedures.sql  -  Views and stored procedures (CREATE OR ALTER, re-runnable)
   Requires SQL Server 2016 SP1 or later.
   Error numbers raised with THROW (mapped to HTTP codes by the API):
     50001 = not found (404)   50002 = conflict / duplicate (409)
     50003 = validation (400)  50004 = business rule violation (422)
   ============================================================================ */
USE [CalibrationManagement];
GO
SET QUOTED_IDENTIFIER ON;   -- required by filtered indexes; stored in each procedure's metadata
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO

/* ---------------------------------------------------------------- VIEWS */
CREATE OR ALTER VIEW dbo.vw_EquipmentCalibration
AS
SELECT  e.EquipmentId,
        lr.ResultId         AS LastResultId,
        lr.CalibrationDate  AS LastCalibrationDate,
        lr.Result           AS LastResult,
        lr.DueDate          AS NextDueDate,
        CASE WHEN lr.ResultId IS NULL THEN N'NotCalibrated'
             WHEN lr.Result = N'NOTOK' THEN N'Failed'
             WHEN lr.DueDate < CAST(SYSDATETIME() AS DATE) THEN N'Overdue'
             WHEN lr.DueDate <= DATEADD(DAY, 30, CAST(SYSDATETIME() AS DATE)) THEN N'DueSoon'
             ELSE N'Valid' END AS CalibrationStatus
FROM dbo.Equipment e
OUTER APPLY (SELECT TOP (1) r.ResultId, r.CalibrationDate, r.Result, r.DueDate
             FROM dbo.CalibrationResults r
             WHERE r.EquipmentId = e.EquipmentId
             ORDER BY r.CalibrationDate DESC, r.ResultId DESC) lr;
GO

CREATE OR ALTER VIEW dbo.vw_EquipmentFull
AS
SELECT  e.EquipmentId, e.EquipmentCode, e.SourceSerialNo, e.EquipmentName, e.RangeLeastCount, e.Make, e.SerialNo,
        e.AcceptableErrorLimit, e.DepartmentId, d.Name AS DepartmentName, e.Location, e.CalibrationType,
        e.AgencyId, a.Name AS AgencyName, e.FrequencyId, f.Label AS FrequencyLabel, f.Months AS FrequencyMonths,
        e.ResponsiblePerson, e.GroupName, e.Status, e.SourceDocument, e.Remarks, e.IsDeleted,
        c.LastCalibrationDate, c.LastResult, c.NextDueDate, c.CalibrationStatus,
        (SELECT COUNT(*) FROM dbo.Equipment x WHERE x.IsDeleted = 0 AND x.EquipmentCode = e.EquipmentCode) AS CodeCount,
        e.CreatedAt, e.UpdatedAt
FROM dbo.Equipment e
LEFT JOIN dbo.Departments d ON d.DepartmentId = e.DepartmentId
LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = e.AgencyId
LEFT JOIN dbo.CalibrationFrequencies f ON f.FrequencyId = e.FrequencyId
JOIN dbo.vw_EquipmentCalibration c ON c.EquipmentId = e.EquipmentId;
GO

/* ---------------------------------------------------------------- AUDIT */
CREATE OR ALTER PROCEDURE dbo.usp_Audit_Write
    @UserId INT, @Action NVARCHAR(50), @EntityType NVARCHAR(50), @EntityId NVARCHAR(50), @Details NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (UserId, Action, EntityType, EntityId, Details)
    VALUES (@UserId, @Action, @EntityType, @EntityId, LEFT(@Details, 1000));
END
GO

/* ---------------------------------------------------------------- USERS */
CREATE OR ALTER PROCEDURE dbo.usp_Roles_List
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description FROM dbo.Roles ORDER BY RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_GetByUsername @Username NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId, u.Username, u.Email, u.FullName, u.PasswordHash, u.RoleId, r.RoleName AS Role,
           u.IsActive, u.FailedLoginCount, u.LockoutUntil, u.LastLoginAt, u.CreatedAt
    FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId
    WHERE u.Username = @Username OR u.Email = @Username;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_GetById @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId, u.Username, u.Email, u.FullName, u.PasswordHash, u.RoleId, r.RoleName AS Role,
           u.IsActive, u.FailedLoginCount, u.LockoutUntil, u.LastLoginAt, u.CreatedAt
    FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId
    WHERE u.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_List
    @Search NVARCHAR(100) = NULL, @Page INT = 1, @PageSize INT = 25
AS
BEGIN
    SET NOCOUNT ON;
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 25;
    IF @PageSize > 200 SET @PageSize = 200;
    DECLARE @Like NVARCHAR(210) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @Like = N'%' + REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Search)), N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
    SELECT u.UserId, u.Username, u.Email, u.FullName, u.RoleId, r.RoleName AS Role, u.IsActive,
           u.LastLoginAt, u.CreatedAt, COUNT(*) OVER () AS TotalCount
    FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId
    WHERE @Like IS NULL OR u.Username LIKE @Like OR u.Email LIKE @Like OR u.FullName LIKE @Like
    ORDER BY u.FullName, u.UserId
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_Create
    @Username NVARCHAR(50), @Email NVARCHAR(256), @FullName NVARCHAR(150),
    @PasswordHash NVARCHAR(200), @RoleId TINYINT, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleId = @RoleId) THROW 50003, N'Role does not exist.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username) THROW 50002, N'Username is already in use.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email) THROW 50002, N'Email is already in use.', 1;
    BEGIN TRAN;
        INSERT dbo.Users (Username, Email, FullName, PasswordHash, RoleId, PasswordChangedAt, CreatedBy)
        VALUES (@Username, @Email, @FullName, @PasswordHash, @RoleId, SYSUTCDATETIME(), @ActorUserId);
        DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Create', N'User', @NewId, @Username;
    COMMIT;
    SELECT @NewId AS UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_Update
    @UserId INT, @Email NVARCHAR(256), @FullName NVARCHAR(150), @RoleId TINYINT, @IsActive BIT, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @AdminRoleId TINYINT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Administrator');
    DECLARE @CurrentRole TINYINT, @CurrentActive BIT;
    SELECT @CurrentRole = RoleId, @CurrentActive = IsActive FROM dbo.Users WHERE UserId = @UserId;
    IF @CurrentRole IS NULL THROW 50001, N'User not found.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleId = @RoleId) THROW 50003, N'Role does not exist.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email AND UserId <> @UserId) THROW 50002, N'Email is already in use.', 1;
    IF @CurrentRole = @AdminRoleId AND @CurrentActive = 1 AND (@RoleId <> @AdminRoleId OR @IsActive = 0)
       AND NOT EXISTS (SELECT 1 FROM dbo.Users WHERE RoleId = @AdminRoleId AND IsActive = 1 AND UserId <> @UserId)
        THROW 50004, N'At least one active administrator must remain.', 1;
    BEGIN TRAN;
        UPDATE dbo.Users SET Email = @Email, FullName = @FullName, RoleId = @RoleId, IsActive = @IsActive,
               UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
        WHERE UserId = @UserId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Update', N'User', @UserId, NULL;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_UpdateProfile @UserId INT, @Email NVARCHAR(256), @FullName NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = @UserId) THROW 50001, N'User not found.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email AND UserId <> @UserId) THROW 50002, N'Email is already in use.', 1;
    UPDATE dbo.Users SET Email = @Email, FullName = @FullName, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UserId
    WHERE UserId = @UserId;
    EXEC dbo.usp_Audit_Write @UserId, N'UpdateProfile', N'User', @UserId, NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_ChangePassword @UserId INT, @PasswordHash NVARCHAR(200), @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = @UserId) THROW 50001, N'User not found.', 1;
    UPDATE dbo.Users SET PasswordHash = @PasswordHash, PasswordChangedAt = SYSUTCDATETIME(),
           FailedLoginCount = 0, LockoutUntil = NULL, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
    WHERE UserId = @UserId;
    EXEC dbo.usp_Audit_Write @ActorUserId, N'ChangePassword', N'User', @UserId, NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_RecordLogin
    @UserId INT, @Success BIT, @MaxFailures INT = 5, @LockoutMinutes INT = 15
AS
BEGIN
    SET NOCOUNT ON;
    IF @Success = 1
        UPDATE dbo.Users SET FailedLoginCount = 0, LockoutUntil = NULL, LastLoginAt = SYSUTCDATETIME() WHERE UserId = @UserId;
    ELSE
        UPDATE dbo.Users
        SET FailedLoginCount = CASE WHEN FailedLoginCount + 1 >= @MaxFailures THEN 0 ELSE FailedLoginCount + 1 END,
            LockoutUntil = CASE WHEN FailedLoginCount + 1 >= @MaxFailures THEN DATEADD(MINUTE, @LockoutMinutes, SYSUTCDATETIME()) ELSE LockoutUntil END
        WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_CreateInitialAdmin
    @Username NVARCHAR(50), @Email NVARCHAR(256), @FullName NVARCHAR(150), @PasswordHash NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @AdminRoleId TINYINT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Administrator');
    IF @AdminRoleId IS NULL THROW 50004, N'Roles are not seeded. Run 05_SeedReferenceData.sql first.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE RoleId = @AdminRoleId)
        THROW 50004, N'An administrator already exists. Initial provisioning is only allowed once.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username OR Email = @Email)
        THROW 50002, N'Username or email is already in use.', 1;
    BEGIN TRAN;
        INSERT dbo.Users (Username, Email, FullName, PasswordHash, RoleId, PasswordChangedAt)
        VALUES (@Username, @Email, @FullName, @PasswordHash, @AdminRoleId, SYSUTCDATETIME());
        DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);
        EXEC dbo.usp_Audit_Write @NewId, N'InitialAdmin', N'User', @NewId, @Username;
    COMMIT;
    SELECT @NewId AS UserId;
END
GO

/* -------------------------------------------------------------- LOOKUPS */
CREATE OR ALTER PROCEDURE dbo.usp_Lookups_Get
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DepartmentId AS Id, Name FROM dbo.Departments WHERE IsActive = 1 ORDER BY Name;
    SELECT AgencyId AS Id, Name FROM dbo.CalibrationAgencies WHERE IsActive = 1 ORDER BY Name;
    SELECT FrequencyId AS Id, Label AS Name, Months FROM dbo.CalibrationFrequencies ORDER BY Months;
    SELECT CAST(RoleId AS INT) AS Id, RoleName AS Name FROM dbo.Roles ORDER BY RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Departments_Create @Name NVARCHAR(150), @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @Name = LTRIM(RTRIM(@Name));
    IF LEN(@Name) = 0 THROW 50003, N'Name is required.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Departments WHERE Name = @Name) THROW 50002, N'Department already exists.', 1;
    INSERT dbo.Departments (Name) VALUES (@Name);
    DECLARE @Id INT = CAST(SCOPE_IDENTITY() AS INT);
    EXEC dbo.usp_Audit_Write @ActorUserId, N'Create', N'Department', @Id, @Name;
    SELECT @Id AS Id, @Name AS Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Agencies_Create @Name NVARCHAR(200), @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @Name = LTRIM(RTRIM(@Name));
    IF LEN(@Name) = 0 THROW 50003, N'Name is required.', 1;
    IF EXISTS (SELECT 1 FROM dbo.CalibrationAgencies WHERE Name = @Name) THROW 50002, N'Calibration agency already exists.', 1;
    INSERT dbo.CalibrationAgencies (Name) VALUES (@Name);
    DECLARE @Id INT = CAST(SCOPE_IDENTITY() AS INT);
    EXEC dbo.usp_Audit_Write @ActorUserId, N'Create', N'CalibrationAgency', @Id, @Name;
    SELECT @Id AS Id, @Name AS Name;
END
GO

/* ------------------------------------------------------------ EQUIPMENT */
CREATE OR ALTER PROCEDURE dbo.usp_Equipment_Search
    @Search NVARCHAR(100) = NULL, @DepartmentId INT = NULL, @AgencyId INT = NULL,
    @Status NVARCHAR(20) = NULL, @CalibrationStatus NVARCHAR(20) = NULL, @DuplicatesOnly BIT = 0,
    @SortBy NVARCHAR(20) = N'code', @SortDir NVARCHAR(4) = N'asc',
    @Page INT = 1, @PageSize INT = 25
AS
BEGIN
    SET NOCOUNT ON;
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 25;
    IF @PageSize > 200 SET @PageSize = 200;
    DECLARE @Like NVARCHAR(210) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @Like = N'%' + REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Search)), N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
    DECLARE @Desc BIT = CASE WHEN LOWER(@SortDir) = N'desc' THEN 1 ELSE 0 END;

    SELECT v.EquipmentId, v.EquipmentCode, v.SourceSerialNo, v.EquipmentName, v.RangeLeastCount, v.Make, v.SerialNo,
           v.AcceptableErrorLimit, v.DepartmentId, v.DepartmentName, v.Location, v.CalibrationType, v.AgencyId, v.AgencyName,
           v.FrequencyId, v.FrequencyLabel, v.FrequencyMonths, v.ResponsiblePerson, v.GroupName, v.Status, v.Remarks,
           v.LastCalibrationDate, v.NextDueDate, v.CalibrationStatus, v.CodeCount, v.CreatedAt, v.UpdatedAt,
           COUNT(*) OVER () AS TotalCount
    FROM dbo.vw_EquipmentFull v
    WHERE v.IsDeleted = 0
      AND (@DepartmentId IS NULL OR v.DepartmentId = @DepartmentId)
      AND (@AgencyId IS NULL OR v.AgencyId = @AgencyId)
      AND (@Status IS NULL OR v.Status = @Status)
      AND (@CalibrationStatus IS NULL OR v.CalibrationStatus = @CalibrationStatus)
      AND (@DuplicatesOnly = 0 OR v.CodeCount > 1)
      AND (@Like IS NULL OR v.EquipmentCode LIKE @Like OR v.EquipmentName LIKE @Like OR v.Make LIKE @Like
           OR v.DepartmentName LIKE @Like OR v.ResponsiblePerson LIKE @Like OR v.RangeLeastCount LIKE @Like)
    ORDER BY
        CASE WHEN @SortBy = N'code' AND @Desc = 0 THEN v.EquipmentCode END ASC,
        CASE WHEN @SortBy = N'code' AND @Desc = 1 THEN v.EquipmentCode END DESC,
        CASE WHEN @SortBy = N'name' AND @Desc = 0 THEN v.EquipmentName END ASC,
        CASE WHEN @SortBy = N'name' AND @Desc = 1 THEN v.EquipmentName END DESC,
        CASE WHEN @SortBy = N'department' AND @Desc = 0 THEN v.DepartmentName END ASC,
        CASE WHEN @SortBy = N'department' AND @Desc = 1 THEN v.DepartmentName END DESC,
        CASE WHEN @SortBy = N'due' AND @Desc = 0 THEN v.NextDueDate END ASC,
        CASE WHEN @SortBy = N'due' AND @Desc = 1 THEN v.NextDueDate END DESC,
        v.EquipmentId
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Equipment_GetById @EquipmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.EquipmentId, v.EquipmentCode, v.SourceSerialNo, v.EquipmentName, v.RangeLeastCount, v.Make, v.SerialNo,
           v.AcceptableErrorLimit, v.DepartmentId, v.DepartmentName, v.Location, v.CalibrationType, v.AgencyId, v.AgencyName,
           v.FrequencyId, v.FrequencyLabel, v.FrequencyMonths, v.ResponsiblePerson, v.GroupName, v.Status, v.Remarks,
           v.LastCalibrationDate, v.NextDueDate, v.CalibrationStatus, v.CodeCount, v.CreatedAt, v.UpdatedAt
    FROM dbo.vw_EquipmentFull v
    WHERE v.EquipmentId = @EquipmentId AND v.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Equipment_Create
    @EquipmentCode NVARCHAR(50), @EquipmentName NVARCHAR(250), @RangeLeastCount NVARCHAR(150) = NULL,
    @Make NVARCHAR(100) = NULL, @SerialNo NVARCHAR(100) = NULL, @AcceptableErrorLimit NVARCHAR(80) = NULL,
    @DepartmentId INT = NULL, @Location NVARCHAR(150) = NULL, @CalibrationType NVARCHAR(20) = N'External',
    @AgencyId INT = NULL, @FrequencyId INT = NULL, @ResponsiblePerson NVARCHAR(100) = NULL,
    @GroupName NVARCHAR(150) = NULL, @Status NVARCHAR(20) = N'Active', @Remarks NVARCHAR(500) = NULL,
    @AllowDuplicateCode BIT = 0, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    SET @EquipmentCode = LTRIM(RTRIM(@EquipmentCode));
    SET @EquipmentName = LTRIM(RTRIM(@EquipmentName));
    IF LEN(@EquipmentCode) = 0 THROW 50003, N'Equipment code is required.', 1;
    IF LEN(@EquipmentName) = 0 THROW 50003, N'Equipment name is required.', 1;
    IF @DepartmentId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentId = @DepartmentId) THROW 50003, N'Department does not exist.', 1;
    IF @AgencyId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationAgencies WHERE AgencyId = @AgencyId) THROW 50003, N'Calibration agency does not exist.', 1;
    IF @FrequencyId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationFrequencies WHERE FrequencyId = @FrequencyId) THROW 50003, N'Calibration frequency does not exist.', 1;
    IF @AllowDuplicateCode = 0 AND EXISTS (SELECT 1 FROM dbo.Equipment WHERE EquipmentCode = @EquipmentCode AND IsDeleted = 0)
        THROW 50002, N'An equipment record with this code already exists. Enable "allow duplicate code" only if this is a genuinely separate instrument.', 1;
    BEGIN TRAN;
        INSERT dbo.Equipment (EquipmentCode, EquipmentName, RangeLeastCount, Make, SerialNo, AcceptableErrorLimit, DepartmentId,
                              Location, CalibrationType, AgencyId, FrequencyId, ResponsiblePerson, GroupName, Status, SourceDocument, Remarks, CreatedBy)
        VALUES (@EquipmentCode, @EquipmentName, @RangeLeastCount, @Make, @SerialNo, @AcceptableErrorLimit, @DepartmentId,
                @Location, @CalibrationType, @AgencyId, @FrequencyId, @ResponsiblePerson, @GroupName, @Status, N'Created in application', @Remarks, @ActorUserId);
        DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Create', N'Equipment', @NewId, @EquipmentCode;
    COMMIT;
    SELECT @NewId AS EquipmentId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Equipment_Update
    @EquipmentId INT, @EquipmentCode NVARCHAR(50), @EquipmentName NVARCHAR(250), @RangeLeastCount NVARCHAR(150) = NULL,
    @Make NVARCHAR(100) = NULL, @SerialNo NVARCHAR(100) = NULL, @AcceptableErrorLimit NVARCHAR(80) = NULL,
    @DepartmentId INT = NULL, @Location NVARCHAR(150) = NULL, @CalibrationType NVARCHAR(20) = N'External',
    @AgencyId INT = NULL, @FrequencyId INT = NULL, @ResponsiblePerson NVARCHAR(100) = NULL,
    @GroupName NVARCHAR(150) = NULL, @Status NVARCHAR(20) = N'Active', @Remarks NVARCHAR(500) = NULL,
    @AllowDuplicateCode BIT = 0, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    SET @EquipmentCode = LTRIM(RTRIM(@EquipmentCode));
    SET @EquipmentName = LTRIM(RTRIM(@EquipmentName));
    IF NOT EXISTS (SELECT 1 FROM dbo.Equipment WHERE EquipmentId = @EquipmentId AND IsDeleted = 0) THROW 50001, N'Equipment not found.', 1;
    IF LEN(@EquipmentCode) = 0 THROW 50003, N'Equipment code is required.', 1;
    IF LEN(@EquipmentName) = 0 THROW 50003, N'Equipment name is required.', 1;
    IF @DepartmentId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentId = @DepartmentId) THROW 50003, N'Department does not exist.', 1;
    IF @AgencyId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationAgencies WHERE AgencyId = @AgencyId) THROW 50003, N'Calibration agency does not exist.', 1;
    IF @FrequencyId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationFrequencies WHERE FrequencyId = @FrequencyId) THROW 50003, N'Calibration frequency does not exist.', 1;
    IF @AllowDuplicateCode = 0 AND EXISTS (SELECT 1 FROM dbo.Equipment WHERE EquipmentCode = @EquipmentCode AND IsDeleted = 0 AND EquipmentId <> @EquipmentId)
        THROW 50002, N'Another equipment record already uses this code. Enable "allow duplicate code" only if this is a genuinely separate instrument.', 1;
    BEGIN TRAN;
        UPDATE dbo.Equipment
        SET EquipmentCode = @EquipmentCode, EquipmentName = @EquipmentName, RangeLeastCount = @RangeLeastCount, Make = @Make,
            SerialNo = @SerialNo, AcceptableErrorLimit = @AcceptableErrorLimit, DepartmentId = @DepartmentId, Location = @Location,
            CalibrationType = @CalibrationType, AgencyId = @AgencyId, FrequencyId = @FrequencyId, ResponsiblePerson = @ResponsiblePerson,
            GroupName = @GroupName, Status = @Status, Remarks = @Remarks, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
        WHERE EquipmentId = @EquipmentId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Update', N'Equipment', @EquipmentId, @EquipmentCode;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Equipment_Delete @EquipmentId INT, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Code NVARCHAR(50) = (SELECT EquipmentCode FROM dbo.Equipment WHERE EquipmentId = @EquipmentId AND IsDeleted = 0);
    IF @Code IS NULL THROW 50001, N'Equipment not found.', 1;
    BEGIN TRAN;
        UPDATE dbo.Equipment SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId WHERE EquipmentId = @EquipmentId;
        UPDATE dbo.CalibrationSchedules SET Status = N'Cancelled', UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
        WHERE EquipmentId = @EquipmentId AND Status = N'Planned';
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Delete', N'Equipment', @EquipmentId, @Code;
    COMMIT;
END
GO

/* ------------------------------------------------------------ SCHEDULES */
CREATE OR ALTER PROCEDURE dbo.usp_Schedules_Search
    @PlanYear INT = NULL, @Status NVARCHAR(20) = NULL, @EquipmentId INT = NULL, @DepartmentId INT = NULL,
    @From DATE = NULL, @To DATE = NULL, @Search NVARCHAR(100) = NULL, @Page INT = 1, @PageSize INT = 25
AS
BEGIN
    SET NOCOUNT ON;
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 25;
    IF @PageSize > 200 SET @PageSize = 200;
    DECLARE @Today DATE = CAST(SYSDATETIME() AS DATE);
    DECLARE @Like NVARCHAR(210) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @Like = N'%' + REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Search)), N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
    ;WITH s AS (
        SELECT s.ScheduleId, s.EquipmentId, e.EquipmentCode, e.EquipmentName, e.DepartmentId, d.Name AS DepartmentName,
               a.Name AS AgencyName, s.PlanYear, s.DueDate, s.AssignedTo, s.Status,
               CASE WHEN s.Status = N'Planned' AND s.DueDate < @Today THEN N'Overdue'
                    WHEN s.Status = N'Planned' AND s.DueDate <= DATEADD(DAY, 30, @Today) THEN N'DueSoon'
                    ELSE s.Status END AS DisplayStatus,
               DATEDIFF(DAY, @Today, s.DueDate) AS DaysUntilDue, s.Notes, s.CompletedAt
        FROM dbo.CalibrationSchedules s
        JOIN dbo.Equipment e ON e.EquipmentId = s.EquipmentId AND e.IsDeleted = 0
        LEFT JOIN dbo.Departments d ON d.DepartmentId = e.DepartmentId
        LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = e.AgencyId
    )
    SELECT s.*, COUNT(*) OVER () AS TotalCount
    FROM s
    WHERE (@PlanYear IS NULL OR s.PlanYear = @PlanYear)
      AND (@Status IS NULL OR s.DisplayStatus = @Status)
      AND (@EquipmentId IS NULL OR s.EquipmentId = @EquipmentId)
      AND (@DepartmentId IS NULL OR s.DepartmentId = @DepartmentId)
      AND (@From IS NULL OR s.DueDate >= @From)
      AND (@To IS NULL OR s.DueDate <= @To)
      AND (@Like IS NULL OR s.EquipmentCode LIKE @Like OR s.EquipmentName LIKE @Like OR s.AssignedTo LIKE @Like)
    ORDER BY s.DueDate, s.ScheduleId
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedules_GetById @ScheduleId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSDATETIME() AS DATE);
    SELECT s.ScheduleId, s.EquipmentId, e.EquipmentCode, e.EquipmentName, e.DepartmentId, d.Name AS DepartmentName,
           a.Name AS AgencyName, s.PlanYear, s.DueDate, s.AssignedTo, s.Status,
           CASE WHEN s.Status = N'Planned' AND s.DueDate < @Today THEN N'Overdue'
                WHEN s.Status = N'Planned' AND s.DueDate <= DATEADD(DAY, 30, @Today) THEN N'DueSoon'
                ELSE s.Status END AS DisplayStatus,
           DATEDIFF(DAY, @Today, s.DueDate) AS DaysUntilDue, s.Notes, s.CompletedAt
    FROM dbo.CalibrationSchedules s
    JOIN dbo.Equipment e ON e.EquipmentId = s.EquipmentId AND e.IsDeleted = 0
    LEFT JOIN dbo.Departments d ON d.DepartmentId = e.DepartmentId
    LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = e.AgencyId
    WHERE s.ScheduleId = @ScheduleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedules_Create
    @EquipmentId INT, @PlanYear INT, @DueDate DATE, @AssignedTo NVARCHAR(100) = NULL,
    @Notes NVARCHAR(500) = NULL, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Equipment WHERE EquipmentId = @EquipmentId AND IsDeleted = 0) THROW 50003, N'Equipment does not exist.', 1;
    IF EXISTS (SELECT 1 FROM dbo.CalibrationSchedules WHERE EquipmentId = @EquipmentId AND DueDate = @DueDate AND Status = N'Planned')
        THROW 50002, N'A planned calibration for this equipment and due date already exists.', 1;
    BEGIN TRAN;
        INSERT dbo.CalibrationSchedules (EquipmentId, PlanYear, DueDate, AssignedTo, Notes, CreatedBy)
        VALUES (@EquipmentId, @PlanYear, @DueDate, @AssignedTo, @Notes, @ActorUserId);
        DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Create', N'Schedule', @NewId, NULL;
    COMMIT;
    SELECT @NewId AS ScheduleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedules_Update
    @ScheduleId INT, @PlanYear INT, @DueDate DATE, @AssignedTo NVARCHAR(100) = NULL,
    @Status NVARCHAR(20), @Notes NVARCHAR(500) = NULL, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Current NVARCHAR(20) = (SELECT Status FROM dbo.CalibrationSchedules WHERE ScheduleId = @ScheduleId);
    IF @Current IS NULL THROW 50001, N'Schedule not found.', 1;
    IF @Status NOT IN (N'Planned', N'Completed', N'Cancelled') THROW 50003, N'Invalid status.', 1;
    IF @Status = N'Completed' AND @Current <> N'Completed'
        THROW 50004, N'A schedule becomes Completed only by recording a calibration result for it.', 1;
    BEGIN TRAN;
        UPDATE dbo.CalibrationSchedules
        SET PlanYear = @PlanYear, DueDate = @DueDate, AssignedTo = @AssignedTo, Status = @Status, Notes = @Notes,
            UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
        WHERE ScheduleId = @ScheduleId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Update', N'Schedule', @ScheduleId, @Status;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Schedules_Delete @ScheduleId INT, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.CalibrationSchedules WHERE ScheduleId = @ScheduleId) THROW 50001, N'Schedule not found.', 1;
    IF EXISTS (SELECT 1 FROM dbo.CalibrationResults WHERE ScheduleId = @ScheduleId)
        THROW 50004, N'This schedule has calibration results linked to it. Cancel it instead of deleting.', 1;
    BEGIN TRAN;
        DELETE FROM dbo.CalibrationSchedules WHERE ScheduleId = @ScheduleId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Delete', N'Schedule', @ScheduleId, NULL;
    COMMIT;
END
GO

/* -------------------------------------------------------------- RESULTS */
CREATE OR ALTER PROCEDURE dbo.usp_Results_Search
    @EquipmentId INT = NULL, @Result NVARCHAR(12) = NULL, @AgencyId INT = NULL, @From DATE = NULL, @To DATE = NULL,
    @Search NVARCHAR(100) = NULL, @Page INT = 1, @PageSize INT = 25
AS
BEGIN
    SET NOCOUNT ON;
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 25;
    IF @PageSize > 200 SET @PageSize = 200;
    DECLARE @Like NVARCHAR(210) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @Like = N'%' + REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Search)), N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
    SELECT r.ResultId, r.EquipmentId, e.EquipmentCode, e.EquipmentName, r.ScheduleId, r.CalibrationDate, r.CertificateReference,
           r.AgencyId, a.Name AS AgencyName, r.ErrorDetails, r.Result, r.ActionTaken, r.DueDate, r.Remarks, r.SignedBy,
           COUNT(*) OVER () AS TotalCount
    FROM dbo.CalibrationResults r
    JOIN dbo.Equipment e ON e.EquipmentId = r.EquipmentId AND e.IsDeleted = 0
    LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = r.AgencyId
    WHERE (@EquipmentId IS NULL OR r.EquipmentId = @EquipmentId)
      AND (@Result IS NULL OR r.Result = @Result)
      AND (@AgencyId IS NULL OR r.AgencyId = @AgencyId)
      AND (@From IS NULL OR r.CalibrationDate >= @From)
      AND (@To IS NULL OR r.CalibrationDate <= @To)
      AND (@Like IS NULL OR e.EquipmentCode LIKE @Like OR e.EquipmentName LIKE @Like OR r.CertificateReference LIKE @Like)
    ORDER BY r.CalibrationDate DESC, r.ResultId DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Results_GetById @ResultId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.ResultId, r.EquipmentId, e.EquipmentCode, e.EquipmentName, r.ScheduleId, r.CalibrationDate, r.CertificateReference,
           r.AgencyId, a.Name AS AgencyName, r.ErrorDetails, r.Result, r.ActionTaken, r.DueDate, r.Remarks, r.SignedBy
    FROM dbo.CalibrationResults r
    JOIN dbo.Equipment e ON e.EquipmentId = r.EquipmentId
    LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = r.AgencyId
    WHERE r.ResultId = @ResultId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Results_Create
    @EquipmentId INT, @ScheduleId INT = NULL, @CalibrationDate DATE, @CertificateReference NVARCHAR(100) = NULL,
    @AgencyId INT = NULL, @ErrorDetails NVARCHAR(500) = NULL, @Result NVARCHAR(12), @ActionTaken NVARCHAR(500) = NULL,
    @DueDate DATE, @Remarks NVARCHAR(500) = NULL, @SignedBy NVARCHAR(100) = NULL, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Responsible NVARCHAR(100);
    SELECT @Responsible = ResponsiblePerson FROM dbo.Equipment WHERE EquipmentId = @EquipmentId AND IsDeleted = 0;
    IF NOT EXISTS (SELECT 1 FROM dbo.Equipment WHERE EquipmentId = @EquipmentId AND IsDeleted = 0) THROW 50003, N'Equipment does not exist.', 1;
    IF @Result NOT IN (N'OK', N'NOTOK', N'Unverified') THROW 50003, N'Result must be OK, NOTOK or Unverified.', 1;
    IF @Result = N'NOTOK' AND LEN(ISNULL(LTRIM(RTRIM(@ActionTaken)), N'')) = 0 THROW 50003, N'Action taken is required when the result is NOTOK.', 1;
    IF @DueDate < @CalibrationDate THROW 50003, N'Due date cannot be earlier than the calibration date.', 1;
    IF @AgencyId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationAgencies WHERE AgencyId = @AgencyId) THROW 50003, N'Calibration agency does not exist.', 1;
    IF @ScheduleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationSchedules WHERE ScheduleId = @ScheduleId AND EquipmentId = @EquipmentId AND Status = N'Planned')
        THROW 50003, N'The selected schedule does not belong to this equipment or is not in Planned status.', 1;
    BEGIN TRAN;
        INSERT dbo.CalibrationResults (EquipmentId, ScheduleId, CalibrationDate, CertificateReference, AgencyId, ErrorDetails,
                                       Result, ActionTaken, DueDate, Remarks, SignedBy, CreatedBy)
        VALUES (@EquipmentId, @ScheduleId, @CalibrationDate, @CertificateReference, @AgencyId, @ErrorDetails,
                @Result, @ActionTaken, @DueDate, @Remarks, @SignedBy, @ActorUserId);
        DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);

        IF @ScheduleId IS NOT NULL
            UPDATE dbo.CalibrationSchedules SET Status = N'Completed', CompletedAt = SYSUTCDATETIME(),
                   UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
            WHERE ScheduleId = @ScheduleId;

        -- Plan the next calibration automatically when this one passed.
        IF @Result = N'OK' AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationSchedules WHERE EquipmentId = @EquipmentId AND DueDate = @DueDate AND Status = N'Planned')
            INSERT dbo.CalibrationSchedules (EquipmentId, PlanYear, DueDate, AssignedTo, Notes, CreatedBy)
            VALUES (@EquipmentId, YEAR(@DueDate), @DueDate, @Responsible, N'Auto-planned from calibration result ' + CAST(@NewId AS NVARCHAR(20)), @ActorUserId);

        EXEC dbo.usp_Audit_Write @ActorUserId, N'Create', N'CalibrationResult', @NewId, @Result;
    COMMIT;
    SELECT @NewId AS ResultId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Results_Update
    @ResultId INT, @CalibrationDate DATE, @CertificateReference NVARCHAR(100) = NULL, @AgencyId INT = NULL,
    @ErrorDetails NVARCHAR(500) = NULL, @Result NVARCHAR(12), @ActionTaken NVARCHAR(500) = NULL,
    @DueDate DATE, @Remarks NVARCHAR(500) = NULL, @SignedBy NVARCHAR(100) = NULL, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.CalibrationResults WHERE ResultId = @ResultId) THROW 50001, N'Calibration result not found.', 1;
    IF @Result NOT IN (N'OK', N'NOTOK', N'Unverified') THROW 50003, N'Result must be OK, NOTOK or Unverified.', 1;
    IF @Result = N'NOTOK' AND LEN(ISNULL(LTRIM(RTRIM(@ActionTaken)), N'')) = 0 THROW 50003, N'Action taken is required when the result is NOTOK.', 1;
    IF @DueDate < @CalibrationDate THROW 50003, N'Due date cannot be earlier than the calibration date.', 1;
    IF @AgencyId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationAgencies WHERE AgencyId = @AgencyId) THROW 50003, N'Calibration agency does not exist.', 1;
    BEGIN TRAN;
        UPDATE dbo.CalibrationResults
        SET CalibrationDate = @CalibrationDate, CertificateReference = @CertificateReference, AgencyId = @AgencyId,
            ErrorDetails = @ErrorDetails, Result = @Result, ActionTaken = @ActionTaken, DueDate = @DueDate,
            Remarks = @Remarks, SignedBy = @SignedBy, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
        WHERE ResultId = @ResultId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Update', N'CalibrationResult', @ResultId, @Result;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Results_Delete @ResultId INT, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @ScheduleId INT, @Found BIT = 0;
    SELECT @ScheduleId = ScheduleId, @Found = 1 FROM dbo.CalibrationResults WHERE ResultId = @ResultId;
    IF @Found = 0 THROW 50001, N'Calibration result not found.', 1;
    BEGIN TRAN;
        UPDATE dbo.Certificates SET ResultId = NULL WHERE ResultId = @ResultId;
        IF @ScheduleId IS NOT NULL
            UPDATE dbo.CalibrationSchedules SET Status = N'Planned', CompletedAt = NULL, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
            WHERE ScheduleId = @ScheduleId AND Status = N'Completed';
        DELETE FROM dbo.CalibrationResults WHERE ResultId = @ResultId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Delete', N'CalibrationResult', @ResultId, NULL;
    COMMIT;
END
GO

/* -------------------------------------------------------------- HISTORY */
CREATE OR ALTER PROCEDURE dbo.usp_History_GetByEquipment @EquipmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT v.EquipmentId, v.EquipmentCode, v.SourceSerialNo, v.EquipmentName, v.RangeLeastCount, v.Make, v.SerialNo,
           v.AcceptableErrorLimit, v.DepartmentId, v.DepartmentName, v.Location, v.CalibrationType, v.AgencyId, v.AgencyName,
           v.FrequencyId, v.FrequencyLabel, v.FrequencyMonths, v.ResponsiblePerson, v.GroupName, v.Status, v.Remarks,
           v.LastCalibrationDate, v.NextDueDate, v.CalibrationStatus, v.CodeCount, v.CreatedAt, v.UpdatedAt
    FROM dbo.vw_EquipmentFull v WHERE v.EquipmentId = @EquipmentId AND v.IsDeleted = 0;

    SELECT CAST(ROW_NUMBER() OVER (ORDER BY r.CalibrationDate, r.ResultId) AS INT) AS SlNo,
           r.ResultId, r.EquipmentId, e.EquipmentCode, e.EquipmentName, r.ScheduleId, r.CalibrationDate, r.CertificateReference,
           r.AgencyId, a.Name AS AgencyName, r.ErrorDetails, r.Result, r.ActionTaken, r.DueDate, r.Remarks, r.SignedBy
    FROM dbo.CalibrationResults r
    JOIN dbo.Equipment e ON e.EquipmentId = r.EquipmentId
    LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = r.AgencyId
    WHERE r.EquipmentId = @EquipmentId
    ORDER BY r.CalibrationDate, r.ResultId;
END
GO

/* --------------------------------------------------------- CERTIFICATES */
CREATE OR ALTER PROCEDURE dbo.usp_Certificates_Search
    @EquipmentId INT = NULL, @Search NVARCHAR(100) = NULL, @ExpiringWithinDays INT = NULL,
    @Page INT = 1, @PageSize INT = 25
AS
BEGIN
    SET NOCOUNT ON;
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 25;
    IF @PageSize > 200 SET @PageSize = 200;
    DECLARE @Today DATE = CAST(SYSDATETIME() AS DATE);
    DECLARE @Like NVARCHAR(210) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @Like = N'%' + REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Search)), N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
    SELECT c.CertificateId, c.EquipmentId, e.EquipmentCode, e.EquipmentName, c.ResultId, c.CertificateNumber, c.AgencyId,
           a.Name AS AgencyName, c.IssueDate, c.ExpiryDate, c.FileName, c.ContentType, c.FileSizeBytes, c.StorageLocation, c.Notes,
           COUNT(*) OVER () AS TotalCount
    FROM dbo.Certificates c
    JOIN dbo.Equipment e ON e.EquipmentId = c.EquipmentId AND e.IsDeleted = 0
    LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = c.AgencyId
    WHERE (@EquipmentId IS NULL OR c.EquipmentId = @EquipmentId)
      AND (@Like IS NULL OR c.CertificateNumber LIKE @Like OR e.EquipmentCode LIKE @Like OR e.EquipmentName LIKE @Like OR c.FileName LIKE @Like)
      AND (@ExpiringWithinDays IS NULL OR (c.ExpiryDate IS NOT NULL AND c.ExpiryDate <= DATEADD(DAY, @ExpiringWithinDays, @Today)))
    ORDER BY c.IssueDate DESC, c.CertificateId DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Certificates_GetById @CertificateId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.CertificateId, c.EquipmentId, e.EquipmentCode, e.EquipmentName, c.ResultId, c.CertificateNumber, c.AgencyId,
           a.Name AS AgencyName, c.IssueDate, c.ExpiryDate, c.FileName, c.ContentType, c.FileSizeBytes, c.StorageLocation, c.Notes
    FROM dbo.Certificates c
    JOIN dbo.Equipment e ON e.EquipmentId = c.EquipmentId
    LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = c.AgencyId
    WHERE c.CertificateId = @CertificateId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Certificates_Create
    @EquipmentId INT, @ResultId INT = NULL, @CertificateNumber NVARCHAR(100), @AgencyId INT = NULL,
    @IssueDate DATE, @ExpiryDate DATE = NULL, @FileName NVARCHAR(260) = NULL, @ContentType NVARCHAR(100) = NULL,
    @FileSizeBytes BIGINT = NULL, @StorageLocation NVARCHAR(500) = NULL, @Notes NVARCHAR(500) = NULL, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Equipment WHERE EquipmentId = @EquipmentId AND IsDeleted = 0) THROW 50003, N'Equipment does not exist.', 1;
    IF @ResultId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationResults WHERE ResultId = @ResultId AND EquipmentId = @EquipmentId)
        THROW 50003, N'The selected calibration result does not belong to this equipment.', 1;
    IF @AgencyId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationAgencies WHERE AgencyId = @AgencyId) THROW 50003, N'Calibration agency does not exist.', 1;
    IF @ExpiryDate IS NOT NULL AND @ExpiryDate < @IssueDate THROW 50003, N'Expiry date cannot be earlier than the issue date.', 1;
    BEGIN TRAN;
        INSERT dbo.Certificates (EquipmentId, ResultId, CertificateNumber, AgencyId, IssueDate, ExpiryDate, FileName, ContentType,
                                 FileSizeBytes, StorageLocation, Notes, CreatedBy)
        VALUES (@EquipmentId, @ResultId, LTRIM(RTRIM(@CertificateNumber)), @AgencyId, @IssueDate, @ExpiryDate, @FileName, @ContentType,
                @FileSizeBytes, @StorageLocation, @Notes, @ActorUserId);
        DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Create', N'Certificate', @NewId, @CertificateNumber;
    COMMIT;
    SELECT @NewId AS CertificateId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Certificates_Update
    @CertificateId INT, @ResultId INT = NULL, @CertificateNumber NVARCHAR(100), @AgencyId INT = NULL,
    @IssueDate DATE, @ExpiryDate DATE = NULL, @FileName NVARCHAR(260) = NULL, @ContentType NVARCHAR(100) = NULL,
    @FileSizeBytes BIGINT = NULL, @StorageLocation NVARCHAR(500) = NULL, @Notes NVARCHAR(500) = NULL, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @EquipmentId INT = (SELECT EquipmentId FROM dbo.Certificates WHERE CertificateId = @CertificateId);
    IF @EquipmentId IS NULL THROW 50001, N'Certificate not found.', 1;
    IF @ResultId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationResults WHERE ResultId = @ResultId AND EquipmentId = @EquipmentId)
        THROW 50003, N'The selected calibration result does not belong to this equipment.', 1;
    IF @AgencyId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CalibrationAgencies WHERE AgencyId = @AgencyId) THROW 50003, N'Calibration agency does not exist.', 1;
    IF @ExpiryDate IS NOT NULL AND @ExpiryDate < @IssueDate THROW 50003, N'Expiry date cannot be earlier than the issue date.', 1;
    BEGIN TRAN;
        UPDATE dbo.Certificates
        SET ResultId = @ResultId, CertificateNumber = LTRIM(RTRIM(@CertificateNumber)), AgencyId = @AgencyId, IssueDate = @IssueDate,
            ExpiryDate = @ExpiryDate, FileName = @FileName, ContentType = @ContentType, FileSizeBytes = @FileSizeBytes,
            StorageLocation = @StorageLocation, Notes = @Notes, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
        WHERE CertificateId = @CertificateId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Update', N'Certificate', @CertificateId, @CertificateNumber;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Certificates_Delete @CertificateId INT, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Certificates WHERE CertificateId = @CertificateId) THROW 50001, N'Certificate not found.', 1;
    BEGIN TRAN;
        DELETE FROM dbo.Certificates WHERE CertificateId = @CertificateId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Delete', N'Certificate', @CertificateId, NULL;
    COMMIT;
END
GO

/* ---------------------------------------------------------- MAINTENANCE */
CREATE OR ALTER PROCEDURE dbo.usp_Maintenance_Search
    @EquipmentId INT = NULL, @Status NVARCHAR(20) = NULL, @MaintenanceType NVARCHAR(20) = NULL,
    @Search NVARCHAR(100) = NULL, @Page INT = 1, @PageSize INT = 25
AS
BEGIN
    SET NOCOUNT ON;
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 25;
    IF @PageSize > 200 SET @PageSize = 200;
    DECLARE @Like NVARCHAR(210) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @Like = N'%' + REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Search)), N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
    SELECT m.MaintenanceId, m.EquipmentId, e.EquipmentCode, e.EquipmentName, m.MaintenanceType, m.MaintenanceDate, m.PerformedBy,
           m.Description, m.Cost, m.NextMaintenanceDate, m.Status, COUNT(*) OVER () AS TotalCount
    FROM dbo.MaintenanceRecords m
    JOIN dbo.Equipment e ON e.EquipmentId = m.EquipmentId AND e.IsDeleted = 0
    WHERE (@EquipmentId IS NULL OR m.EquipmentId = @EquipmentId)
      AND (@Status IS NULL OR m.Status = @Status)
      AND (@MaintenanceType IS NULL OR m.MaintenanceType = @MaintenanceType)
      AND (@Like IS NULL OR e.EquipmentCode LIKE @Like OR e.EquipmentName LIKE @Like OR m.Description LIKE @Like OR m.PerformedBy LIKE @Like)
    ORDER BY m.MaintenanceDate DESC, m.MaintenanceId DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Maintenance_GetById @MaintenanceId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MaintenanceId, m.EquipmentId, e.EquipmentCode, e.EquipmentName, m.MaintenanceType, m.MaintenanceDate, m.PerformedBy,
           m.Description, m.Cost, m.NextMaintenanceDate, m.Status
    FROM dbo.MaintenanceRecords m JOIN dbo.Equipment e ON e.EquipmentId = m.EquipmentId
    WHERE m.MaintenanceId = @MaintenanceId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Maintenance_Create
    @EquipmentId INT, @MaintenanceType NVARCHAR(20), @MaintenanceDate DATE, @PerformedBy NVARCHAR(100) = NULL,
    @Description NVARCHAR(1000), @Cost DECIMAL(12,2) = NULL, @NextMaintenanceDate DATE = NULL,
    @Status NVARCHAR(20) = N'Open', @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Equipment WHERE EquipmentId = @EquipmentId AND IsDeleted = 0) THROW 50003, N'Equipment does not exist.', 1;
    IF @NextMaintenanceDate IS NOT NULL AND @NextMaintenanceDate < @MaintenanceDate THROW 50003, N'Next maintenance date cannot be earlier than the maintenance date.', 1;
    BEGIN TRAN;
        INSERT dbo.MaintenanceRecords (EquipmentId, MaintenanceType, MaintenanceDate, PerformedBy, Description, Cost, NextMaintenanceDate, Status, CreatedBy)
        VALUES (@EquipmentId, @MaintenanceType, @MaintenanceDate, @PerformedBy, @Description, @Cost, @NextMaintenanceDate, @Status, @ActorUserId);
        DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Create', N'Maintenance', @NewId, @MaintenanceType;
    COMMIT;
    SELECT @NewId AS MaintenanceId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Maintenance_Update
    @MaintenanceId INT, @MaintenanceType NVARCHAR(20), @MaintenanceDate DATE, @PerformedBy NVARCHAR(100) = NULL,
    @Description NVARCHAR(1000), @Cost DECIMAL(12,2) = NULL, @NextMaintenanceDate DATE = NULL,
    @Status NVARCHAR(20), @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.MaintenanceRecords WHERE MaintenanceId = @MaintenanceId) THROW 50001, N'Maintenance record not found.', 1;
    IF @NextMaintenanceDate IS NOT NULL AND @NextMaintenanceDate < @MaintenanceDate THROW 50003, N'Next maintenance date cannot be earlier than the maintenance date.', 1;
    BEGIN TRAN;
        UPDATE dbo.MaintenanceRecords
        SET MaintenanceType = @MaintenanceType, MaintenanceDate = @MaintenanceDate, PerformedBy = @PerformedBy, Description = @Description,
            Cost = @Cost, NextMaintenanceDate = @NextMaintenanceDate, Status = @Status, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ActorUserId
        WHERE MaintenanceId = @MaintenanceId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Update', N'Maintenance', @MaintenanceId, @Status;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Maintenance_Delete @MaintenanceId INT, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.MaintenanceRecords WHERE MaintenanceId = @MaintenanceId) THROW 50001, N'Maintenance record not found.', 1;
    BEGIN TRAN;
        DELETE FROM dbo.MaintenanceRecords WHERE MaintenanceId = @MaintenanceId;
        EXEC dbo.usp_Audit_Write @ActorUserId, N'Delete', N'Maintenance', @MaintenanceId, NULL;
    COMMIT;
END
GO

/* --------------------------------------------------------- NOTIFICATIONS */
CREATE OR ALTER PROCEDURE dbo.usp_Notifications_Generate
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSDATETIME() AS DATE);
    INSERT dbo.Notifications (UserId, EquipmentId, NotificationType, Title, Message, DedupeKey)
    SELECT u.UserId, v.EquipmentId,
           CASE v.CalibrationStatus WHEN N'Overdue' THEN N'Overdue' WHEN N'Failed' THEN N'Failed' ELSE N'DueSoon' END,
           CASE v.CalibrationStatus WHEN N'Overdue' THEN N'Calibration overdue: ' WHEN N'Failed' THEN N'Calibration failed: ' ELSE N'Calibration due soon: ' END + v.EquipmentCode,
           v.EquipmentName + CASE v.CalibrationStatus
               WHEN N'Failed' THEN N' failed its last calibration and needs corrective action.'
               WHEN N'Overdue' THEN N' was due on ' + CONVERT(NVARCHAR(10), v.NextDueDate, 23) + N' (' + CAST(DATEDIFF(DAY, v.NextDueDate, @Today) AS NVARCHAR(10)) + N' days overdue).'
               ELSE N' is due on ' + CONVERT(NVARCHAR(10), v.NextDueDate, 23) + N' (in ' + CAST(DATEDIFF(DAY, @Today, v.NextDueDate) AS NVARCHAR(10)) + N' days).' END,
           CONCAT(v.CalibrationStatus, N':', v.EquipmentId, N':', CONVERT(NVARCHAR(10), v.NextDueDate, 23))
    FROM dbo.vw_EquipmentFull v
    CROSS JOIN dbo.Users u
    WHERE v.IsDeleted = 0 AND v.Status = N'Active' AND u.IsActive = 1
      AND v.CalibrationStatus IN (N'DueSoon', N'Overdue', N'Failed')
      AND NOT EXISTS (SELECT 1 FROM dbo.Notifications n
                      WHERE n.UserId = u.UserId
                        AND n.DedupeKey = CONCAT(v.CalibrationStatus, N':', v.EquipmentId, N':', CONVERT(NVARCHAR(10), v.NextDueDate, 23)));
    SELECT @@ROWCOUNT AS Created;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notifications_List
    @UserId INT, @UnreadOnly BIT = 0, @Page INT = 1, @PageSize INT = 25
AS
BEGIN
    SET NOCOUNT ON;
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 25;
    IF @PageSize > 200 SET @PageSize = 200;
    SELECT n.NotificationId, n.EquipmentId, n.NotificationType, n.Title, n.Message, n.IsRead, n.CreatedAt, n.ReadAt,
           COUNT(*) OVER () AS TotalCount
    FROM dbo.Notifications n
    WHERE n.UserId = @UserId AND (@UnreadOnly = 0 OR n.IsRead = 0)
    ORDER BY n.IsRead, n.CreatedAt DESC, n.NotificationId DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notifications_UnreadCount @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS UnreadCount FROM dbo.Notifications WHERE UserId = @UserId AND IsRead = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notifications_MarkRead @UserId INT, @NotificationId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Notifications WHERE NotificationId = @NotificationId AND UserId = @UserId) THROW 50001, N'Notification not found.', 1;
    UPDATE dbo.Notifications SET IsRead = 1, ReadAt = SYSUTCDATETIME() WHERE NotificationId = @NotificationId AND UserId = @UserId AND IsRead = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notifications_MarkAllRead @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Notifications SET IsRead = 1, ReadAt = SYSUTCDATETIME() WHERE UserId = @UserId AND IsRead = 0;
    SELECT @@ROWCOUNT AS Updated;
END
GO

/* ------------------------------------------------------------ DASHBOARD */
CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_Summary @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSDATETIME() AS DATE);
    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);

    -- 1) headline metrics
    SELECT
        COUNT(*)                                                                    AS TotalEquipment,
        ISNULL(SUM(CASE WHEN CalibrationStatus = N'Valid' THEN 1 ELSE 0 END), 0)               AS ValidCount,
        ISNULL(SUM(CASE WHEN CalibrationStatus = N'DueSoon' THEN 1 ELSE 0 END), 0)             AS DueSoonCount,
        ISNULL(SUM(CASE WHEN CalibrationStatus = N'Overdue' THEN 1 ELSE 0 END), 0)             AS OverdueCount,
        ISNULL(SUM(CASE WHEN CalibrationStatus = N'Failed' THEN 1 ELSE 0 END), 0)              AS FailedCount,
        ISNULL(SUM(CASE WHEN CalibrationStatus = N'NotCalibrated' THEN 1 ELSE 0 END), 0)       AS NotCalibratedCount,
        (SELECT COUNT(*) FROM dbo.CalibrationSchedules s JOIN dbo.Equipment e2 ON e2.EquipmentId = s.EquipmentId AND e2.IsDeleted = 0
          WHERE s.Status = N'Planned' AND s.DueDate >= @MonthStart AND s.DueDate < DATEADD(MONTH, 1, @MonthStart)) AS PlannedThisMonth,
        (SELECT COUNT(*) FROM dbo.MaintenanceRecords WHERE Status IN (N'Open', N'InProgress')) AS OpenMaintenance,
        (SELECT COUNT(*) FROM dbo.Certificates WHERE ExpiryDate IS NOT NULL AND ExpiryDate <= DATEADD(DAY, 30, @Today)) AS ExpiringCertificates,
        (SELECT COUNT(*) FROM dbo.Notifications WHERE UserId = @UserId AND IsRead = 0) AS UnreadNotifications
    FROM dbo.vw_EquipmentFull WHERE IsDeleted = 0 AND Status = N'Active';

    -- 2) by department (top 10 by problem count)
    SELECT TOP (10) ISNULL(DepartmentName, N'(none)') AS DepartmentName, COUNT(*) AS Total,
           SUM(CASE WHEN CalibrationStatus = N'Overdue' THEN 1 ELSE 0 END) AS Overdue,
           SUM(CASE WHEN CalibrationStatus = N'DueSoon' THEN 1 ELSE 0 END) AS DueSoon
    FROM dbo.vw_EquipmentFull WHERE IsDeleted = 0 AND Status = N'Active'
    GROUP BY DepartmentName
    ORDER BY SUM(CASE WHEN CalibrationStatus IN (N'Overdue', N'DueSoon') THEN 1 ELSE 0 END) DESC, COUNT(*) DESC;

    -- 3) calibrations falling due, next 12 months
    ;WITH n AS (SELECT 0 AS i UNION ALL SELECT i + 1 FROM n WHERE i < 11)
    SELECT CONVERT(NVARCHAR(7), DATEADD(MONTH, n.i, @MonthStart), 126) AS MonthKey,
           (SELECT COUNT(*) FROM dbo.vw_EquipmentFull v
             WHERE v.IsDeleted = 0 AND v.Status = N'Active' AND v.NextDueDate >= DATEADD(MONTH, n.i, @MonthStart)
               AND v.NextDueDate < DATEADD(MONTH, n.i + 1, @MonthStart)) AS DueCount
    FROM n ORDER BY n.i;

    -- 4) recent calibration results
    SELECT TOP (6) r.ResultId, e.EquipmentId, e.EquipmentCode, e.EquipmentName, r.CalibrationDate, r.Result
    FROM dbo.CalibrationResults r JOIN dbo.Equipment e ON e.EquipmentId = r.EquipmentId AND e.IsDeleted = 0
    ORDER BY r.CalibrationDate DESC, r.ResultId DESC;

    -- 5) upcoming / problem items
    SELECT TOP (10) EquipmentId, EquipmentCode, EquipmentName, DepartmentName, NextDueDate, CalibrationStatus
    FROM dbo.vw_EquipmentFull
    WHERE IsDeleted = 0 AND Status = N'Active' AND CalibrationStatus IN (N'DueSoon', N'Overdue', N'Failed')
    ORDER BY CASE CalibrationStatus WHEN N'Failed' THEN 0 WHEN N'Overdue' THEN 1 ELSE 2 END, NextDueDate, EquipmentCode;
END
GO

/* -------------------------------------------------------------- REPORTS */
CREATE OR ALTER PROCEDURE dbo.usp_Reports_Calibration
    @ReportType NVARCHAR(20) = N'all',            -- all | due | overdue | failed | notcalibrated
    @From DATE = NULL, @To DATE = NULL, @DepartmentId INT = NULL, @AgencyId INT = NULL,
    @Search NVARCHAR(100) = NULL, @Page INT = 1, @PageSize INT = 25      -- @PageSize = 0 returns up to 10000 rows (export)
AS
BEGIN
    SET NOCOUNT ON;
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 0 SET @PageSize = 25;
    IF @PageSize = 0 SET @PageSize = 10000;
    IF @PageSize > 10000 SET @PageSize = 10000;
    IF @ReportType NOT IN (N'all', N'due', N'overdue', N'failed', N'notcalibrated') THROW 50003, N'Unknown report type.', 1;
    DECLARE @Today DATE = CAST(SYSDATETIME() AS DATE);
    DECLARE @Like NVARCHAR(210) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @Like = N'%' + REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Search)), N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
    SELECT v.EquipmentId, v.EquipmentCode, v.EquipmentName, v.RangeLeastCount, v.Make, v.DepartmentName, v.AgencyName, v.FrequencyLabel,
           v.ResponsiblePerson, v.LastCalibrationDate, v.NextDueDate, v.CalibrationStatus,
           CASE WHEN v.NextDueDate IS NULL THEN NULL ELSE DATEDIFF(DAY, @Today, v.NextDueDate) END AS DaysToDue,
           COUNT(*) OVER () AS TotalCount
    FROM dbo.vw_EquipmentFull v
    WHERE v.IsDeleted = 0 AND v.Status = N'Active'
      AND (@ReportType = N'all'
           OR (@ReportType = N'due' AND v.CalibrationStatus IN (N'DueSoon', N'Overdue'))
           OR (@ReportType = N'overdue' AND v.CalibrationStatus = N'Overdue')
           OR (@ReportType = N'failed' AND v.CalibrationStatus = N'Failed')
           OR (@ReportType = N'notcalibrated' AND v.CalibrationStatus = N'NotCalibrated'))
      AND (@From IS NULL OR v.NextDueDate >= @From)
      AND (@To IS NULL OR v.NextDueDate <= @To)
      AND (@DepartmentId IS NULL OR v.DepartmentId = @DepartmentId)
      AND (@AgencyId IS NULL OR v.AgencyId = @AgencyId)
      AND (@Like IS NULL OR v.EquipmentCode LIKE @Like OR v.EquipmentName LIKE @Like OR v.ResponsiblePerson LIKE @Like)
    ORDER BY CASE WHEN v.NextDueDate IS NULL THEN 1 ELSE 0 END, v.NextDueDate, v.EquipmentCode
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Reports_AgencySummary @From DATE = NULL, @To DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(a.Name, N'(no agency)') AS AgencyName,
           COUNT(*) AS TotalCalibrations,
           SUM(CASE WHEN r.Result = N'OK' THEN 1 ELSE 0 END) AS OkCount,
           SUM(CASE WHEN r.Result = N'NOTOK' THEN 1 ELSE 0 END) AS NotOkCount,
           SUM(CASE WHEN r.Result = N'Unverified' THEN 1 ELSE 0 END) AS UnverifiedCount,
           MIN(r.CalibrationDate) AS FirstCalibration, MAX(r.CalibrationDate) AS LastCalibration
    FROM dbo.CalibrationResults r
    JOIN dbo.Equipment e ON e.EquipmentId = r.EquipmentId AND e.IsDeleted = 0
    LEFT JOIN dbo.CalibrationAgencies a ON a.AgencyId = r.AgencyId
    WHERE (@From IS NULL OR r.CalibrationDate >= @From) AND (@To IS NULL OR r.CalibrationDate <= @To)
    GROUP BY a.Name
    ORDER BY COUNT(*) DESC, a.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Reports_DataQuality @OnlyOpen BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IssueId, SourceFile, SourceRow, IssueType, Severity, EquipmentCode, EquipmentId, Details, IsResolved
    FROM dbo.DataQualityIssues
    WHERE @OnlyOpen = 0 OR IsResolved = 0
    ORDER BY CASE Severity WHEN N'Error' THEN 0 WHEN N'Warning' THEN 1 ELSE 2 END, IssueType, SourceFile, SourceRow;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DataQuality_Resolve @IssueId INT, @IsResolved BIT, @ActorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.DataQualityIssues WHERE IssueId = @IssueId) THROW 50001, N'Issue not found.', 1;
    UPDATE dbo.DataQualityIssues SET IsResolved = @IsResolved WHERE IssueId = @IssueId;
    EXEC dbo.usp_Audit_Write @ActorUserId, N'Update', N'DataQualityIssue', @IssueId, NULL;
END
GO
