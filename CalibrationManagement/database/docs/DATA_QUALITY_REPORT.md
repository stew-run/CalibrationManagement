# Data quality report

Generated while importing the three supplied Excel templates. **No record was discarded**; every finding is also loaded into the table `dbo.DataQualityIssues` by `07_ImportExcelData.sql` and is visible in the application (Reports > Data quality).

## Counts

| Item | Count |
|---|---|
| Rows in calibration plan (QA.FMT-28, 2026) | 86 (76 distinct codes; 2 extra rows are section headings, not instruments) |
| Rows in MME list (QA.FMT-27) | 85 (69 distinct codes) |
| Codes only in the plan | 7 |
| MME-only rows imported as Inactive | 6 |
| Plan rows without a calibration date | 4 |
| Equipment records created by the import | 92 |
| Calibration results / schedules created | 82 / 82 |

## Findings by type

| Type | Severity | Count |
|---|---|---|
| AgencyNameVariant | Info | 1 |
| BlankTemplate | Info | 1 |
| DepartmentNameDifference | Info | 1 |
| DueDateInconsistent | Warning | 1 |
| DuplicateCode | Warning | 13 |
| FrequencyVariant | Info | 1 |
| InvalidValue | Warning | 1 |
| MissingCalibrationDate | Warning | 4 |
| MissingDepartment | Warning | 10 |
| MmeOnlyRecord | Warning | 6 |
| NotInMmeList | Warning | 7 |
| NumericCode | Warning | 1 |
| ResponsiblePersonVariant | Info | 3 |
| ResultNotRecorded | Info | 1 |
| SerialNumberSequence | Info | 1 |

## Details

