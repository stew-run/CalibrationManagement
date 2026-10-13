/* ============================================================================
   06_CreateInitialAdmin.sql  -  Pre-flight check for administrator provisioning
   ----------------------------------------------------------------------------
   T-SQL cannot generate a BCrypt hash, and this project never stores or ships a
   default password. The administrator is therefore created by the API's one-time
   command, which prompts for the credentials, hashes the password with BCrypt and
   calls dbo.usp_Users_CreateInitialAdmin (it refuses to run if an administrator
   already exists):

       cd Backend\src\CalibrationManagement.Api
       dotnet run -- create-admin

   This script only verifies that the database is ready for that step.
   ============================================================================ */
USE [CalibrationManagement];
GO
SET NOCOUNT ON;
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Administrator')
    RAISERROR (N'Roles are missing. Run 05_SeedReferenceData.sql first.', 16, 1);
ELSE IF EXISTS (SELECT 1 FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId WHERE r.RoleName = N'Administrator')
    PRINT 'An administrator already exists. Provisioning is complete - nothing more to do.';
ELSE
    PRINT 'Ready. Now run:  dotnet run -- create-admin   (from the API project folder).';
GO
