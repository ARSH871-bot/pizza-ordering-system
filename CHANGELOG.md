# Changelog

All notable changes to this project are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]

---

## [2.15.0] — 2026-03-26

### Added

- **`EndOfDayForm`** (`Forms/EndOfDayForm.cs`) — Z-Report: cashier shift-close summary for the current calendar day. Shows four KPI boxes (Orders, Revenue, GST, Average), a payment-method breakdown ListView, and a top-items ListView. Print to any printer via `PrintPreviewDialog`; Export CSV button. Opened via **"End of Day"** button (forest-green, Tab 2) or `Alt+E`.
- **`Alt+E` keyboard shortcut** — wired in `ProcessCmdKey` to open `EndOfDayForm` from anywhere in the main form.

### Fixed

- **`PrintReceipt` multi-page and line-wrap** — previous implementation used a single `DrawString` call, truncating any receipt longer than one page and not wrapping long lines. Rewritten to walk lines one-by-one with character-level wrapping and proper `HasMorePages` signalling so any receipt length prints correctly.

### Changed

- **About dialog** — test count updated from 95 to 192.
- **Sales Report button** — removed the Unicode emoji (surrogate-pair) from the button label; plain text "Sales Report" renders reliably across all Windows DPI settings.
- **Order confirmation delivery estimate** — reads `DeliveryMinutes` from the SQLite `Settings` table at runtime (falls back to `AppConfig.DeliveryMinutes` when settings are unavailable). Delivery time now reflects whatever the admin has configured, without a restart.
- **README** — updated version badge (2.15.0), test count badge (192), architecture diagram, feature sections (reporting, admin, keyboard shortcuts), and project structure tree.
- `AssemblyInfo.cs` — version bumped to `2.15.0.0`.

---

## [2.14.0] — 2026-03-26

### Added

- **`PinLoginForm`** (`Forms/PinLoginForm.cs`) — dark-theme numpad PIN dialog shown at startup when a staff PIN is configured. Features: circle-dot masking (●), physical numpad and keyboard support (digits, Backspace, Delete, Enter, Escape), CLR button, shake animation on incorrect entry. If `StaffPin` is empty the dialog is bypassed entirely — zero friction for single-operator shops.
- **Staff PIN startup gate in `Program.cs`** — `PinLoginForm.PinRequired(settings)` check added between migration and `Application.Run`; cancelling the dialog exits cleanly without launching the POS.
- **`SalesReportForm`** (`Forms/SalesReportForm.cs`) — dark-theme period sales report form (1020×680). Period selector with Today / This Week / This Month shortcuts, four KPI boxes (Orders, Revenue, GST, Avg Order in amber), and three detail ListViews: Daily Breakdown, Top Items (by revenue), Payment Methods. Includes Export CSV. Opened via **Sales Report** button (Tab 2) or `Alt+R`.
- **Sales Report button** (Tab 2, `Form1`) — wired to `OpenSalesReportForm()`; `Alt+R` keyboard shortcut registered in `ProcessCmdKey`.
- **`StaffPin` friendly name in `SettingsForm`** — displays as "Staff PIN (leave blank to disable)" in the admin settings grid.
- **Void Order in `OrderHistoryForm`** — "Void Order" button (dark amber), right-click context menu item, and `V` keyboard shortcut. Voided orders remain in the log (for audit) but are excluded from all revenue reports. Voided rows are rendered in italic grey so they are immediately distinguishable from active orders.
- **Status column in `OrderHistoryForm`** — shows "Active" or "Voided" per row; detail view includes the status line.
- **15 new integration tests** in `OrderRepositoryTests` covering `VoidOrder` (3), `GetSummaryForPeriod` (3), `GetDailySummaries` (3), `GetTopItems` (3), and `GetPaymentBreakdown` (3).

### Changed

- `AssemblyInfo.cs` — version bumped to `2.14.0.0`.

---

## [2.13.0] — 2026-03-26

### Added

- **Composition root in `Program.cs`** — single explicit wiring point; `Form1` now depends only on interfaces. The only place that calls `new` on a concrete service is `Program.cs`.
- **`DatabaseMigrator`** (`Infrastructure/DatabaseMigrator.cs`) — dependency-free migration runner built on existing SQLite + Dapper. Tracks applied scripts in a `SchemaHistory` table; each script runs exactly once across all environments. Zero new NuGet packages.
- **`Settings` database table** (migration `0001_AddSettingsTable`) — all prices and delivery time now persisted in SQLite, seeded with defaults on first run.
- **`ISettingsRepository` + `SettingsRepository`** — interface + SQLite implementation for reading/writing the `Settings` table.
- **`CartService` dynamic pricing** — accepts an optional `ISettingsRepository`; prices are resolved from the DB at order time, falling back to `AppConfig` compile-time values. All 177 existing tests pass unchanged.
- **`SettingsForm`** (`Forms/SettingsForm.cs`) — dark-theme admin form; opens via `⚙ Settings` button (Tab 2) or `Alt+W`. DataGridView with friendly names, inline editing, numeric validation, immediate persistence. Changes take effect on the next order — no restart required.

