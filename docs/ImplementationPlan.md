# Tag-and-Track Implementation Plan

This document outlines the implementation plan for 11 feature requests. Each section includes the current state, required changes, affected files, and implementation details.

---

## Table of Contents

1. [Don't Require Signature to Send Email](#1-dont-require-signature-to-send-email)
2. [Require at Least 1 Item Before Finalize](#2-require-at-least-1-item-before-finalize)
3. [Multi-Select Page for Adding Items to Container](#3-multi-select-page-for-adding-items-to-container)
4. [Show Container Membership on Specimen View](#4-show-container-membership-on-specimen-view)
5. [Move "Clear Database" to Settings with Confirmation](#5-move-clear-database-to-settings-with-confirmation)
6. [Delete Specimen from DB (If Not in Any Loans)](#6-delete-specimen-from-db-if-not-in-any-loans)
7. [Calendar UI for Loan Due Date](#7-calendar-ui-for-loan-due-date)
8. [Specimen Loan History in Item View](#8-specimen-loan-history-in-item-view)
9. [DB ↔ CSV Import/Export](#9-db--csv-importexport)
10. [Remove Test QR Codes from Settings](#10-remove-test-qr-codes-from-settings)
11. [Filter Specimens by Status & Filter Loans by Status](#11-filter-specimens-by-status--filter-loans-by-status)

---

## 1. Don't Require Signature to Send Email

### Current State
- `FinalizeLoanPage.ConfirmLoan()` blocks submission if `signaturePad.IsBlank` is true.
- `Emailer.Email()` accepts `signatureData` as optional (`byte[]?`), and already handles the `null` case — it simply won't embed a signature image block. No changes needed on the emailer side.
- `LoanCreator.FinalizeLoanAsync()` already accepts `signatureBytes` as nullable. No changes needed there either.

### Changes Required

#### `Pages/SupportPages/FinalizeLoanPage.cs`
- **Remove the validation block** that checks `signaturePad.IsBlank` and shows the "Borrower signature is required." error alert.
- Keep the signature pad UI visible so users _can_ sign if they want — just don't enforce it.
- The `signatureBytes` local variable is already set inside a try/catch and defaults to `null`, so if the pad is blank, `GetSignatureBytes()` will return `null` (or an empty byte array which the emailer handles). No other changes needed.

#### Tests
- No backend logic changes; this is purely a UI validation removal. No new tests needed.

---

## 2. Require at Least 1 Item Before Finalize

### Current State
- `StartLoanPage` has a "Finalize Loan" button that navigates to `FinalizeLoanPage` unconditionally.
- `LoanCreator.LoanItems` could be empty at that point.

### Changes Required

#### `Pages/MainPages/StartLoanPage.cs`
- In the `FinalizeLoan()` method, **before** navigating to `FinalizeLoanPage`, check `LoanCreator.LoanItems.Count`:
  - If `== 0`, show `DisplayAlert("Error", "At least one item must be added to the loan before finalizing.", "OK")` and return early.
  - Otherwise, proceed with navigation.

#### Tests
- **`LoanCreatorTests.cs`**: Add a test `AddItem_ToEmptyLoan_ReturnsNullOnSuccess` to ensure the basic `AddItem` workflow succeeds (already exists implicitly but worth having a named test for the "at least 1 item" contract).
- The guard itself is UI-only and can't be unit-tested without a UI testing framework, so the test focuses on the `LoanCreator` state.

---

## 3. Multi-Select Page for Adding Items to Container

### Current State
- `ViewContainerPage.AddSpecimenAsync()` calls `DisplayActionSheet()` to show a flat list of specimen names. Only one can be selected at a time.
- The `AllSpecimensPage` displays all specimens in a `DataTable<SpecimenItem>` with columns: ID, Arctos ID, Name, Description, Status icon, View button.

### Changes Required

#### New Page: `Pages/SupportPages/SelectSpecimensPage.cs`
- Create a new page that receives a callback (`Action<List<SpecimenItem>>`) or uses `MessagingCenter`/`TaskCompletionSource` to return selected items.
- **Constructor** parameters: `List<SpecimenItem> availableSpecimens` (pre-filtered to exclude already-in-container specimens), and a `TaskCompletionSource<List<SpecimenItem>>` (or equivalent) to return results.
- **Layout**:
  - `HeaderTemplate("Select Specimens")`
  - Reuse the `DataTable<SpecimenItem>` component with columns: a **CheckBox** column (new), ID, Arctos ID, Name, Description, Status icon.
  - **"Add Selected" button** at the bottom — collects all checked items, completes the task, and pops the page.
  - **"Cancel" button** — pops without returning anything.
- **Selection mechanism**: Two approaches (choose one):
  - **Option A — Checkbox column in DataTable**: Add a new column type `AddCheckbox` to `DataTableColumnBuilder<T>` that renders a `CheckBox` in each row. Maintain a `HashSet<T>` of selected items. This is the cleanest approach and matches the "Google Photos multi-select" pattern.
  - **Option B — CollectionView.SelectionMode = Multiple**: Replace the `DataTable` with a native `CollectionView` that has `SelectionMode="Multiple"`. Style selected rows with a highlight color. Simpler but loses DataTable search/filter functionality.
  - **Recommended: Option A** — keeps search bar functional so the user can search while selecting.

#### `Components/DataTable/DataTableColumnBuilder.cs`
- Add `AddCheckbox(string header, Action<T, bool> onToggled, int width = 50)` method.
- This creates a column definition with `IsCheckbox = true`.

#### `Components/DataTable/DataTableColumn.cs` (new or extend existing)
- Add `IsCheckbox` bool property and `CheckboxAction` delegate.

#### `Components/DataTable/DataTable.cs`
- In the `ItemTemplate` rendering loop, handle the new `IsCheckbox` column type by creating a `CheckBox` view that calls the column's `CheckboxAction` on toggle.

#### `Pages/SupportPages/ViewContainerPage.cs`
- Replace the `AddSpecimenAsync()` method body:
  - Fetch all specimens, filter out existing container specimens (same as now).
  - Create a `TaskCompletionSource<List<SpecimenItem>>`.
  - `await Navigation.PushAsync(new SelectSpecimensPage(available, tcs))`.
  - Await the `tcs.Task` to get selected specimens.
  - For each selected specimen, call `_container.AddSpecimen(specimen)`.
  - Call `DbService.UpdateContainerSpecimensAsync(...)` once with the updated full list.
  - Reload the container view.
- **Note**: Items can belong to several containers — the existing `ContainerItem.AddSpecimen()` and DB model already support this (no uniqueness constraint across containers), so no model changes needed.

#### `AppShell.xaml.cs`
- Register route for `SelectSpecimensPage` if using Shell navigation (or use `Navigation.PushAsync` directly).

#### Tests
- No new backend logic that needs unit testing (the selection is purely UI). Existing `ContainerItem.AddSpecimen` tests already cover the model layer.

---

## 4. Show Container Membership on Specimen View

### Current State
- `ViewItemPage.SpecimenView()` shows specimen info (Type, ID, Arctos ID, QR, Status, Description) but has **no container information**.
- There is no `DbService` method to find which containers a specimen belongs to.

### Changes Required

#### `Backend/Data/DbService.cs`
- Add: `public static async Task<List<ContainerItem>> GetContainersBySpecimenIdAsync(int specimenId)`
  - Query all `ContainerEntity` rows.
  - For each, check if `SpecimenIds` CSV contains the given ID.
  - Map matching entities to `ContainerItem` objects via `MapToContainerAsync()`.
  - Return the list.
  - **Performance note**: With a small dataset this is fine. If the container table grows large, consider adding an index table (`ContainerSpecimenEntity` with ContainerId + SpecimenId columns) in a future refactor.

#### `Pages/SupportPages/ViewItemPage.cs` — `SpecimenView()` method
- After building the existing info grid, call `DbService.GetContainersBySpecimenIdAsync((int)specimen.ID)`.
- If containers are found, add a **"Containers" section** below the existing info:
  - A `Label` header: "Part of Containers:".
  - For each container, render a row with:
    - Container name label.
    - An `ImageButton` with `info.png` icon (the "i" icon). On click, navigate to `ViewContainerPage` with that container's ID.
- If no containers, show "Not in any containers" label.
- Since `SpecimenView` is currently synchronous, it will need to become async or use a loading pattern:
  - **Recommended approach**: Keep `SpecimenView` synchronous for the initial render, then use `Task.Run` or `OnAppearing` to async-load container data and inject it into the layout afterward.
  - Alternatively, refactor `SpecimenView` to be `async` and change `Initialize()` to call it with `_ = SpecimenViewAsync(specimen)`.

#### Tests
- **`DbService` is excluded from the test project**, so `GetContainersBySpecimenIdAsync` can't be directly unit-tested without refactoring.
- Consider adding an integration test or testing the lookup logic in isolation if the CSV-parsing logic is extracted to a helper.

---

## 5. Move "Clear Database" to Settings with Confirmation

### Current State
- "Reset DB" button is on `LoginPage` inside `Initialize()`. It calls `ResetDatabaseAsync()` which drops all tables, recreates them, and optionally re-seeds.
- `SettingsPage` currently only has a light/dark mode toggle and test QR codes.

### Changes Required

#### `Pages/MainPages/SettingsPage.cs`
- Add a **"Clear Database"** button (use `TagAndTrackButton` with `trash.png` icon).
- On click, show a **two-step confirmation**:
  1. `DisplayAlert("Clear Database", "This will permanently delete ALL data (specimens, loans, containers, employees). This action cannot be undone.", "I understand, clear it", "Cancel")`.
  2. If confirmed, call `DbService.ResetDatabaseAsync()`.
  3. Show a success alert and navigate back to `LoginPage` (since employee data is also wiped): `await Shell.Current.GoToAsync("//LoginPage")`.

#### `Pages/MainPages/LoginPage.cs`
- **Remove** the `resetDbButton` and its `ResetDatabaseAsync()` method.
- Remove the "Reset DB" `TagAndTrackButton` from the `pageContent` children list.

#### Tests
- No new backend logic. The existing `ResetDatabaseAsync` is unchanged.

---

## 6. Delete Specimen from DB (If Not in Any Loans)

### Current State
- There is no way to delete a specimen from the database.
- `DbService` has no `DeleteSpecimenAsync` method.
- Loans reference specimens via comma-separated IDs in `LoanEntity.SpecimenIds`.

### Changes Required

#### `Backend/Data/DbService.cs`
- Add: `public static async Task<bool> IsSpecimenInAnyLoanAsync(int specimenId)`
  - Query all `LoanEntity` rows.
  - For each, parse `SpecimenIds` CSV and check if it contains the specimen ID.
  - Return `true` if found in **any** loan (active or returned), `false` otherwise.
- Add: `public static async Task DeleteSpecimenAsync(int specimenId)`
  - Delete the `SpecimenEntity` row with the given ID.
  - Also remove the specimen ID from any `ContainerEntity.SpecimenIds` CSV strings (to keep container references clean).

#### `Pages/SupportPages/ViewItemPage.cs` — `SpecimenView()` method
- Add a **"Delete Specimen" button** (use `TagAndTrackButton` with `trash.png` icon).
- On click:
  1. Call `DbService.IsSpecimenInAnyLoanAsync((int)specimen.ID)`.
  2. If `true`, show `DisplayAlert("Cannot Delete", "This specimen is referenced by one or more loans (active or past) and cannot be deleted.", "OK")`.
  3. If `false`, show confirmation: `DisplayAlert("Delete Specimen", "Are you sure you want to permanently delete this specimen? This cannot be undone.", "Delete", "Cancel")`.
  4. If confirmed, call `DbService.DeleteSpecimenAsync((int)specimen.ID)`.
  5. Show success alert and pop navigation back to `AllSpecimensPage`.

#### Tests
- **Unit test in a new `DbServiceTests.cs`** (or helper-level test): Since `DbService` depends on SQLite, a true unit test is hard without the DB. However, the **loan-reference check logic** (parsing CSV strings for an ID) can be extracted into a testable static helper:
  - Extract: `public static bool ContainsSpecimenId(string specimenIdsCsv, int specimenId)` — pure function, testable.
  - Write tests for: ID present, ID absent, empty string, null string, ID at start/middle/end of CSV, partial numeric matches (e.g., specimen ID `1` should not match `11`).

---

## 7. Calendar UI for Loan Due Date

### Current State
- `FinalizeLoanPage` has a `// TODO: date entry that complies with a DateTime` comment.
- The due date is currently hardcoded to `DateTime.MaxValue` in `ConfirmLoan()`.

### Changes Required

#### `Pages/SupportPages/FinalizeLoanPage.cs`
- Add a `DatePicker` control (built-in MAUI control) for the due date:
  ```
  var dueDatePicker = new DatePicker
  {
      MinimumDate = DateTime.Today.AddDays(1),  // must be in the future
      Date = DateTime.Today.AddDays(30),        // default: 30 days from now
      Format = "yyyy-MM-dd",
      TextColor = CurrentTheme.Instance.Theme.Text,
      BackgroundColor = CurrentTheme.Instance.Theme.Background
  };
  ```
- Add a label: "Due Date:" above the picker.
- Add the picker and label to the page layout (between `clientEmailEntry` and the signature section).
- In `ConfirmLoan()`:
  - Replace `DateTime.MaxValue` with `dueDatePicker.Date`.
  - Add validation: if `dueDatePicker.Date <= DateTime.Today`, show `DisplayAlert("Error", "Due date must be in the future.", "OK")`.
- Update the email body to include the actual due date instead of `DateTime.MaxValue`.

#### Tests
- No backend logic changes. The `DatePicker` is a MAUI built-in with its own validation. The `LoanCreator.FinalizeLoanAsync` already accepts any `DateTime` for `dueDate`.

---

## 8. Specimen Loan History in Item View

### Current State
- `ViewItemPage.SpecimenView()` shows basic specimen info but no loan history.
- There is no `DbService` method to find all loans that reference a specific specimen.

### Changes Required

#### `Backend/Data/DbService.cs`
- Add: `public static async Task<List<LoanItem>> GetLoansBySpecimenIdAsync(int specimenId)`
  - Query all `LoanEntity` rows.
  - For each, parse `SpecimenIds` CSV and check if it contains the specimen ID.
  - Map matching entities to `LoanItem` via `MapToLoanAsync()`.
  - Return the list (includes both active and returned loans).

#### `Pages/SupportPages/ViewItemPage.cs` — `SpecimenView()` method
- After the container section (from feature #4), add a **"Loan History" section**:
  - A `Label` header: "Loan History:".
  - Call `DbService.GetLoansBySpecimenIdAsync((int)specimen.ID)`.
  - If loans exist, render a `DataTable<LoanItem>` or a simple list with columns:
    - Loan ID
    - Loan Name
    - Borrower
    - Status (Checked In / On Loan / Overdue)
    - A **"View Loan" button** (using `info.png` icon) that sets `ScannedQRItem.lastScannedItem = loan.QRID` and navigates to `new ViewItemPage()`.
  - If no loans, show "No loan history for this specimen." label.
- **Async loading**: Same pattern as feature #4 — load asynchronously and inject into layout.

#### Implementation Note
- Features #4 and #8 both add async-loaded sections to `SpecimenView()`. They should be implemented together to share the async loading pattern. Consider refactoring `SpecimenView` to:
  1. Synchronously build the basic info layout.
  2. Show a loading indicator.
  3. `await Task.WhenAll(loadContainers, loadLoanHistory)` to fetch both in parallel.
  4. Inject both sections into the layout.

#### Tests
- Same situation as feature #4 — the CSV-parsing logic for finding a specimen ID in a loan's `SpecimenIds` is the same helper. If extracted per feature #6's recommendation, it's already tested.

---

## 9. DB ↔ CSV Import/Export

### Current State
- No import/export functionality exists.
- The DB has 4 tables: `SpecimenEntity`, `LoanEntity`, `ContainerEntity`, `EmployeeEntity`.
- The term "1 to 1 DB to CSV" means each table maps to one CSV file (or section).

### Changes Required

#### New Utility: `Backend/Utils/CsvService.cs`

**Export (`ExportDatabaseToCsvAsync`)**:
- Create one CSV file per table (or a single ZIP/folder containing multiple CSVs).
- **Recommended approach**: Create 4 CSV files in a temp directory, then let the user pick a save location via `FileSaver` (from MAUI Community Toolkit or `FolderPicker`).
- CSV format per table:
  - **specimens.csv**: `Id,ArctosId,Name,Description,IsPresent`
  - **loans.csv**: `Id,ArctosId,Name,Description,Borrower,Email,DateCheckedOut,DateDue,IsReturned,SpecimenIds,SignatureData` (SignatureData as Base64 string)
  - **containers.csv**: `Id,Name,Description,SpecimenIds`
  - **employees.csv**: `Id,Name,LastLogin`
- Use `CsvHelper` NuGet package (or hand-roll with `StringBuilder` since the schema is simple and fixed).
- Fields containing commas or quotes must be properly escaped (wrap in double quotes per RFC 4180).

**Import (`ImportDatabaseFromCsvAsync`)**:
- Accept a file path (user picks via `FilePicker`).
- **Strategy**: 
  1. Show confirmation: "Importing will overwrite ALL current data. Continue?"
  2. Call `DbService.ResetDatabaseAsync()` (drops and recreates tables, but skip the seed step).
  3. Parse each CSV file.
  4. Insert rows into the corresponding tables.
  5. Validate data during parsing — show errors for malformed rows but continue with valid ones.
- **File format**: Expect either a folder with 4 named CSVs, or a single CSV with table name headers/separators. **Recommended: folder of 4 CSVs** for simplicity.
- **ID handling**: The imported IDs should be used as-is to preserve relational references (SpecimenIds in loans/containers). Use `InsertAsync` with explicit ID values, which requires either:
  - Inserting with `_db.InsertAsync(entity)` and letting SQLite auto-assign (but then IDs won't match cross-references). **This won't work.**
  - Using raw SQL: `INSERT INTO SpecimenEntity (Id, ArctosId, ...) VALUES (?, ?, ...)` to preserve IDs.
  - **Recommended**: Drop tables, recreate without AUTOINCREMENT (SQLite `INTEGER PRIMARY KEY` allows explicit ID insertion by default), then insert with explicit IDs.

#### `Backend/Data/DbService.cs`
- Add: `public static async Task<List<SpecimenEntity>> GetAllSpecimenEntitiesAsync()` — returns raw entities (not mapped Items) for export.
- Add: Similar entity-level getters for loans, containers, employees.
- Add: `public static async Task InsertRawSpecimenAsync(SpecimenEntity entity)` — inserts with explicit ID.
- Add: Similar raw inserters for other entity types.
- Alternatively, expose `_db` via a method for the CSV service to use directly (less clean but fewer methods).

#### `Pages/MainPages/SettingsPage.cs`
- Add two buttons:
  - **"Export Database to CSV"** (`TagAndTrackButton` with appropriate icon, e.g., `export.png` or reuse existing icon).
    - On click: Call `CsvService.ExportDatabaseToCsvAsync()`.
    - Use `FileSaver` or write to a known directory and show the path.
  - **"Import Database from CSV"** (`TagAndTrackButton` with `import.png` or similar).
    - On click: Show confirmation prompt ("This will overwrite all current data. Are you sure?").
    - Use `FilePicker` to let user select a folder or ZIP.
    - Call `CsvService.ImportDatabaseFromCsvAsync(path)`.
    - On success, navigate to `LoginPage` (data has been replaced).

#### NuGet Packages
- Consider adding `CommunityToolkit.Maui` for `FileSaver`/`FolderPicker` if not already present.
- Optional: `CsvHelper` NuGet for robust CSV parsing (handles escaping edge cases). For a simple fixed schema, manual parsing with `string.Split` may suffice.

#### Resources/Images
- Add `export.png` and `import.png` icons (or reuse existing icons like `plus.png` / `view.png`).

#### Tests
- **`CsvServiceTests.cs`** (new file):
  - Test CSV generation from known entity lists — verify header row, value formatting, comma escaping, Base64 encoding of signature data.
  - Test CSV parsing — verify round-trip: export → import → export produces identical output.
  - Test edge cases: empty tables, null fields, specimen IDs with single item, signature data with null bytes.

---

## 10. Remove Test QR Codes from Settings

### Current State
- `SettingsPage.Initialize()` creates 4 `QrCodeView` objects (`Specimen:1`, `Specimen:2`, `Loan:1`, `Loan:2`) and puts them in a `HorizontalStackLayout`.

### Changes Required

#### `Pages/MainPages/SettingsPage.cs`
- **Remove** the entire `qr` array declaration (the 4 `QrCodeView` objects).
- **Remove** the `HorizontalStackLayout` that contains them from the `Content` children.
- The `Content` `VerticalStackLayout` should then only contain `header`, `themeButton`, and the new buttons from features #5 and #9.

#### Tests
- No tests needed — pure UI removal.

---

## 11. Filter Specimens by Status & Filter Loans by Status

### Current State
- `AllSpecimensPage` uses `DataTable<SpecimenItem>` with a search bar that filters across text columns. There is no status-based filter.
- `LoanHistoryPage` uses `DataTable<LoanItem>` with a search bar. The "Status" column shows "Checked In" / "On Loan" / "Overdue" but is not filterable (`filterable: false`).

### Changes Required

#### Approach: Add Filter Buttons/Segmented Control Above Each DataTable

**Option A — Segmented Buttons (Recommended):**
A row of toggle buttons above the table: "All" | "Checked In" | "Checked Out" (for specimens) or "All" | "On Loan" | "Overdue" | "Checked In" (for loans). Clicking one filters the table.

**Option B — Picker Dropdown:**
A `Picker` with filter options. Less visual but simpler.

**Recommended: Option A** — more visible and matches modern UI patterns.

#### `Pages/MainPages/AllSpecimensPage.cs`
- Add a `HorizontalStackLayout` with 3 filter buttons above the DataTable:
  - **"All"** (default, selected) — shows all specimens.
  - **"Checked In"** — shows only specimens where `Status == true`.
  - **"Checked Out"** — shows only specimens where `Status == false`.
- Store the full `List<SpecimenItem>` from `DbService.GetAllSpecimensAsync()`.
- On filter button click:
  - Filter the list based on the selected status.
  - Rebuild the `DataTable<SpecimenItem>` with the filtered list (or add a public `UpdateItems(IEnumerable<T>)` method to `DataTable<T>`).
  - Highlight the active filter button (e.g., different `BackgroundColor`).

#### `Pages/MainPages/LoanHistoryPage.cs`
- Add a `HorizontalStackLayout` with 4 filter buttons:
  - **"All"** (default).
  - **"On Loan"** — `Status == false && DateTime.Now <= DateDue`.
  - **"Overdue"** — `Status == false && DateTime.Now > DateDue`.
  - **"Checked In"** — `Status == true`.
- Same pattern: store full list, filter on button click, rebuild DataTable.
- Make the "Status" column `filterable: true` so the text search also works on status values.

#### `Components/DataTable/DataTable.cs` (Enhancement)
- Add a public method: `public void UpdateItems(IEnumerable<T> newItems)` that:
  - Clears `_allItems` and `_filteredItems`.
  - Repopulates both from `newItems`.
  - This avoids recreating the entire DataTable on every filter change.
- Alternatively, if rebuilding is simpler and performant enough (small dataset), just reconstruct the DataTable in the page each time.

#### Tests
- No backend logic changes. The filtering is purely UI-side list filtering using existing `Status` and date properties.
- If an `UpdateItems` method is added to `DataTable<T>`, it's a UI component — no unit test needed.

---

## Implementation Order (Suggested)

The features are listed in a recommended implementation order based on dependencies and complexity:

| Priority | Feature | Complexity | Dependencies |
|----------|---------|------------|--------------|
| 1 | #10 — Remove test QR codes | Trivial | None |
| 2 | #1 — Don't require signature | Trivial | None |
| 3 | #2 — Require ≥1 item to finalize | Trivial | None |
| 4 | #5 — Move clear DB to settings | Low | #10 (settings page cleanup) |
| 5 | #7 — Calendar UI for due date | Low | None |
| 6 | #11 — Filter specimens/loans by status | Medium | None |
| 7 | #4 — Show containers on specimen view | Medium | New DbService method |
| 8 | #8 — Specimen loan history | Medium | New DbService method, pairs with #4 |
| 9 | #6 — Delete specimen | Medium | New DbService methods, pairs with #8 |
| 10 | #3 — Multi-select container items | High | DataTable checkbox column extension |
| 11 | #9 — CSV import/export | High | New service, file I/O, NuGet dependencies |

Features #4 and #8 share the same async-loading pattern in `SpecimenView()` and should be implemented together.

Features #5 and #10 both modify `SettingsPage` and should be done in sequence.

---

## Summary of New Files

| File | Purpose |
|------|---------|
| `Pages/SupportPages/SelectSpecimensPage.cs` | Multi-select specimen picker for containers (#3) |
| `Backend/Utils/CsvService.cs` | CSV import/export logic (#9) |

## Summary of Modified Files

| File | Features |
|------|----------|
| `Pages/SupportPages/FinalizeLoanPage.cs` | #1, #7 |
| `Pages/MainPages/StartLoanPage.cs` | #2 |
| `Pages/SupportPages/ViewContainerPage.cs` | #3 |
| `Pages/SupportPages/ViewItemPage.cs` | #4, #6, #8 |
| `Pages/MainPages/SettingsPage.cs` | #5, #9, #10 |
| `Pages/MainPages/LoginPage.cs` | #5 |
| `Pages/MainPages/AllSpecimensPage.cs` | #11 |
| `Pages/MainPages/LoanHistoryPage.cs` | #11 |
| `Backend/Data/DbService.cs` | #4, #6, #8 |
| `Components/DataTable/DataTable.cs` | #3, #11 |
| `Components/DataTable/DataTableColumnBuilder.cs` | #3 |
| `AppShell.xaml.cs` | #3 (route registration) |

## Summary of New Tests

| Test File | Covers |
|-----------|--------|
| `LoanCreatorTests.cs` (extend) | #2 — empty loan guard |
| `CsvServiceTests.cs` (new) | #9 — CSV round-trip, escaping, edge cases |
| `DbServiceHelperTests.cs` (new) | #6 — `ContainsSpecimenId` CSV parsing helper |
