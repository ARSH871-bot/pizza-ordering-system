# Pizza Express - New Zealand

[![Build and Test](https://github.com/ARSH871-bot/pizza-ordering-system/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/ARSH871-bot/pizza-ordering-system/actions/workflows/build-and-test.yml)
![Version](https://img.shields.io/badge/version-2.50.0-brightgreen)
![Tests](https://img.shields.io/badge/tests-461%20passing-success)
![Coverage](https://img.shields.io/badge/coverage-92.2%25%20line%20(75%25%20gate)-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![Framework](https://img.shields.io/badge/.NET-4.8-purple)

Local-first Pizza Express POS and ordering software for Windows, built in C# WinForms on .NET Framework 4.8 with SQLite + Dapper persistence. This repository is a serious prototype / strong portfolio project focused on truth, operator flow, and zero-budget maintainability.

For maintainers or AI coding agents resuming from lost context, start with [`PROJECT_HANDOFF.md`](PROJECT_HANDOFF.md). It records the latest verified source, CI, coverage, and release state.

## What The App Does

- Builds pizza orders with 4 sizes, 3 crusts, toppings, drinks, water, and sides
- Supports pizza quantity changes and multi-pizza orders in one checkout
- Calculates subtotal, GST, promo discounts, amount due, and change
- Persists orders to a local SQLite database and shows them in order history
- Generates receipts, sales reports, and end-of-day summaries
- Stores menu pricing and delivery settings in SQLite so admin changes apply on the next order
- Creates daily rolling database backups and supports manual backup / restore from Settings

## Current Scope

- Single-workstation desktop POS
- No cloud sync or multi-terminal coordination
- No payment gateway integration
- Non-cash checkout stores only a masked or reference-style payment identifier
- Portable ZIP release package, not an installer

## Key Features

### Ordering
- Small, Medium, Large, and Extra Large pizzas
- Normal, Cheesy, and Sausage crusts
- Toppings priced per item
- Quantity selector per pizza
- Multi-pizza staging before checkout
- Settings-backed drink, water, side, and delivery pricing

### Checkout
- GST at 15%
- NZ postal code, phone, and email validation
- Cash, Credit Card, Debit Card, and Promo Card payment methods
- Promo codes: `PIZZA10`, `PIZZA20`, `FREESHIP`
- Payment references persisted end to end for non-cash orders
- Receipt export to `.txt`, clipboard copy, and print preview

### Operations
- SQL-backed order history with search, filters, sort, void, delete, and CSV export
- Sales reporting by date range
- End-of-day report with payment split and top items
- Daily rolling auto-backup plus manual backup / restore

### Staff Trust Boundary
- Optional staff PIN stored with PBKDF2 hashing
- Blank PIN keeps the app in single-operator mode
- Legacy plaintext PINs upgrade automatically after successful login
- Temporary keypad lockout after repeated failures
- Recent staff auth is reused for Settings and history void/delete actions

## Architecture

- `WindowsFormsApplication3/Form1.cs`
  Main POS UI and workflow orchestration. Some checkout flow still lives here; business rules are pushed into services where practical.
- `WindowsFormsApplication3/Forms/`
  Secondary UI surfaces: `SettingsForm`, `PinLoginForm`, `OrderHistoryForm`, `SalesReportForm`, and `EndOfDayForm`.
- `WindowsFormsApplication3/Services/`
  Pricing, promo logic, validation, receipts, repositories, PIN security, logging, and supporting workflow services.
- `WindowsFormsApplication3/Models/`
  Domain and persistence models including `Order`, `OrderItem`, `Customer`, `OrderRecord`, `OrderLineRecord`, and reporting models.
- `WindowsFormsApplication3/Infrastructure/`
  `DatabaseMigrator` and `DatabaseBackupService` for local data bootstrap and recovery-friendly behavior.

## Tech Stack

| Area | Technology |
|---|---|
| Language | C# |
| Runtime | .NET Framework 4.8 |
| UI | Windows Forms |
| Persistence | SQLite 1.0.118.0 + Dapper 2.1.35 |
| Tests | MSTest 3.3.1 + NSubstitute 5.1.0 |
| Tooling | `dotnet` CLI + `vstest.console.exe` |
| CI | GitHub Actions on `windows-latest` |

## Getting Started

### Download and Run (end users)

1. Go to the [Releases page](https://github.com/ARSH871-bot/pizza-ordering-system/releases) and download the latest `PizzaExpress-*-portable.zip`.
2. Extract the ZIP to any folder (e.g. `Desktop\PizzaExpress`).
3. Double-click `WindowsFormsApplication3.exe` — no installer or elevated permissions needed.

**Requirements:** Windows 10 or 11 with .NET Framework 4.8.
.NET Framework 4.8 is pre-installed on Windows 11.
Windows 10 users: download it free from [microsoft.com](https://dotnet.microsoft.com/download/dotnet-framework/net48).

**Verify the download (optional):** A `.sha256` checksum file is published alongside each ZIP on the Releases page.

```powershell
(Get-FileHash 'PizzaExpress-*-portable.zip' -Algorithm SHA256).Hash
```

Compare the output against the contents of the matching `.sha256` file.

### Build from Source (developers)

**Prerequisites:** Windows 10 or 11, Visual Studio 2022 Community or later, .NET Framework 4.8.

```powershell
git clone https://github.com/ARSH871-bot/pizza-ordering-system.git
cd pizza-ordering-system
dotnet restore WindowsFormsApplication3.sln
dotnet build WindowsFormsApplication3.sln --configuration Debug
```

Open `WindowsFormsApplication3.sln` in Visual Studio and press `F5`, or run the built executable from `WindowsFormsApplication3\bin\Debug\net48\`.

## Validation

### Preferred Local Test Flow

```powershell
dotnet restore WindowsFormsApplication3.sln
dotnet build WindowsFormsApplication3.sln --configuration Debug
.\scripts\Run-Tests.ps1 -Configuration Debug
```

`Run-Tests.ps1` is the verified local and CI runner for this .NET Framework MSTest suite.

### Release Build Validation

```powershell
dotnet build WindowsFormsApplication3.sln --configuration Release
.\scripts\Run-Tests.ps1 -Configuration Release
```

### Portable Package Validation

```powershell
.\scripts\Package-PortableRelease.ps1 -Configuration Release
.\scripts\Test-PortablePackage.ps1 -PackagePath .\artifacts\PizzaExpress-2.17.0-portable.zip
```

## Local Data

- Database: `%APPDATA%\PizzaExpress\orders.db`
- Logs: `%APPDATA%\PizzaExpress\Logs\`
- Auto-backups: `%APPDATA%\PizzaExpress\Backups\`

Use the Settings screen for manual backup and restore before editing or troubleshooting local data directly.

## Project Structure

```text
WindowsFormsApplication3/
  Config/              Compile-time defaults and constants
  Forms/               Secondary WinForms surfaces
  Infrastructure/      Migrations and backup services
  Models/              Order, customer, receipt, and report models
  Services/            Pricing, validation, receipts, repos, security, logging
  Form1.cs             Main POS form and workflow orchestration
  OrderHistoryForm.cs  History, search, void/delete, export

PizzaExpress.Tests/
  Tests/               Regression and smoke coverage

scripts/
  Run-Tests.ps1
  Package-PortableRelease.ps1
  Test-PortablePackage.ps1
```

## Troubleshooting

- `dotnet restore` fails:
  Confirm internet access to `https://api.nuget.org`, then retry from the repository root.
- Orders or settings look wrong:
  Inspect `%APPDATA%\PizzaExpress\orders.db` and restore from the latest backup if needed.
- CI passes locally but fails on GitHub:
  Check `.github/workflows/*.yml` path assumptions against the repo root.
- Portable package does not launch:
  Rebuild Release and re-run `Package-PortableRelease.ps1`; the release artifact must include the full Release output, not only the `.exe`.

## Known Limitations

- This is still a single-machine POS, not multi-terminal software.
- There is no inventory, kitchen display, or staff role model beyond the local PIN gate.
- Payment handling is reference-based only.
- `Form1.cs` still owns more workflow orchestration than an ideal long-term design.
- Accessibility has improved, but it is not yet verified with full assistive-tech testing.
- Coverage is gated in CI for the `WindowsFormsApplication3` package with a 75% line-rate threshold.

## Roadmap

- Keep shrinking high-risk logic out of `Form1.cs`
- Publish and verify the latest public release tag/assets when source has moved ahead of GitHub Releases
- Improve install experience beyond the portable ZIP
- Continue cleaning older doc/comment encoding artifacts

## Recent Release Highlights

| Version | Highlights |
|---|---|
| `v2.50.0` | 2 new tests: `SettingsForm` Restore button null-dir guard and Restore with dataDir cancel confirmation; 461 total, 92.2% |
| `v2.49.0` | 2 new tests: `ProcessCmdKey` Alt+W with valid settings opens SettingsForm; `OrderHistoryForm` ExportCsv empty-list guard; 459 total, 92.2% |
| `v2.48.0` | 8 new tests: 6 `ProcessCmdKey` branches (Alt+H/R/E/C/F1/Alt+W null-settings), 2 `cboPaymentMethod_SelectedIndexChanged` label/enabled assertions; 457 total, 92.2% |
| `v2.47.0` | 8 new tests: PinLoginForm locked-out guards, intermediate error messages, null-settings; Form1 null-settings OpenSettingsForm; 449 total, 92.2% |
| `v2.45.0` | 8 new tests: `PrintReceipt` via reflection, 3 `ProcessCmdKey` branches (Alt+K/Alt+Y/Escape), inline validation Leave handlers, ListView context menu Remove, 2 `SettingsForm` Save paths; 432 total, 92.2% |
| `v2.44.1` | Fix: replace clipboard-based receipt test (CI timeout) with Skip-button test; `DialogButtonClicker` helper retained; 424 total, 90.0% |
| `v2.44.0` | `DialogButtonClicker` helper + receipt dialog button coverage; 424 total |
| `v2.43.0` | 4 new tests: 3 Form1 Tab 2 button dialogs (History, Sales Report, End of Day) + SettingsForm CellBeginEdit/EndEdit lambdas; 423 total, coverage 89.9% |
| `v2.42.0` | 3 new `OrderHistoryForm` tests (column-click sort toggle, all sort columns, KeyDown Delete guard); covers `ListView_ColumnClick`, `SortOrders` lambda all branches; 419 total, coverage 89.5% |
| `v2.41.0` | 1 new `Form1` smoke test covering receipt-options dialog block (`showReceiptDialogs: true` path); `DialogAutoCloser` closes both sequential dialogs; covers `ResetFullForm`; 416 total, coverage 89.0% |
| `v2.40.0` | 3 new `Form1` smoke tests (Exit Yes closes form, About dialog, Keyboard Help via reflection); covers `btnExit_Click`, `ShowAboutDialog`, `ShowKeyboardHelp`; 415 total, coverage 87.5% |
| `v2.39.0` | 4 new `SalesReportForm` smoke tests (Run Report with orders, Today/This Week/This Month quick-date buttons); `SalesReportForm` 85.2%→95.1%; 412 total, coverage 86.1% |
| `v2.38.0` | 2 new `EndOfDayForm` tests: KPI/payment-list population with real orders (covers non-empty branches) and Print Report with `DialogAutoCloser`; `EndOfDayForm` 80%→95.6%; 408 total, coverage 85.5% |
| `v2.37.0` | 3 new `SettingsForm` smoke tests (Backup Unavailable dialog with null data dir, View Auto-Backups with no backups, View Auto-Backups with backups); `SettingsForm` 74.3%→80.9%; 406 total, coverage 84.4% |
| `v2.36.0` | 4 new `OrderHistoryForm` smoke tests (search filter, no-match row, view details dialog, date filter toggle); `OrderHistoryForm` 79.3%→87.9%; 403 total, coverage 83.9% |
| `v2.35.0` | 8 new keyboard tests for `PinLoginForm.OnKeyDown` (digit, numpad, backspace, delete, Enter, Escape, unhandled key, max-length guard); promoted to `internal`; `PinLoginForm` 73.6%→85.5%; 399 total, coverage 83.2% |
| `v2.34.0` | 5 new tests: 4 Form1 smoke tests (OrderAgain nav, GoBack nav, debit card checkout, cash underpayment) + OrderItem parameterless constructor; error dialogs in `btnPay_Click` given titles; 391 total, coverage 82.3% |
| `v2.33.0` | 11 new tests for `SettingsForm.GetDisplayValue` and `TrySaveStaffPin`; both promoted to `internal static`; 386 total, coverage 82.2% |
| `v2.32.0` | 2 new smoke tests: credit card checkout (masks card ref, persists order) and Clear Order confirm flow; Form1 coverage 63%→65%, overall 81.9% |
| `v2.31.0` | 24 new tests for `SettingsForm.FriendlyName` and `IsNumericKey`; methods promoted to `internal static`; 373 total, coverage 81.6% |
| `v2.30.0` | 15 new tests for `CheckoutWorkflowService` (constructor guards, `BuildOrderRecord`, `GetDeliveryMinutes` with settings, `ParseCurrencyOrZero` edge cases); 349 total |
| `v2.29.0` | 27 new tests for `PinSecurity` and `StaffAuthSession`; `StaffAuthSession` to 100%, `PinSecurity` to 97%; coverage up to 81.5% |
| `v2.28.0` | 25 new tests for `OrderSubmissionService` (0% to 100%) and `PaymentReferenceHelper` (83% to 100%); coverage up to 81.2% |
| `v2.27.0` | CI coverage gating: `dotnet-coverage` + `Check-Coverage.ps1` enforce 75% line-rate on `WindowsFormsApplication3`; `-CollectCoverage` flag on `Run-Tests.ps1` |
| `v2.26.0` | Fix ProcessCmdKey/mnemonic conflicts: Check Out now Alt+K (`Chec&k Out`), Pay now Alt+Y (`Pa&y`); both wired into ProcessCmdKey tab-aware; keyboard help updated |
| `v2.25.1` | Fix Check Out mnemonic conflict: `&Check Out` (Alt+C) corrected to `C&heck Out` (Alt+H); strengthen mnemonic test to assert exact text and uniqueness |
| `v2.25.0` | Accessibility pass on main order form: button mnemonics, label mnemonics, per-tab AcceptButton, decorative image exclusion, 4 new tests |
| `v2.24.0` | Destructive-admin smoke coverage: void, delete, backup/restore round-trip with real SQLite; `DialogAutoCloser` extended to dismiss Yes/No dialogs |
| `v2.23.0` | Extract checkout/payment flow from `Form1.cs` into `CheckoutWorkflowService`; 16 new tests; fix `OrderSubmissionService` missing `PaymentReference` |
| `v2.22.5` | Bump `actions/upload-artifact` to v7 and `softprops/action-gh-release` to v3; silence Node.js 20 deprecation warnings in CI |
| `v2.22.4` | Fix `release.yml`: add `permissions: contents: write` so `GITHUB_TOKEN` can create GitHub Releases; replace em dash in release body with ASCII `-` |
| `v2.22.3` | Fix checkout smoke tests still timing out on CI: post-submit receipt dialog is a custom `Form` (not `MessageBox`), so `WM_COMMAND/IDOK` never applied; tests use `showReceiptDialogs: false` internal constructor path instead |
| `v2.22.2` | Fix WinForms smoke tests failing on CI: `DialogAutoCloser` now sends `WM_COMMAND/IDOK` to dismiss single-OK-button `MessageBox` dialogs; `RunInSta` timeout raised from 90 s to 180 s |
| `v2.22.1` | Fix non-ASCII em dash in `Test-PortablePackage.ps1` that caused a parser error on Windows PowerShell 5.1 |
| `v2.22.0` | Install hardening: SHA256 checksum generation/verification, improved `PORTABLE-README.txt`, user-facing "Download and Run" section in README, `.sha256` uploaded as release asset |
| `v2.21.0` | CSV/print export unit tests (22 new); extracted `BuildHistoryCsv`, `BuildSalesReportCsv`, `BuildZReportCsv`, `BuildZReportText` as `internal static` builders; `InternalsVisibleTo` for test project |
| `v2.20.0` | WinForms smoke tests hardened to 235 passing; `[DoNotParallelize]`, improved `DialogAutoCloser`, `DatabaseBackupService` integration tests |
| `v2.17.0` | Payment references persisted end to end across checkout, receipts, history, and SQLite |
| `v2.16.0` | Daily rolling backups, manual backup/restore UI, startup auto-backup |
| `v2.15.0` | End-of-day reporting and better receipt printing |
| `v2.13.0` | SQLite-backed settings, dynamic prices, composition root |

See [CHANGELOG.md](CHANGELOG.md) for the full release history.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for workflow and code standards.

All pull requests should:

- build successfully
- pass the automated test suite
- update docs when behavior or setup changes
- add changelog notes under `[Unreleased]`

## Security

See [SECURITY.md](SECURITY.md) for responsible disclosure and trust-boundary notes.

## License

MIT. See [LICENSE](LICENSE).