### Changed

- **`Form1` constructor injection** — hardwired field initialisers replaced with `(IOrderRepository, ICartService, ISettingsRepository)` constructor. Parameterless fallback retained for WinForms Designer.
- `AssemblyInfo.cs` — version bumped to `2.13.0.0`.

---

## [2.12.0] — 2026-03-26

### Fixed

- **Critical pricing bug in `BuildOrderForReceipt()`** — the ListView price column stores line totals (`qty × unitPrice`), but the method was passing that value directly as the `unitPrice` parameter of `OrderItem`. Since `OrderItem.TotalPrice = UnitPrice × max(Quantity, 1)`, the quantity was being applied twice, inflating `Order.Subtotal` and producing a negative change amount on the receipt. Fixed by back-calculating `unitPrice = lineTotal / max(qty, 1)` before constructing each `OrderItem`.

### Added

- **2 regression tests** in `ReceiptWriterTests` — `Build_MultiQtyDrink_SubtotalNotDoubledByQuantity` and `Build_MultiQtyDrink_DoesNotDoubleCount` guard against re-introducing the double-counting bug.
- **World-class UI/UX overhaul (Form1)** — applied programmatically in `ApplyUiPolish()`:
  - Form launches **maximised** so the tab control fills the screen with no dead space or scrolling.
  - `tabControl1` gains `Anchor = Top | Left | Right | Bottom` so it resizes with the window.
  - Form background changed from `DarkGoldenrod` to brand burnt-orange `rgb(192, 64, 0)`, eliminating the ugly golden-brown strip.
  - **Button colour hierarchy**: primary forward-flow buttons (Confirm Order, Check Out, Submit Order) → brand orange-red; secondary/back buttons (Add to Cart, Order Again, Go Back) → charcoal; destructive buttons (Exit, Clear Order) → crimson; Pay button → forest green. All buttons use `FlatStyle.Flat`, no border, `Cursor.Hand`, "Segoe UI 10pt Bold".
  - "Add Pizza to Cart" button auto-sized to `175×40` so the label is never clipped.
  - History and About buttons resized to match the rest of the action row.
- **World-class UI/UX overhaul (OrderHistoryForm)** — dark-theme redesign:
  - Background `rgb(26, 26, 26)` (deep charcoal) with surface panels `rgb(38, 38, 38)`.
  - Filter bar on dark surface; search TextBox styled dark; labels in `rgb(240, 240, 240)`.
  - Stats bar uses a dark brand-tint panel with warm amber text `rgb(255, 200, 140)`, "Segoe UI 9pt Bold" — instantly visible at a glance.
  - ListView dark background `rgb(32, 32, 32)` with light text; no border.
  - Button bar: View Details → brand orange-red; Delete → crimson; Export CSV / Close → charcoal. All flat, hand cursor, "Segoe UI 9.5pt Bold".
  - Form default size increased to `940×600`.

### Changed

- `AssemblyInfo.cs` — version bumped to `2.12.0.0`.

---

## [2.11.0] — 2026-03-26

### Added

- **Central Package Management (`Directory.Packages.props`)** — all 12 NuGet package versions centralised in a single file. `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` enabled in `Directory.Build.props`. Upgrading any package now requires editing exactly one line in one file; version drift between projects is impossible.
- **Tooltips on Order History controls** — hover hints added to the stats bar (explains all-time vs. filtered), Delete button (`Del` key reminder), View Details button (`Enter` key reminder), and Export CSV button.

### Changed

- `Directory.Build.props` — `ManagePackageVersionsCentrally` enabled; analyzer `PackageReference` entries no longer carry `Version=` attributes.
- Both `.csproj` files — all `Version=` attributes removed from every `PackageReference`. Versions now resolved exclusively from `Directory.Packages.props`.
- `AssemblyInfo.cs` — version bumped to `2.11.0.0`.

---

## [2.10.0] — 2026-03-26

### Added

