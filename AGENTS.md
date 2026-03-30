# AGENTS.md

## Purpose

Repository guidance for humans and coding agents working on Pizza Express NZ.

This project is a production-style, zero-budget, Windows-only desktop POS built with:

- C# WinForms on .NET Framework 4.8
- SQLite + Dapper for persistence
- MSTest + NSubstitute + Coverlet for validation
- GitHub Actions free-tier workflows only

Do not rewrite the stack. Prefer incremental improvements that preserve existing behavior unless fixing a bug or reconciling a documented inconsistency.

## Architecture Overview

- `WindowsFormsApplication3/Form1.cs`
  Main POS workflow UI. Keep orchestration here, but move business rules into services when logic becomes reusable or hard to test.
- `WindowsFormsApplication3/Forms/`
  Secondary UI surfaces: settings, order history, sales report, end-of-day, PIN login.
- `WindowsFormsApplication3/Services/`
  Business logic and infrastructure-facing services. This is the default home for pricing rules, promo logic, validation, receipts, and repository behavior.
- `WindowsFormsApplication3/Models/`
  Domain/data-transfer models used across UI and services.
- `WindowsFormsApplication3/Infrastructure/DatabaseMigrator.cs`
  SQLite schema bootstrap + idempotent migrations. Safe on first launch and on repeat runs.
- `PizzaExpress.Tests/Tests/`
  Regression suite. Add tests for every bug fix or business-rule change.

## Non-Negotiable Constraints

- Free tools only. No paid APIs, hosted services, monitoring, scanners, or CI add-ons.
- No blind rewrites or stack swaps.
- Preserve local-data compatibility where practical.
- Keep changes reviewable and incremental.
- If docs, tests, issues, and runtime behavior disagree, fix the mismatch.

## Coding Standards

- Prefer small service-oriented refactors over moving more logic into WinForms event handlers.
- Keep SQL parameterized. Do not build SQL from user input.
- Keep SQLite operations transactional when multiple tables must stay in sync.
- Enable or preserve foreign-key safety when opening SQLite connections.
- Use `AppConfig` as the fallback source of truth for default values, with settings overrides where the product already supports runtime configuration.
- Avoid hardcoded claims in UI/docs that are likely to drift, especially exact test counts.
- Add XML docs for new public members when they materially help maintainability.

## Validation Commands

Run from the repository root.

Preferred local flow:

```powershell
dotnet restore WindowsFormsApplication3.sln
dotnet build WindowsFormsApplication3.sln --configuration Debug
dotnet test PizzaExpress.Tests/PizzaExpress.Tests.csproj --configuration Debug --settings coverlet.runsettings --collect "XPlat Code Coverage"
```

Constrained/offline fallback used in this repo when restore is unavailable:

```powershell
dotnet build WindowsFormsApplication3\WindowsFormsApplication3.csproj --no-restore --configuration Debug -v minimal
dotnet build PizzaExpress.Tests\PizzaExpress.Tests.csproj --no-restore --configuration Debug -v minimal
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' '.\PizzaExpress.Tests\bin\Debug\net48\PizzaExpress.Tests.dll' /Logger:trx
```

## Release / PR Checklist

- Build succeeds locally.
- Automated tests pass locally.
- New or changed business logic has regression coverage.
- UI behavior changes include manual verification notes.
- `CHANGELOG.md` has an `[Unreleased]` entry.
- `README.md`, `USER_STORIES.md`, and contributor guidance are updated if behavior or setup changed.
- GitHub Actions workflow paths still match the repository layout.

## Docs Synchronization Rules

- Do not hardcode test totals unless you validated and updated every place that mentions them.
- Keep README feature lists truthful to the current app, not aspirational.
- Keep `USER_STORIES.md` aligned with actual implementation status.
- Keep changelog entries focused on real shipped behavior, not intent.

## Safe Refactoring Rules

- Favor extracting logic into `Services/` over reworking designer files.
- Do not rename persisted SQLite columns without a migration path.
- Re-saving an order must not duplicate child rows.
- Promo/discount changes must be validated across:
  receipt output, persisted totals, history, summary reporting, and payment/change calculations.
- Pricing changes must be validated across:
  live total, order review, receipt, persistence, and settings.

## Product-Specific Notes

- US-30 pizza quantity selection and US-31 multi-pizza ordering are implemented and should stay regression-covered.
- Non-cash checkout uses a reference/promo entry field; do not introduce full card-number storage.
- Local data lives under `%APPDATA%\\PizzaExpress`.
