# CLAUDE.md

Claude Code handoff for this repository.

## Read First

- Start with `AGENTS.md` for the full repo operating rules.
- This file is a short Claude-focused summary of the current architecture, the latest high-value changes, and the safest next steps.

## Project Summary

- Project: Pizza Express NZ POS / ordering system
- Stack: C# WinForms on .NET Framework 4.8
- Persistence: SQLite + Dapper
- Tests: MSTest + NSubstitute + Coverlet
- Constraints: zero-budget, no paid tools/services, no blind rewrites

## Architecture

- `WindowsFormsApplication3/Form1.cs`
  Main POS workflow and UI orchestration.
- `WindowsFormsApplication3/Forms/`
  Secondary UI: settings, order history, sales report, end-of-day, PIN login.
- `WindowsFormsApplication3/Services/`
  Business logic, pricing, validation, receipts, repositories.
- `WindowsFormsApplication3/Models/`
  Domain and persistence models.
- `WindowsFormsApplication3/Infrastructure/DatabaseMigrator.cs`
  SQLite bootstrap + idempotent schema migrations.

## Important Current Truths

- US-30 pizza quantity selection is implemented.
- US-31 ordering multiple different pizzas is implemented through `_stagedPizzas` and `btnAddPizzaToCart`.
- Non-cash checkout now uses a reference/promo field only. Do not add full card-number storage.
- Runtime settings now affect pizza, topping, canned drink, water, side, and delivery pricing/behavior.

## Changes Completed In This Pass

### Correctness and Product Flow

- Fixed a build-breaking issue in `SalesReportForm`.
- Added discount-aware order modeling:
  - `Order.Discount`
  - `Order.DiscountDescription`
  - `Order.DeliveryMinutes`
  - `Order.AmountDue`
- Promo checkout now persists the discounted total instead of only changing UI text.
- Receipts now show discounts correctly and use the discounted amount due plus runtime delivery minutes.
- `Form1` now resets stale payment state when order/payment inputs change.

### Pricing and UX Integrity

- `ICartService` / `CartService` now expose settings-backed prices for:
  - canned drinks
  - bottled water
  - side items
- `Form1` live-total and confirm-order flows now use those runtime prices instead of hardcoded `AppConfig` values.
- The non-cash label changed from card-number wording to `Reference:` to avoid encouraging PAN entry.

### Persistence and Migration Safety

- `DatabaseMigrator` was rewritten to be safe on a brand-new database:
  - creates core tables first
  - enables foreign keys
  - seeds defaults safely
  - applies later columns idempotently
- `OrderRepository` now:
  - runs against the migration-based schema
  - enables SQLite foreign keys on open
  - replaces existing order lines on re-save instead of duplicating them
  - persists discount fields
  - excludes voided orders from all-time summary revenue
  - uses parameterized line loading for matched order ids

### Tests and Operations

- Added regression tests for:
  - fresh database migration
  - idempotent migration reruns
  - discount-aware totals
  - discount-aware receipts
  - settings-backed prices
  - repository line replacement on re-save
  - discount persistence
  - voided-order exclusion from summary
- Current validated test total: 207 passing.
- GitHub Actions workflows were fixed to use the actual repo-root paths and updated free action versions:
  - `actions/checkout@v6`
  - `actions/setup-dotnet@v5`
  - `actions/upload-artifact@v6`

### Documentation

- Added `AGENTS.md`.
- Updated `README.md`, `CHANGELOG.md`, `CONTRIBUTING.md`, `SECURITY.md`, and `USER_STORIES.md` to match actual behavior.
- Removed stale hardcoded test-count claims from operational guidance and UI copy where possible.

## Validation Commands

Preferred:

```powershell
dotnet restore WindowsFormsApplication3.sln
dotnet build WindowsFormsApplication3.sln --configuration Debug
dotnet test PizzaExpress.Tests/PizzaExpress.Tests.csproj --configuration Debug --settings coverlet.runsettings --collect "XPlat Code Coverage"
```

Offline/constrained fallback used during this pass:

```powershell
dotnet build WindowsFormsApplication3\WindowsFormsApplication3.csproj --no-restore --configuration Debug -v minimal
dotnet build PizzaExpress.Tests\PizzaExpress.Tests.csproj --no-restore --configuration Debug -v minimal
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' '.\PizzaExpress.Tests\bin\Debug\net48\PizzaExpress.Tests.dll' /Logger:trx
```

## Next Best Improvements

- Add backup/export and recovery flows for the local SQLite database.
- Add stronger UI-level smoke coverage for multi-pizza checkout and settings changes.
- Improve release packaging beyond a raw `.exe`.
- Clean up older mojibake/encoding artifacts in legacy docs/comments.

## Guardrails For Future Claude Sessions

- Do not rewrite the stack.
- Keep business logic moving out of UI code, not the reverse.
- Keep docs, tests, issues, and behavior synchronized.
- Any pricing, promo, persistence, or reporting change must be validated end to end:
  - live total
  - order review
  - receipt
  - persisted history
  - reporting/summary