- **`Directory.Build.props`** — single source of truth for properties shared by every project: `LangVersion=7.3`, `Nullable=disable`, `Deterministic=true`, and both analyzer packages (`Microsoft.CodeAnalysis.NetAnalyzers` + `StyleCop.Analyzers`). Eliminates the duplication that previously existed between the two `.csproj` files.
- **`global.json`** — pins the .NET SDK to `9.0.x` (`rollForward: latestMinor`, `allowPrerelease: false`). Ensures every developer and CI agent uses the same SDK version, preventing "works on my machine" build differences.
- **`NuGet.Config`** — explicit `<packageSources>` (nuget.org only) + `<packageSourceMapping>`. Prevents accidental resolution from machine-level or corporate feed sources; makes package restore fully reproducible and auditable.
- **`Delete` / `Enter` keyboard shortcuts in Order History** — `KeyPreview = true` on the form; `Delete` key triggers the delete-with-confirmation flow, `Enter` key opens the detail view. No mouse required for common actions.

### Changed

- Both `.csproj` files — `LangVersion`, `Nullable`, and analyzer `PackageReference` blocks removed (now inherited from `Directory.Build.props`). Files are shorter and easier to read.
- `AssemblyInfo.cs` — version bumped to `2.10.0.0`.

---

## [2.9.1] — 2026-03-26

### Changed

- **Zero-warning CI gate** — `dotnet build` in both `build-and-test.yml` and `release.yml` now passes `-warnaserror`, turning any surviving diagnostic warning into a build failure.
- **`.editorconfig`** — all eight StyleCop rule categories suppressed via `dotnet_analyzer_diagnostic.category-StyleCop.CSharp.*.severity = none`. This eliminates the ~740 pre-existing StyleCop noise warnings (spacing, ordering, naming, layout, documentation) that were present in the WinForms / Designer-generated code without meaningful benefit. Additional CA rule suppressions added for common WinForms patterns: `CA1002`, `CA1031`, `CA1051`, `CA1501`, `CA1502`, `CA1506`, `CA1711`, `CA1716`, `CA1724`, `CA2213`, `CA2227`, `CA1416`.
- `AssemblyInfo.cs` — version bumped to `2.9.1.0`.

---

## [2.9.0] — 2026-03-26

### Added

- **`IOrderRepository.Delete(id)`** — permanently removes an order and all its line items in a single atomic transaction (`ON DELETE CASCADE` handles the lines). No-ops silently on unknown or null ids.
- **Delete Order UI** — "Delete Order" button (dark-red text) in the Order History button bar; also available via right-click context menu on any list row. Both paths show a confirmation dialog (default: No) before deleting, then refresh the list and stats bar automatically.
- **SQLite indexes** — `IX_Orders_OrderDate` on `Orders(OrderDate)` and `IX_OrderLines_OrderId` on `OrderLines(OrderId)` created at schema initialisation (`CREATE INDEX IF NOT EXISTS`). Both existing and new databases gain the indexes automatically on next launch.
- **5 new tests** — 4 integration tests in `OrderRepositoryTests` (`Delete` removes record, `Delete` cascades lines, non-existent id is a no-op, null/empty id is a no-op); 1 NSubstitute mock test in `ServiceInterfaceTests`. **Total tests: 170 → 175**.

### Changed

- `CONTRIBUTING.md` — updated test count 145 → 175; replaced legacy `msbuild` + `vstest.console.exe` commands with `dotnet restore` / `dotnet build` / `dotnet test`.
- `AssemblyInfo.cs` — version bumped to `2.9.0.0`.

---

## [2.8.0] — 2026-03-26

### Added

- **SQL-powered search** — `IOrderRepository` gains `Search(string text, DateTime? from, DateTime? to)`. All filtering is now performed in the database via SQL `LIKE` and date-range `WHERE` clauses instead of iterating a full in-memory list. Matches on CustomerName, Region, PaymentMethod, or OrderDate.
- **`OrderSummary` model + `GetSummary()` method** — new `IOrderRepository.GetSummary()` returns a single-query aggregate (`COUNT`, `SUM(Total)`, `AVG(Total)`) rounded to 2 d.p.
- **Stats bar in Order History** — a slim panel above the list shows all-time order count, total revenue, and average order value (always reflects the full database, not the current filter).
- **9 new tests** — 7 new `OrderRepositoryTests` (Search with no filter, by text, by date range, combined text+date, no-match, `GetSummary` on empty DB, `GetSummary` with data); 2 new `ServiceInterfaceTests` (NSubstitute mocks for `Search` and `GetSummary`). **Total tests: 161 → 170**.

### Changed

