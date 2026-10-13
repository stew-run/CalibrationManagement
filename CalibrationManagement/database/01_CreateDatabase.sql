/* ============================================================================
   01_CreateDatabase.sql  -  Calibration Management System
   Creates the database if it does not exist. Safe to re-run.
   Run in SSMS (connected to the SQL Server instance, any database).
   ============================================================================ */
IF DB_ID(N'CalibrationManagement') IS NULL
BEGIN
    CREATE DATABASE [CalibrationManagement] COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Database CalibrationManagement created.';
END
ELSE
    PRINT 'Database CalibrationManagement already exists - nothing to do.';
GO
ALTER DATABASE [CalibrationManagement] SET RECOVERY SIMPLE;   -- development default; change for production backups
GO
