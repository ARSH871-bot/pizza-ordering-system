# CLAUDE.md

Claude Code handoff for this repository.

## Read First

- Start with `AGENTS.md` for the full repo operating rules.
- Read `PROJECT_HANDOFF.md` before making claims about current CI/release status.
- This file is a short Claude-focused summary of the current architecture, the latest high-value changes, and the safest next steps.

## Project Summary

- Project: Pizza Express NZ POS / ordering system
- Stack: C# WinForms on .NET Framework 4.8
- Persistence: SQLite + Dapper
- Tests: MSTest + NSubstitute
- Coverage: `dotnet-coverage collect` wrapping `vstest.console.exe`
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
- `WinFormsTestHelper` has `DialogAutoCloser` (title-fragment + IDOK/IDYES/WM_CLOSE; also `respondNo: true` overload sending IDNO for YesNo dialogs) and `DialogButtonClicker` (finds child button by text, sends BM_CLICK) for modal-dialog testing.
- `Clipboard.SetText()` is unreliable in headless CI; never write tests that depend on it succeeding.
- `CheckoutWorkflowService` owns customer assembly, promo application, standard payment processing, order assembly, order-record assembly, and delivery-minutes resolution. `Form1` only reads controls and calls this service.
- `ICheckoutWorkflowService` + `CheckoutWorkflowService` live in `Services/`; tested via `CheckoutWorkflowServiceTests.cs` (16 tests).
- Portable release package includes a SHA256 sidecar and is published automatically on tag push.

## Current Verified Handoff

- Current working version: `v2.60.0` (local, pending CI). Previous verified baseline: `v2.59.0` (local 491/491).
- Local test run: `495/495` passed, coverage gate passed.
- `v2.44.0` failed because clipboard-dependent receipt-dialog smoke coverage timed out in headless CI. Do not reintroduce clipboard-dependent smoke tests.
- Next task: continue coverage improvements (v2.61.0+).
- Lesson: never call `form.Show()` + `PerformClick()` on buttons in `OrderHistoryForm` in a test that doesn't need the window visible — use reflection to invoke private methods directly.

## Validation Commands

Preferred:

```powershell
dotnet restore WindowsFormsApplication3.sln
dotnet build WindowsFormsApplication3.sln --configuration Debug
.\scripts\Run-Tests.ps1 -Configuration Debug
```

Expected: 495 tests passing. Coverage gate: 75% line-rate on WindowsFormsApplication3 (currently 94.6%+).

Coverage validation:

```powershell
dotnet tool install --global dotnet-coverage
.\scripts\Run-Tests.ps1 -Configuration Debug -ResultsDirectory ".\TestResults" -LogFileName "results.trx" -CollectCoverage -CoverageOutput ".\TestResults\coverage.xml"
.\scripts\Check-Coverage.ps1 -CoverageXml ".\TestResults\coverage.xml" -PackageFilter "WindowsFormsApplication3" -MinLineRate 0.75
```

Release validation:

```powershell
dotnet build WindowsFormsApplication3.sln --configuration Release --no-restore -v minimal
.\scripts\Run-Tests.ps1 -Configuration Release
.\scripts\Package-PortableRelease.ps1 -Configuration Release
.\scripts\Test-PortablePackage.ps1 -PackagePath .\artifacts\PizzaExpress-*-portable.zip
```

## Next Best Improvements

All originally planned improvements have been implemented. Meaningful next areas:

- Publish and verify the latest public release tag/assets if `v2.44.1` is still absent.
- Shrink remaining workflow logic from `Form1.cs` into services.
- Improve install experience beyond the portable ZIP.

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