- `OrderHistoryForm.ApplyFilter()` — replaced in-memory C# loop with `_repo.Search()` call; no longer loads all rows on every keystroke.
- `OrderHistoryForm.LoadOrders()` — now calls `RefreshStats()` once on open, then delegates to `ApplyFilter()`.
- `AssemblyInfo.cs` — version bumped to `2.8.0.0`.

---

## [2.7.0] — 2026-03-25

### Added

- **SQLite embedded database** — `OrderRepository` now persists orders to `%APPDATA%\PizzaExpress\orders.db` using SQLite 1.0.118.0 via Dapper 2.1.35. Replaces the NDJSON flat-file store. Schema created automatically on first run; both the order header (Orders table) and line items (OrderLines table) are written in a single ACID transaction.
- **Automatic three-generation migration** — on first launch the repository checks for legacy data files and silently imports them: (1) `orders.ndjson` → SQLite, then renames to `.migrated`; (2) if no NDJSON, `orders.json` (pre-v2.4.0 JSON array) → SQLite, then renames to `.migrated`. Zero user action required.
- **SDK-style project files** — both `.csproj` files migrated from old-style (`ToolsVersion="4.0"` + `packages.config`) to SDK-style (`Sdk="Microsoft.NET.Sdk"` + `PackageReference`). Enables `dotnet restore`, `dotnet build`, `dotnet test` and lays the groundwork for the planned WPF/.NET 8 migration (Phase 10).

### Changed

- CI workflows (`build-and-test.yml`, `release.yml`) — replaced `setup-msbuild` + `setup-nuget` + `nuget restore` + `msbuild` + `vstest.console.exe` search with `actions/setup-dotnet@v4` + `dotnet restore` + `dotnet build` + `dotnet test`. Simpler, faster, and more portable.
- `Microsoft.CodeAnalysis.NetAnalyzers` bumped from `8.0.0` → `9.0.0` to match the .NET 9 SDK's built-in analyzer version and eliminate the build warning.
- `AssemblyInfo.cs` — version bumped to `2.7.0.0`.

---

## [2.6.0] — 2026-03-25

### Added

- **6 new `OrderValidatorTests`** — `ValidateCustomer` now tested for: `null` customer, missing `LastName`, missing `Address`, customer with invalid email (delegates to `ValidateEmail`), customer with invalid contact number (delegates to `ValidateContactNo`); `ValidatePayment` now tested for `null` method. **Total tests: 145 → 151**.
- **5 new `ReceiptWriterTests`** — `Build()` now verified to contain: `Subtotal:` label, `Change:` label, customer contact number, customer region, and delivery time (30 min). **Total: 151 → 156**.
- **2 new `PromoEngineTests`** — `Apply` message verified to contain the percentage string (`"10%"`) for discount codes; verified to contain the discounted total for `PIZZA20`. **Total: 156 → 158**.
- **2 new `OrderRepositoryTests`** — `LoadAll` with a present-but-empty NDJSON file returns an empty list (not a crash); `Save` of three records round-trips in insertion order. **Total: 158 → 160**.
- **1 new `CartServiceTest`** — `BuildPizzaItems` for `PizzaSize.Large` verifies the unit price is `$10.00`. **Total: 160 → 161**.

### Changed

- `CONTRIBUTING.md` — updated stale "all 95 tests" reference to "all 145 tests"; added Coverlet collection to the local test command example; added ≥ 70% line-coverage requirement.
- `AssemblyInfo.cs` — version bumped to `2.6.0.0`.
- README — version badge, test count badge, description, project structure tree, and contributing section all updated to reflect 161 tests.

---

## [2.5.0] — 2026-03-25

### Added

- **Lifetime-free CI coverage gate** — Codecov (third-party, pricing risk) replaced with a self-contained PowerShell step that parses `coverage.opencover.xml`, writes a formatted table directly to the **GitHub Actions job summary**, and fails the build if line coverage drops below 70%. Zero external services, zero tokens, works forever.
- **`.gitattributes`** — eliminates the CRLF/LF conversion warnings that appeared on every `git commit`. Sets `text=auto eol=crlf` for C# source files and `eol=lf` for YAML/shell CI files; marks binaries as `binary`.
- **`NullLogger`** — sealed no-op `ILogger` implementation with a shared `Instance` singleton. Use in tests and anywhere logging output is unwanted without passing `null`.
- **`FileLoggerTests`** — 14 tests covering `FileLogger` and `NullLogger`: Info/Warn/Error write to file, timestamp format, exception type included, multiple entries appended, directory auto-created, silent failure when path is inaccessible, and all `NullLogger` variants.
- **ILogger + ICartService mock tests** — 7 new tests added to `ServiceInterfaceTests`: NSubstitute mocks for `ILogger.Info`, `ILogger.Error`, `ICartService.BuildPizzaItems`, `ICartService.CalculateTotal`; real-impl interface checks for `NullLogger`, `FileLogger`, and `CartService`. **Total tests: 123 → 145**.
- **Release build configuration** for `PizzaExpress.Tests.csproj` — the test project was missing a `Release|AnyCPU` `PropertyGroup`, causing the CI release workflow to fail. Added with `Optimize=true`, `pdbonly` debug symbols, and correct `OutputPath`.

