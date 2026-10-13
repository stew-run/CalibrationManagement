/* ============================================================================
   03_CreateIndexes.sql  -  Re-runnable (each index is created only if missing)
   ============================================================================ */
USE [CalibrationManagement];
GO
SET QUOTED_IDENTIFIER ON;   -- required by filtered indexes; stored in each procedure's metadata
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Equipment_SourceSerialNo' AND object_id = OBJECT_ID(N'dbo.Equipment'))
    CREATE UNIQUE INDEX UX_Equipment_SourceSerialNo ON dbo.Equipment(SourceSerialNo) WHERE SourceSerialNo IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Equipment_Code' AND object_id = OBJECT_ID(N'dbo.Equipment'))
    CREATE INDEX IX_Equipment_Code ON dbo.Equipment(EquipmentCode) INCLUDE (EquipmentName, IsDeleted);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Equipment_Department' AND object_id = OBJECT_ID(N'dbo.Equipment'))
    CREATE INDEX IX_Equipment_Department ON dbo.Equipment(DepartmentId) WHERE IsDeleted = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Equipment_Agency' AND object_id = OBJECT_ID(N'dbo.Equipment'))
    CREATE INDEX IX_Equipment_Agency ON dbo.Equipment(AgencyId) WHERE IsDeleted = 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schedules_Due' AND object_id = OBJECT_ID(N'dbo.CalibrationSchedules'))
    CREATE INDEX IX_Schedules_Due ON dbo.CalibrationSchedules(DueDate, Status) INCLUDE (EquipmentId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Schedules_Equipment' AND object_id = OBJECT_ID(N'dbo.CalibrationSchedules'))
    CREATE INDEX IX_Schedules_Equipment ON dbo.CalibrationSchedules(EquipmentId, PlanYear);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Results_EquipmentDate' AND object_id = OBJECT_ID(N'dbo.CalibrationResults'))
    CREATE INDEX IX_Results_EquipmentDate ON dbo.CalibrationResults(EquipmentId, CalibrationDate DESC, ResultId DESC) INCLUDE (DueDate, Result);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Results_DueDate' AND object_id = OBJECT_ID(N'dbo.CalibrationResults'))
    CREATE INDEX IX_Results_DueDate ON dbo.CalibrationResults(DueDate);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_Equipment' AND object_id = OBJECT_ID(N'dbo.Certificates'))
    CREATE INDEX IX_Certificates_Equipment ON dbo.Certificates(EquipmentId, IssueDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_Number' AND object_id = OBJECT_ID(N'dbo.Certificates'))
    CREATE INDEX IX_Certificates_Number ON dbo.Certificates(CertificateNumber);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Maintenance_Equipment' AND object_id = OBJECT_ID(N'dbo.MaintenanceRecords'))
    CREATE INDEX IX_Maintenance_Equipment ON dbo.MaintenanceRecords(EquipmentId, MaintenanceDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Maintenance_Status' AND object_id = OBJECT_ID(N'dbo.MaintenanceRecords'))
    CREATE INDEX IX_Maintenance_Status ON dbo.MaintenanceRecords(Status, NextMaintenanceDate);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notifications_User' AND object_id = OBJECT_ID(N'dbo.Notifications'))
    CREATE INDEX IX_Notifications_User ON dbo.Notifications(UserId, IsRead, CreatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Notifications_Dedupe' AND object_id = OBJECT_ID(N'dbo.Notifications'))
    CREATE UNIQUE INDEX UX_Notifications_Dedupe ON dbo.Notifications(UserId, DedupeKey) WHERE DedupeKey IS NOT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_Entity' AND object_id = OBJECT_ID(N'dbo.AuditLog'))
    CREATE INDEX IX_AuditLog_Entity ON dbo.AuditLog(EntityType, EntityId, CreatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DQI_Type' AND object_id = OBJECT_ID(N'dbo.DataQualityIssues'))
    CREATE INDEX IX_DQI_Type ON dbo.DataQualityIssues(IssueType, IsResolved);
GO
