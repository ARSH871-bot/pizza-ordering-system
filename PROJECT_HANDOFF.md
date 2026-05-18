# Project Handoff

This file is the durable "start here if context is lost" handoff for Pizza Express NZ. Keep it truthful, short, and evidence-based.

## Project Identity

- Product: Pizza Express NZ POS / ordering system.
- Stack: C# WinForms on .NET Framework 4.8.
- Persistence: SQLite + Dapper.
- Tests: MSTest + NSubstitute.
- Coverage: `dotnet-coverage collect` wrapping `vstest.console.exe`; Coverlet paths are stale for this stack.
- Operating constraint: zero-budget only. No paid services, paid APIs, hosted monitoring, or blind rewrites.
- Honest tier: serious prototype / strong portfolio project moving toward early MVP credibility, not production-grade small-business POS yet.

## Source Of Truth Files

- `PROJECT_HANDOFF.md`: current verified state and next-best path.
- `AGENTS.md`: general repo rules for all coding agents.
- `CLAUDE.md`: Claude-specific short handoff. Keep it aligned with `AGENTS.md` and this file.
- `README.md`: public user/contributor overview.
- `CHANGELOG.md`: chronological project history. Verify claims before repeating them.
- `USER_STORIES.md`: story-level product intent and implemented status.

If these disagree, trust fresh code/test/workflow evidence first, then update the docs.

## Latest Verified State

Verified on 2026-05-18 from this workspace.

- Current `master` / `origin/master`: `v2.52.0` (CI green, run 26030731842).
- Commit title: `v2.52.0: fix ExportCsv empty-list guard + 3 new coverage tests; 465 total`.
- Previous CI-verified baseline: `v2.48.0`. v2.49.0–v2.51.0 failed; v2.52.0 is the new verified baseline.
- Local pre-push validation: Debug 465/465 passed, coverage gate passed.
- Coverage gate: passed (75% threshold, 92.2%+ actual).

## Public Release State

- `v2.52.0` CI green (run 26030731842). New verified baseline.
- Next task: continue coverage improvements (v2.53.0+).

## Important Recent History

- `v2.44.0` commit `7fd812e504e1c015c5d856ac97a1c83719cb2807` failed Build/Test run `26012828961`.
- Failure: `Form1_ReceiptDialog_CopyToClipboard_ThenSkip_CompletesOrder` timed out after 180 seconds in `WinFormsTestHelper.RunInSta`.
- Root cause: clipboard-dependent WinForms smoke coverage is unreliable in headless Windows CI.
- `v2.44.1` replaced that fragile clipboard path with a Skip-button receipt dialog test and restored green CI.
- Future tests should not depend on `Clipboard.SetText()` succeeding in GitHub Actions.

## Validation Commands

Run from the repository root.

```powershell
dotnet restore WindowsFormsApplication3.sln
dotnet build WindowsFormsApplication3.sln --configuration Debug
.\scripts\Run-Tests.ps1 -Configuration Debug
```

Coverage validation:

```powershell
dotnet tool install --global dotnet-coverage
.\scripts\Run-Tests.ps1 -Configuration Debug -ResultsDirectory ".\TestResults" -LogFileName "results.trx" -CollectCoverage -CoverageOutput ".\TestResults\coverage.xml"
.\scripts\Check-Coverage.ps1 -CoverageXml ".\TestResults\coverage.xml" -PackageFilter "WindowsFormsApplication3" -MinLineRate 0.75
```

Portable release validation:

```powershell
dotnet build WindowsFormsApplication3.sln --configuration Release --no-restore -v minimal
.\scripts\Run-Tests.ps1 -Configuration Release
.\scripts\Package-PortableRelease.ps1 -Configuration Release
.\scripts\Test-PortablePackage.ps1 -PackagePath .\artifacts\PizzaExpress-*-portable.zip
```

Remote verification:

```powershell
gh run list --workflow=build-and-test.yml --limit 5
gh run view <run-id> --log
gh release view <tag>
```

## Architecture Notes

- `WindowsFormsApplication3/Form1.cs` is still the main POS UI/workflow surface. Keep shrinking high-risk orchestration out of it, but do not rewrite the UI.
- `WindowsFormsApplication3/Services/CheckoutWorkflowService.cs` owns customer assembly, promo application, payment processing, order assembly, order-record mapping, and delivery-minute resolution.
- `WindowsFormsApplication3/Services/OrderSubmissionService.cs` coordinates order persistence and receipt generation.
- `WindowsFormsApplication3/Services/PaymentReferenceHelper.cs` normalizes and masks non-cash payment references.
- `WindowsFormsApplication3/Infrastructure/DatabaseMigrator.cs` owns SQLite bootstrap/migrations.
- `WindowsFormsApplication3/Infrastructure/DatabaseBackupService.cs` owns daily auto-backups, manual backup, restore, safety copy, and DB-size helpers.
- `PizzaExpress.Tests/Tests/WinFormsTestHelper.cs` contains STA/dialog helpers for smoke tests.

## Product Truths To Preserve

- US-30 pizza quantity selection is implemented.
- US-31 ordering multiple different pizzas is implemented.
- Non-cash payment stores only a masked/reference-style identifier; never store full card numbers.
- Staff PINs are PBKDF2-protected; legacy plaintext PINs should upgrade rather than remain.
- Sensitive actions use staff re-auth/recent-auth boundaries where implemented.
- Local data lives under `%APPDATA%\PizzaExpress`.
- Portable ZIP is the current delivery format; there is no installer yet.
- This is single-workstation software; no cloud sync, multi-register coordination, payment gateway, inventory, or staff role model beyond local PIN.

## Next Best Work

1. Publish and verify the missing/latest public release tag if it is still absent.
2. Keep shrinking `Form1.cs` workflow logic into focused services.
3. Improve install experience beyond the portable ZIP.
4. Add focused tests only where they raise trust; avoid brittle WinForms automation that depends on clipboard, printer, or OS dialogs.
5. Continue docs truth cleanup, especially stale release/test/coverage statements.

## Agent Guardrails

- Verify before claiming green.
- Do not trust badges, changelog entries, or prior chat summaries without checking code, tests, and workflows.
- Use `rg` / `rg --files` for search.
- Use `apply_patch` for manual edits.
- Do not revert unrelated dirty files.
- Do not add non-ASCII characters to PowerShell scripts or YAML files.
- Do not add `Co-Authored-By` trailers unless explicitly requested.