- **DueDateInconsistent** (Warning) `plan` row 53 code `413 SM` - Calibrated 29.07.2026 with frequency "1 Year" implies next due about 29.07.2027, but the plan states 28.09.2026. Imported as stated.
- **DuplicateCode** (Warning) `MME list` row 71 code `R-23525` - Code "R-23525" appears 2 times in QA.FMT-27 (rows 71, 109) but only 1 time(s) in the plan.
- **DuplicateCode** (Warning) `MME list` row 78 code `SP-5` - Code "SP-5" appears 2 times in QA.FMT-27 (rows 78, 104) but only 1 time(s) in the plan.
- **DuplicateCode** (Warning) `MME list` row 79 code `SP-6` - Code "SP-6" appears 2 times in QA.FMT-27 (rows 79, 105) but only 1 time(s) in the plan.
- **DuplicateCode** (Warning) `MME list` row 80 code `SP-7` - Code "SP-7" appears 2 times in QA.FMT-27 (rows 80, 106) but only 1 time(s) in the plan.
- **DuplicateCode** (Warning) `MME list` row 81 code `ST -02` - Code "ST -02" appears 2 times in QA.FMT-27 (rows 81, 107) but only 1 time(s) in the plan.
- **DuplicateCode** (Warning) `MME list` row 82 code `ST-01` - Code "ST-01" appears 2 times in QA.FMT-27 (rows 82, 108) but only 1 time(s) in the plan.
- **DuplicateCode** (Warning) `plan` row 9 code `209 SM` - Code "209 SM" appears 2 times in the plan (rows 9, 10) with different certificates/departments. All records were imported as separate equipment; confirm whether these are distinct instruments sharing a code.
- **DuplicateCode** (Warning) `plan` row 34 code `402 SM` - Code "402 SM" appears 3 times in the plan (rows 34, 35, 36) with different certificates/departments. All records were imported as separate equipment; confirm whether these are distinct instruments sharing a code.
- **DuplicateCode** (Warning) `plan` row 44 code `410 SM` - Code "410 SM" appears 2 times in the plan (rows 44, 45) with different certificates/departments. All records were imported as separate equipment; confirm whether these are distinct instruments sharing a code.
- **DuplicateCode** (Warning) `plan` row 46 code `411 SM` - Code "411 SM" appears 2 times in the plan (rows 46, 47) with different certificates/departments. All records were imported as separate equipment; confirm whether these are distinct instruments sharing a code.
- **DuplicateCode** (Warning) `plan` row 49 code `413 SM` - Code "413 SM" appears 4 times in the plan (rows 49, 50, 51, 53) with different certificates/departments. All records were imported as separate equipment; confirm whether these are distinct instruments sharing a code.
- **DuplicateCode** (Warning) `plan` row 54 code `417 SM` - Code "417 SM" appears 2 times in the plan (rows 54, 55) with different certificates/departments. All records were imported as separate equipment; confirm whether these are distinct instruments sharing a code.
- **DuplicateCode** (Warning) `plan` row 56 code `418 SM` - Code "418 SM" appears 2 times in the plan (rows 56, 57) with different certificates/departments. All records were imported as separate equipment; confirm whether these are distinct instruments sharing a code.
- **InvalidValue** (Warning) `plan` row 83 code `ST-01` - Range & least count contains only backtick characters ("`````"); imported unchanged.
- **MissingCalibrationDate** (Warning) `plan` row 45 code `410 SM` - No calibration date / due date / certificate recorded for "PLUNGER DIAL GAUGE" (CNC). Equipment imported with status Not Calibrated and no schedule.
- **MissingCalibrationDate** (Warning) `plan` row 48 code `412 SM` - No calibration date / due date / certificate recorded for "PLUNGER DIAL GAUGE" (VISUAL INSPECTION (SKF)). Equipment imported with status Not Calibrated and no schedule.
- **MissingCalibrationDate** (Warning) `plan` row 52 code `415 SM` - No calibration date / due date / certificate recorded for "PLUNGER DIAL GAUGE" (VISUAL INSPECTION (SKF)). Equipment imported with status Not Calibrated and no schedule.
- **MissingCalibrationDate** (Warning) `plan` row 57 code `418 SM` - No calibration date / due date / certificate recorded for "PLUNGER DIAL GAUGE" (VISUAL INSPECTION (SKF)). Equipment imported with status Not Calibrated and no schedule.
- **MissingDepartment** (Warning) `MME list` row 74 code `SP-10` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 75 code `SP-11` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 76 code `SP-2` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 77 code `SP-4` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 78 code `SP-5` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 79 code `SP-6` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 80 code `SP-7` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 81 code `ST -02` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 82 code `ST-01` - Issue department is blank for "COMPARATOR STAND SURFACE PLATE".
- **MissingDepartment** (Warning) `MME list` row 83 code `SP-1585` - Issue department is blank for "SURFACE PLATE".
- **MmeOnlyRecord** (Warning) `MME list` row 104 code `SP-5` - QA.FMT-27 row 104 (S.No 70, range "300X200MM", error limit "±0.01MM", department "SECONDARY") has no counterpart in the 2026 plan - the plan matches a different MME row for this code. Imported as an Inactive equipment record (no calibration data) so nothing is lost; verify, merge or retire it.
- **MmeOnlyRecord** (Warning) `MME list` row 105 code `SP-6` - QA.FMT-27 row 105 (S.No 71, range "300X200MM", error limit "±0.01MM", department "CNC") has no counterpart in the 2026 plan - the plan matches a different MME row for this code. Imported as an Inactive equipment record (no calibration data) so nothing is lost; verify, merge or retire it.
- **MmeOnlyRecord** (Warning) `MME list` row 106 code `SP-7` - QA.FMT-27 row 106 (S.No 72, range "300X200MM", error limit "±0.01MM", department "UNIT-2 QC TABLE") has no counterpart in the 2026 plan - the plan matches a different MME row for this code. Imported as an Inactive equipment record (no calibration data) so nothing is lost; verify, merge or retire it.
- **MmeOnlyRecord** (Warning) `MME list` row 107 code `ST -02` - QA.FMT-27 row 107 (S.No 73, range "200X160MM", error limit "±0.01MM", department "UNIT-2- MC NEAR") has no counterpart in the 2026 plan - the plan matches a different MME row for this code. Imported as an Inactive equipment record (no calibration data) so nothing is lost; verify, merge or retire it.
- **MmeOnlyRecord** (Warning) `MME list` row 108 code `ST-01` - QA.FMT-27 row 108 (S.No 74, range "300X200MM", error limit "±0.01MM", department "CNC") has no counterpart in the 2026 plan - the plan matches a different MME row for this code. Imported as an Inactive equipment record (no calibration data) so nothing is lost; verify, merge or retire it.
- **MmeOnlyRecord** (Warning) `MME list` row 109 code `R-23525` - QA.FMT-27 row 109 (S.No 75, range "_", error limit "±4.0", department "QA AREA") has no counterpart in the 2026 plan - the plan matches a different MME row for this code. Imported as an Inactive equipment record (no calibration data) so nothing is lost; verify, merge or retire it.
- **NotInMmeList** (Warning) `plan` row 65 code `VMM` - Code "VMM" is in the calibration plan but missing from the QA.FMT-27 MME list.
- **NotInMmeList** (Warning) `plan` row 86 code `AGIPL 8616` - Code "AGIPL 8616" is in the calibration plan but missing from the QA.FMT-27 MME list.
- **NotInMmeList** (Warning) `plan` row 88 code `V26-M6 1&2` - Code "V26-M6 1&2" is in the calibration plan but missing from the QA.FMT-27 MME list.
- **NotInMmeList** (Warning) `plan` row 89 code `V26-M7 1&2` - Code "V26-M7 1&2" is in the calibration plan but missing from the QA.FMT-27 MME list.
- **NotInMmeList** (Warning) `plan` row 90 code `V26-M5 1&2` - Code "V26-M5 1&2" is in the calibration plan but missing from the QA.FMT-27 MME list.
- **NotInMmeList** (Warning) `plan` row 91 code `V26-M4 1&2` - Code "V26-M4 1&2" is in the calibration plan but missing from the QA.FMT-27 MME list.
- **NotInMmeList** (Warning) `plan` row 92 code `AGIPL 8607` - Code "AGIPL 8607" is in the calibration plan but missing from the QA.FMT-27 MME list.
- **NumericCode** (Warning) `plan` row 32 code `2128835` - Instrument code 2128835 is stored as a number in the workbook; imported as text "2128835". Leading zeros, if any, cannot be recovered.
- **AgencyNameVariant** (Info) `plan`  - Agency spelled several ways (J.K CALIBRATION CENTER | JK CALIBRATION CENTER); merged into "JK CALIBRATION CENTER".
- **BlankTemplate** (Info) `history card`  - QA.FMT-29 sheets "Revised" (QA.FMT-29 rev 00) and "Old" (QAD.FMT-37 rev 01) are empty forms with no history rows, so no historical calibration results beyond the plan's latest calibration could be imported.
- **DepartmentNameDifference** (Info) `plan`  - 70 plan rows list a different "Issue DEPT" text than the matching QA.FMT-27 row (e.g. plan row 7 "INCOMING QUALITY" vs MME row 7 "TOOL SETTER (PRESS SHOP)"). The plan value (newer, rev 12) was imported.
- **FrequencyVariant** (Info) `MME list`  - Frequency written as "1-Year"; normalised to "1 Year".
- **ResponsiblePersonVariant** (Info) `plan`  - Possible same person spelled differently: NAVANEE | NAVANEETHAN. Stored exactly as written.
- **ResponsiblePersonVariant** (Info) `plan`  - Possible same person spelled differently: ARUN KUMAR | ARUNKUMAR. Stored exactly as written.
- **ResponsiblePersonVariant** (Info) `plan`  - Possible same person spelled differently: SONAI MUTHU | SONAIMUTHU. Stored exactly as written.
- **ResultNotRecorded** (Info) `plan`  - The plan does not state whether each calibration passed. Imported calibration rows have Result = Unverified rather than assuming OK; review and update.
- **SerialNumberSequence** (Info) `MME list`  - QA.FMT-27 has 85 data rows but its S.No sequence ends at 79; S.No values 41, 48, 70, 71, 72, 73, 74, 75 are used more than once. S.No was not used as a key.
