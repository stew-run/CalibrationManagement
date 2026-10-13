/* ============================================================================
   05_SeedReferenceData.sql  -  Roles and calibration frequencies. Re-runnable.
   No users and no passwords are created here (see 06_CreateInitialAdmin.sql).
   ============================================================================ */
USE [CalibrationManagement];
GO
SET QUOTED_IDENTIFIER ON;   -- required by filtered indexes; stored in each procedure's metadata
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO
MERGE dbo.Roles AS t
USING (VALUES
    (CAST(1 AS TINYINT), N'Administrator',  N'Full access including user and role management'),
    (CAST(2 AS TINYINT), N'QualityManager', N'Manage equipment, plans, results, certificates, reports; can delete'),
    (CAST(3 AS TINYINT), N'Technician',     N'Record calibrations, maintenance and certificates; cannot delete equipment'),
    (CAST(4 AS TINYINT), N'Viewer',         N'Read-only access')
) AS s (RoleId, RoleName, Description)
ON t.RoleId = s.RoleId
WHEN MATCHED THEN UPDATE SET RoleName = s.RoleName, Description = s.Description
WHEN NOT MATCHED THEN INSERT (RoleId, RoleName, Description) VALUES (s.RoleId, s.RoleName, s.Description);
GO
MERGE dbo.CalibrationFrequencies AS t
USING (VALUES
    (1, N'6 Months', 6), (2, N'1 Year', 12), (3, N'2 Year', 24), (4, N'3 Months', 3), (5, N'3 Year', 36)
) AS s (FrequencyId, Label, Months)
ON t.FrequencyId = s.FrequencyId
WHEN MATCHED THEN UPDATE SET Label = s.Label, Months = s.Months
WHEN NOT MATCHED THEN INSERT (FrequencyId, Label, Months) VALUES (s.FrequencyId, s.Label, s.Months);
GO
PRINT 'Reference data seeded.';
GO
