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

Verified on 2026-05-18 from this workspace and GitHub Actions.

- Current `master` / `origin/master`: `a96e55e42d0245a3efa1a909e847dec08ef2d957`.
- Commit title: `v2.44.1: fix receipt dialog test timeout (replace clipboard test with Skip button test)`.
- Build/Test workflow run: `26013196506`.
- Result: success.
- Debug build: passed.
- Tests with coverage: `424` total, `424` passed.
- Coverage gate: passed.
- App package coverage: `WindowsFormsApplication3` at `90%` line coverage against a `75%` threshold.
- Coverage artifact uploaded: `coverage-report`, artifact ID `7049461464`.
- Release build: passed.
- Portable package smoke test: passed for `PizzaExpress-2.44.1-portable.zip`.

## Public Release State

- `gh release list --limit 12` showed `v2.33.0` as the latest published GitHub Release.
- `gh release view v2.44.1` returned `release not found`.
- `git ls-remote --tags origin "refs/tags/v2.44.1"` returned no exact tag during verification.
- Therefore: source is green at `v2.44.1`, but public release/tag publication for `v2.44.1` was not verified.

Next release task: create/push the `v2.44.1` tag if still absent, let `release.yml` run, verify the GitHub Release exists, and confirm the portable ZIP plus `.sha256` assets are attached.

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