### Changed

- `build-and-test.yml` CI: Codecov upload step removed; replaced with `Parse coverage and write job summary` PowerShell step with a 70% line-coverage gate.
- README: Codecov badge replaced with static `coverage: >70% gated` badge; test count updated to 145.

---

## [2.4.0] — 2026-03-25

### Added

- **`ICartService` + `CartService`** — pizza-building and price-calculation logic extracted from `Form1.cs` into a dedicated, WinForms-free service. `Form1` now calls `_cart.BuildPizzaItems()` and `_cart.CalculateTotal()` instead of duplicating math inline. Fully unit-testable.
- **19 `CartServiceTests`** — cover `BuildPizzaItems` (all sizes, crust types, qty scaling, topping names, empty/null toppings, zero-qty guard), `CalculateSubtotal` (empty, null, multi-item), `CalculateTax` (zero, standard, rounding), `CalculateTotal`. **Total tests: 104 → 123**.
- **Live running total in status bar** — right side of the status bar updates in real time as the user selects pizza size, crust, quantity, toppings, drinks, and sides on Tab 1. Shows `Live total (incl. GST): $XX.XX`. Updates instantly on every checkbox/radio/NumericUpDown change.
- **Input length limits** — `MaxLength` set on every `TextBox`: FirstName/LastName 50, Address 100, City 60, PostalCode 4, ContactNo 15, Email 100, CardOrPromo 30, AmountPaid 12.
- **Accessibility labels** — `AccessibleName` set on 22 interactive controls (text fields, combo boxes, buttons, ListView). Screen readers now announce meaningful names instead of generic "Button" or "TextBox".
- **`ILogger` interface** — minimal `Info` / `Warn` / `Error(message, ex)` contract; services and UI receive it via field injection.
- **`FileLogger` implementation** — appends timestamped entries to a daily-rotating file at `%APPDATA%\PizzaExpress\Logs\app_yyyy-MM-dd.log`. Thread-safe. Never throws (all I/O errors swallowed internally).
- **Structured app logging** — three key events logged: application start (with version), order saved (with ID, customer, total, method), promo applied (with code and discounted amount). Errors during history persistence logged at ERROR level with exception details.
- **Append-only NDJSON store** — `OrderRepository` switched from full-array JSON (read-whole-file-on-every-save) to Newline-Delimited JSON (one line per order). `Save()` is now O(1) regardless of history size; `LoadAll()` reads line-by-line and silently skips corrupted lines.
- **Automatic legacy migration** — if `orders.json` (old format) exists but `orders.ndjson` does not, all records are imported on first load and the old file is renamed `orders.json.migrated`. Zero user action required.
- **2 new `OrderRepositoryTests`** — corrupted-line-is-skipped test and legacy migration test. Total repository tests: 7 → 9.
- **Coverlet `coverlet.runsettings`** — configures OpenCover format output, excludes test assembly and auto-generated files (`Form1.Designer.cs`, `Program.cs`, `AssemblyInfo.cs`).
- **CI: Coverlet + Codecov** — `build-and-test.yml` now collects `XPlat Code Coverage` using `coverlet.runsettings`, uploads the `coverage.opencover.xml` report to Codecov, and uploads it as a build artifact. Codecov badge added to README.
- **Setup note:** add `CODECOV_TOKEN` as a GitHub Actions secret (Settings → Secrets → New repository secret) from [codecov.io](https://codecov.io) to enable the live coverage badge.

### Changed

- `OrderRepository` data file: `orders.json` → `orders.ndjson` (with automatic one-time migration from old format).
- `Form1.BuildCurrentPizzaItems()` now delegates to `CartService.BuildPizzaItems()` — no duplicate price logic in UI.

---

## [2.3.0] — 2026-03-25

### Added

- **Email validation** — `IOrderValidator.ValidateEmail()` and `OrderValidator.ValidateEmail()` validate optional email format (`local@domain.tld`). `ValidateCustomer()` now also checks email. Inline green/red feedback on the Email field `Leave` event.
- **9 new email validation tests** — null, empty, whitespace (all valid/optional), standard address, subdomain address, missing `@`, missing domain, missing TLD, error message content. **Total tests: 95 → 104**.
- **Postal code input masking** — `txtPostalCode` now accepts digits only (non-digit keys silently blocked) and is capped at 4 characters, making invalid input impossible to type.
- **Global unhandled exception handler** in `Program.cs` — catches exceptions on both the UI thread (`Application.ThreadException`) and background threads (`AppDomain.UnhandledException`). Writes a timestamped crash log to `%APPDATA%\PizzaExpress\Logs\crash_*.log` and shows a user-friendly error dialog instead of a raw .NET crash box.
- **Version in title bar** — title now shows `Pizza Express New Zealand — v2.3.0` using `Application.ProductVersion` (reads from `AssemblyFileVersion`).
- **About dialog** — new "About" button on Tab 2 shows version, architecture summary, test count, CI note, and the local data directory path.
- **Column header sorting in Order History** — clicking any column header sorts by that column; clicking again toggles ascending/descending. Sort direction indicated by ▲/▼ arrow in the header text.
- **`AssemblyInfo.cs` updated** — `AssemblyTitle`, `AssemblyProduct`, `AssemblyCompany`, `AssemblyCopyright`, `AssemblyVersion`, and `AssemblyFileVersion` all set to reflect Pizza Express NZ v2.3.0.
- **`CONTRIBUTING.md` rewritten** — reflects current 104-test suite, interface-first architecture rule, StyleCop/Roslyn zero-warning requirement, and conventional commit format table.

---

## [2.2.0] — 2026-03-25

### Added

- **Service interfaces** — `IPromoEngine`, `IOrderValidator`, `IReceiptWriter`, `IOrderRepository` introduced; all service fields in `Form1` and `OrderHistoryForm` now use the interface type. Services are fully swappable without touching UI code.
- **Roslyn NetAnalyzers 8.0** — ~200 FxCop-style rules run at compile time on both projects (warnings surfaced in Visual Studio Error List).
- **StyleCop.Analyzers** — enforces consistent C# naming, file layout, and spacing rules across the codebase.
- **`.editorconfig`** — root-level config: 4-space indent, UTF-8-BOM, trailing-whitespace trimming, `_camelCase` private field convention; analyzer severity overrides tuned for WinForms desktop.
- **NSubstitute 5.1.0** — mocking library added to test project; Castle.Core 5.1.1 included as required proxy-generation dependency.
- **`ServiceInterfaceTests.cs`** — 9 new tests confirming every interface is correctly implemented by its concrete class and that NSubstitute can substitute each interface.
- **`OrderRepositoryTests.cs`** — 7 integration tests for `OrderRepository` using an isolated temp directory (never touches real user data). Covers save, multi-record persistence, field round-trip, missing file, corrupted file, directory auto-creation, and null guard.
- **Coverlet 6.0.2** — code coverage collector added to test project.
- **`release.yml`** CI workflow — automatically builds a Release configuration, runs all tests, and publishes the compiled `.exe` as a GitHub Release asset when a `v*.*.*` tag is pushed.
- **Dependabot** — weekly automated PRs for stale NuGet packages and GitHub Actions versions.
- **Status bar** — live strip at the bottom of the main window showing cart item count and running total (incl. GST) in real time.
- **Tooltips** — every non-obvious field now shows a concise hint on hover (postal code format, contact number rules, amount paid, etc.).
- **Confirm before clearing** — "Clear Order" now shows a warning dialog; cancellable.
- **Remove item from cart** — right-click any item in the Order Review ListView and choose "Remove selected item"; status bar updates instantly.
- **Keyboard shortcuts** — `Alt+C` (Confirm Order), `Alt+H` (Order History), `Alt+P` (Proceed to Payment), `Esc` (navigate back a tab).
- **Print Receipt** — "Print" button in the post-order receipt dialog; opens a full `PrintPreviewDialog` using `Courier New` monospace font.
- **Copy Receipt to Clipboard** — one-click copy of the full formatted receipt text.
- **Order History: live search** — text box filters by customer name, region, payment method, or date while typing.
- **Order History: date range filter** — optional From/To date pickers narrow the history list.
- **Order History: result count** — label shows how many orders match the current filter.
- **Order History: CSV export** — "Export CSV" button saves all visible rows to a `.csv` file (UTF-8).
- **XML `<summary>` doc comments** on all public models, service interfaces, and `OrderHistoryForm`.
- **GitHub issue templates** (Bug Report + Feature Request) under `.github/ISSUE_TEMPLATE/`.
- **PR template** — checklist enforcing tests, CHANGELOG update, and zero magic strings.
- **`SECURITY.md`** — responsible disclosure policy.
- **`CODE_OF_CONDUCT.md`** — Contributor Covenant v2.1.
- **`README.md`** completely rewritten — ASCII architecture diagram, full feature list, updated tech stack table, updated test count (95), project structure tree, and all new badges.
- **`OrderRepository` injectable data path** — constructor overload accepting a custom directory; enables clean integration tests without redirecting `%APPDATA%`.

### Changed

- Total test count: **79 → 95** (16 new tests across 2 new test classes).
- `OrderRepository` refactored from static file path fields to instance fields; data directory now injectable via constructor.

---

## [2.1.0] — 2026-03-24

### Added

- **Order History persistence** — every submitted order is automatically saved to `%APPDATA%\PizzaExpress\orders.json` using `System.Web.Script.Serialization.JavaScriptSerializer` (built-in, no extra NuGet packages). Records include a short unique ID, timestamp, customer details, itemised lines, and totals.
- **Order History viewer** — a new "Order History" button on the Order Review tab opens a resizable dialog listing all past orders (most-recent first) with Date/Time, Customer, Region, Payment, and Total columns. Double-clicking or pressing "View Details" shows the full formatted receipt in a message box.
- **Inline field validation with visual feedback** — the Postal Code and Contact Number fields now colour their background on `Leave`:
  - Green (Honeydew) when the value passes validation
  - Red (MistyRose) when it fails
  - Background resets to the system default on "Order Again"
- **`OrderRecord` / `OrderLineRecord` models** — lightweight, serialisable snapshots decoupled from the live `Order` domain model.
- **`OrderRepository` service** — encapsulates all JSON read/write logic; creates the data directory on first use; gracefully returns an empty list if the file is missing or corrupt.
- **`OrderHistoryForm`** — fully code-constructed WinForms dialog (no `.Designer.cs`).

---

## [2.0.0] — 2026-03-24

### Added

- Three-layer architecture: domain **Models** (`Order`, `OrderItem`, `Customer`, `PizzaSize`, `CrustType`), **Config** (`AppConfig`), and **Services** (`PromoEngine`, `OrderValidator`, `ReceiptWriter`, `ValidationResult`, `PromoResult`).
- `AppConfig` as single source of truth for all constants (tax rate, prices, regions, promo codes, payment methods, delivery time).
- **MSTest unit-test project** (`PizzaExpress.Tests`) — 79 tests covering all service classes and models, 100% passing.
- **GitHub Actions CI pipeline** (`build-and-test.yml`) — builds on `windows-latest` and runs all 79 tests on every push and pull request to master.
- **NZ localisation** — GST 15%, 16 NZ regions (Province dropdown), 4-digit NZ postal code validation, "Price NZD" column header, NZD currency formatting throughout.
- **Resizable window** — `FormBorderStyle.Sizable` + `MaximizeBox = true` with enforced `MinimumSize`.
- **Descriptive control names** — all 60+ generic control names (`textBox1`, `checkBox1`, `button1`, …) renamed to meaningful identifiers (`txtFirstName`, `cbPepperoni`, `btnConfirmOrder`, …) via a PowerShell word-boundary regex script.
- **`System.Web.Extensions` assembly reference** for built-in JSON serialisation.

### Changed

- `TargetFrameworkVersion` upgraded from `v4.5` to `v4.8`.
- Replaced hard-coded magic numbers/strings with `AppConfig` constants across all handlers.
- Replaced `Controls.Find` loops (broken after rename) with direct typed field references.

### Removed

- Sourcecodester attribution from UI footer.
- All Canadian localisation (provinces, HST 13%, CAD currency, Canadian postal code format).

---

## [1.3.0] — 2026-03-23

### Added

- **US-30** — Pizza Quantity Selection: a `NumericUpDown` control (min 1, max 20, default 1) is now displayed in the Pizza Size groupbox. The pizza ListView row shows the chosen quantity and the price reflects `unit price × qty`. Qty resets to 1 on "Order Again".
- **US-31** — Order Multiple Different Pizzas: a new "Add Pizza to Cart" button stages the current pizza (size + crust + qty + toppings), resets those fields to defaults, and allows the customer to configure another pizza before clicking "Confirm Order". All staged pizzas are flushed into the order list when "Confirm Order" is clicked. Staged list clears on "Order Again".

---

## [1.2.0] — 2026-03-23

### Added

- **US-34** — Promo Code Discount: the "Promo Card" payment method now accepts real discount codes.
  - Supported codes: `PIZZA10` (10% off), `PIZZA20` (20% off), `FREESHIP` (100% off / free order)
  - The card number label dynamically changes to "*Promo Code:" when Promo Card is selected
  - Invalid codes are rejected with a clear error message
- **US-35** — Export Order Receipt: after clicking Submit Order a Yes/No dialog offers to save a timestamped `.txt` receipt.
  - Receipt includes store header, customer delivery info, itemised order list, subtotal / HST / total, and payment method
  - Default filename: `Receipt_yyyyMMdd_HHmmss.txt`

### Improved

- **US-32** — Postal Code Format Validation: the Pay button now enforces the Canadian A1A 1A1 format (regex `^[A-Z]\d[A-Z]\d[A-Z]\d$`) before processing any payment. Spaces and lowercase input are normalised automatically.
- **US-33** — Contact Number Validation: when a contact number is entered it is validated for digits only and a length of 7–15 digits (regex `^\+?\d{7,15}$`). The field remains optional.

---

## [1.1.0] — 2026-03-23

### Fixed

#### Critical
- **FIX-01** — App crashed with `FormatException` when a drink checkbox was ticked but its quantity field was left empty. All 7 drink quantity fields are now validated with `int.TryParse` before any processing begins. A specific message is shown per drink (e.g. "Please enter a valid quantity (greater than 0) for Coke.").
- **FIX-02** — App crashed when the amount paid field contained multiple decimal points (e.g. `1.2.3`). The `KeyPress` handler now blocks a second decimal point if one already exists in the field.

#### High
- **FIX-03** — Clicking "Next" multiple times duplicated every order item in the list, causing incorrect totals. The order list is now cleared at the start of each "Next" click before items are re-added.
- **FIX-04** — Once the Confirm button was enabled by a valid payment, re-entering an insufficient amount showed the warning but left the Confirm button enabled. `button8.Enabled = false` is now set in the insufficient payment branch.
- **FIX-05** — The card number field stayed disabled after switching away from "Cash" to any card-based payment method, making card payments impossible. The `comboBox2_SelectedIndexChanged` handler now re-enables the field for all non-Cash selections.
- **FIX-06** — On "Order Again", the pizza size and crust radio buttons were not reset to their defaults (Small, Normal Crust), leaving the next order with no pre-selection. Defaults are now restored, and the Confirm button and card field are also reset.
- **FIX-07** — The Confirm button stayed enabled when the user went back from the Order Review tab or cleared the order, allowing a stale payment to confirm a modified order. Both the Back button and the Clear button now disable `button8`.

#### Medium
- **FIX-08** — A drink quantity of `0` was accepted and added a $0.00 line item with no actual drink. Quantity is now validated to be greater than 0.
- **FIX-09** — An empty order (no items in list) could proceed through to the checkout tab showing $0.00 totals. A guard now blocks navigation if the order list is empty after building.
- **FIX-10** — The `listView1_SelectedIndexChanged` handler was an empty auto-generated stub. Clarified with a comment that it is intentionally empty (Designer-wired, no action required).
- **FIX-11** — Drink costs were stored as raw double strings (e.g. `"2.9"`) while all other items used 2 decimal places. All drink costs now use `.ToString("F2")` for consistency.

#### Low
- **FIX-12** — The Exit button on Tab 1 closed the application immediately with no confirmation, risking accidental data loss mid-order. A Yes/No confirmation dialog is now shown before closing.
- **FIX-13** — The card number field was enabled by default on application load even before any payment method was selected. It is now disabled on load and only enabled when a card-based payment method is chosen.

---

## [1.0.0] — 2026-03-23

### Added
- Initial release of Pizza Express Ordering System
- 3-tab Windows Forms UI: Order Selection, Order Review, Checkout
- Pizza customisation: 4 sizes, 3 crust types
- 14 topping options at $0.75 each
- 7 drink options with individual quantity fields
- 4 side items and 3 free dipping sauces
- Itemised order summary with ListView (Item, Qty, Price columns)
- 13% HST calculation with subtotal and total due display
- Customer delivery information form with Canadian province dropdown
- Payment methods: Cash, Credit Card, Debit Card, Promo Card
- Change calculation for cash payments
- Required field validation before payment processing
- Order confirmation dialog with option to place another order or exit
- Full state reset on "Order Again" flow
- Numeric-only input validation on all quantity fields
- `.gitignore` for Visual Studio / .NET build artifacts
- `README.md` with setup instructions and project structure
- `USER_STORIES.md` with 29 detailed user stories, acceptance criteria, priorities and story points
