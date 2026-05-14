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
- Non-cash checkout uses a reference/promo field only. Do not add full card-number storage.
- Runtime settings affect pizza, topping, canned drink, water, side, and delivery pricing/behavior.
- Staff PINs are PBKDF2-protected; legacy plaintext PINs upgrade on successful login; recent staff auth is reused for Settings plus history void/delete actions.
- CSV/print export content builders are extracted as `internal static` methods and covered by unit tests.
- `Form1` has an `internal` constructor overload with `showReceiptDialogs = false` used by smoke tests.
- Both CI workflows (`build-and-test.yml`, `release.yml`) are fully green as of v2.22.5.
- `CheckoutWorkflowService` owns customer assembly, promo application, standard payment processing, order assembly, order-record assembly, and delivery-minutes resolution. `Form1` only reads controls and calls this service.
- `ICheckoutWorkflowService` + `CheckoutWorkflowService` live in `Services/`; tested via `CheckoutWorkflowServiceTests.cs` (16 tests).
- Portable release package includes a SHA256 sidecar and is published automatically on tag push.

## Validation Commands

Preferred:

```powershell
dotnet restore WindowsFormsApplication3.sln
dotnet build WindowsFormsApplication3.sln --configuration Debug
.\scripts\Run-Tests.ps1 -Configuration Debug
```

Expected: 273 tests passing.

Release validation:

```powershell
dotnet build WindowsFormsApplication3.sln --configuration Release --no-restore -v minimal
.\scripts\Run-Tests.ps1 -Configuration Release
.\scripts\Package-PortableRelease.ps1 -Configuration Release
.\scripts\Test-PortablePackage.ps1 -PackagePath .\artifacts\PizzaExpress-*-portable.zip
```

## Next Best Improvements

Remaining meaningful improvements:

- Add more destructive-admin smoke coverage (void, delete, backup/restore round-trip via UI).
- Accessibility pass: keyboard navigation and screen-reader labels on the main order form.

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
- Do not add non-ASCII characters to PowerShell scripts or YAML files.
- Do not include Co-Authored-By trailers in git commit messages.
