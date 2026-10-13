# Excel → database mapping

Source files inspected: `calibration plan 2026.xlsx` (QA.FMT-28 rev 12), `QA .FMT - 27 List of Monitoring&Measuring Equipments(MME).xls` (rev 00) and `QA .FMT - 29 Equipment History Card.xls` (sheets *Revised* QA.FMT-29 rev 00, *Old* QAD.FMT-37 rev 01, plus two empty sheets).
Column names below are copied from the workbooks. The one-time import is `07_ImportExcelData.sql`; findings are in `DATA_QUALITY_REPORT.md`.

## Calibration plan 2026 (QA.FMT-28) – one row per instrument

| Excel column | Table.column | Notes |
|---|---|---|
| SI.No | `Equipment.SourceSerialNo` | unique when present (filtered unique index) |
| Instrument / Code No | `Equipment.EquipmentCode` | **not unique**: the plan contains duplicate codes |
| Instrument Name | `Equipment.EquipmentName` | whitespace normalised |
| Range & least count | `Equipment.RangeLeastCount` | |
| Make | `Equipment.Make` | |
| Acceptable Error Limit | `Equipment.AcceptableErrorLimit` | also called "Deviation Allowed" on the history card |
| Calibration Agency | `Equipment.AgencyId` → `CalibrationAgencies.Name` | spelling variants merged (e.g. "J.K CALIBRATION CENTER" / "JK CALIBRATION CENTER") |
| Calibration Frequency | `Equipment.FrequencyId` → `CalibrationFrequencies` | "1 Year" = 12 months, "2 Year" = 24 months |
| Date of Calibration | `CalibrationResults.CalibrationDate` | dd.MM.yyyy text → `DATE` |
| Next Due Date | `CalibrationResults.DueDate` **and** `CalibrationSchedules.DueDate` | the schedule is the plan entry; `PlanYear` = year of the due date |
| Certificate referance | `CalibrationResults.CertificateReference` | |
| RESPONSIBLE PERSONAL | `Equipment.ResponsiblePerson` (also `CalibrationSchedules.AssignedTo`) | stored as written |
| Issue DEPT | `Equipment.DepartmentId` → `Departments.Name` | |
| Section heading rows ("DEWTREAT GAUGE (MAE-300)", "AIR GAUGE - MASTERS") | `Equipment.GroupName` | applied to the instrument rows that follow the heading |

## MME list (QA.FMT-27)

All columns (`S.No`, `Instrument / Code No`, `Instrument Name`, `Range & least count`, `Make`, `Acceptable Error Limit`, `Calibration Agency`, `Calibration Frequency`, `Issue DEPT`) map to the same `Equipment` columns as above. Each plan row is paired with its best-matching MME row (same code; then name/range/make/error limit; then department). Differences are logged (`PlanVsMmeMismatch`). MME rows left without a partner are imported as **Inactive** equipment with an explanatory remark (`MmeOnlyRecord`). `S.No` is not used as a key because the sheet reuses numbers.

## Equipment history card (QA.FMT-29 "Revised" / QAD.FMT-37 "Old")

| Excel field | Table.column |
|---|---|
| Equipment Name / Make / Range & Least Count / Code No | `Equipment.EquipmentName / Make / RangeLeastCount / EquipmentCode` |
| Calibration at : Internal / External / Customer | `Equipment.CalibrationType` (the import assumes *External*; no template row states it) |
| Location | `Equipment.Location` |
| Deviation Allowed | `Equipment.AcceptableErrorLimit` |
| Frequency | `Equipment.FrequencyId` |
| Serial No (old card only) | `Equipment.SerialNo` |
| Sl.No | computed (`ROW_NUMBER()`) in `usp_History_GetByEquipment` |
| Calibration Date | `CalibrationResults.CalibrationDate` |
| Calibration Cert Ref | `CalibrationResults.CertificateReference` |
| Calibration Agency | `CalibrationResults.AgencyId` |
| Error Details | `CalibrationResults.ErrorDetails` |
| Result: OK / NOTOK (old card: "Status of Calibration" OK / NOK) | `CalibrationResults.Result` = `OK` / `NOTOK` (`Unverified` for imported rows) |
| If Not OK, Action Taken | `CalibrationResults.ActionTaken` (required when `NOTOK`) |
| Due Date | `CalibrationResults.DueDate` |
| Remarks / Sign | `CalibrationResults.Remarks / SignedBy` |

The history-card sheets are **blank forms** (no data rows), so the only calibration history available to import is the latest calibration on each plan row.

## Business rules taken from the templates and enforced

* Next due date = calibration date + frequency − 1 day (11.11.2025 + 1 year → 10.11.2026). Applied automatically when a result is saved without a due date; an explicit due date is accepted.
* A result of NOT OK requires "action taken" (database check constraint, stored procedure and service).
* Status shown everywhere is derived from the latest result: *Not calibrated*, *Failed*, *Overdue* (due date passed), *Due soon* (≤ 30 days), *Valid*.
* Recording an OK result completes the linked plan entry and plans the next calibration.

## Items that are not in the Excel files and were assumed

* `Calibration type` → External. `Result` of imported calibrations → *Unverified* (the plan does not say pass/fail). Review both.
* Roles, users, notifications, certificates (metadata), maintenance records and the audit log have no Excel source; they are application features.
