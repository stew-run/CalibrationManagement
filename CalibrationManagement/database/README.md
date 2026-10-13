# Calibration Management – Database (SQL Server)

Run the scripts **in order** in SQL Server Management Studio (connect with a login that may create databases; click *Execute* / F5 for each file):

| # | Script | Purpose | Re-runnable |
|---|---|---|---|
| 1 | `01_CreateDatabase.sql` | creates database `CalibrationManagement` | yes |
| 2 | `02_CreateTables.sql` | tables, keys, constraints | yes |
| 3 | `03_CreateIndexes.sql` | indexes | yes |
| 4 | `04_CreateStoredProcedures.sql` | 2 views + 50 stored procedures (`CREATE OR ALTER`) | yes |
| 5 | `05_SeedReferenceData.sql` | roles, calibration frequencies | yes |
| 6 | `06_CreateInitialAdmin.sql` | pre-flight check only; the admin is created by the API command (see ../README.md (First run)) | yes |
| 7 | `07_ImportExcelData.sql` | **optional** import of the supplied Excel data + data-quality findings | skips itself if already imported |

Command-line alternative (PowerShell, from this folder; `-I` is required because of filtered indexes, `-b` stops on the first error):

```powershell
foreach ($f in '01_CreateDatabase.sql','02_CreateTables.sql','03_CreateIndexes.sql','04_CreateStoredProcedures.sql','05_SeedReferenceData.sql','07_ImportExcelData.sql') {
  sqlcmd -S localhost -E -I -b -i ".\$f"      # SQL Express: -S localhost\SQLEXPRESS ; SQL login: replace -E with -U <user> -P <password>
}
```

Requires SQL Server 2016 SP1 or later. Documentation: `docs/EXCEL_MAPPING.md`, `docs/DATA_QUALITY_REPORT.md`.
