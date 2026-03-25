# Contributing to Pizza Express NZ

Thank you for your interest in contributing!

---

## Workflow

1. **Fork** the repository and create a branch from `master`.
2. Name your branch: `feature/short-description` or `fix/short-description`.
3. Make your changes, keeping each commit focused and atomic.
4. **Run the tests** before pushing — all 95 tests must pass:
   ```powershell
   msbuild WindowsFormsApplication3.sln /p:Configuration=Debug
   vstest.console.exe PizzaExpress.Tests\bin\Debug\PizzaExpress.Tests.dll
   ```
5. **Update `CHANGELOG.md`** under `[Unreleased]` with a brief description of the change.
6. Open a Pull Request using the provided template. CI (GitHub Actions) must be green before merge.

---

## Code Standards

- **No magic strings or numbers** — use `AppConfig` constants.
- **New service methods** must have a corresponding interface method in `Services/I*.cs`.
- **Business logic** belongs in `Services/` — never in `Form1.cs`.
- **New public methods** must have an XML `<summary>` doc comment.
- **Zero new build warnings** — Roslyn NetAnalyzers and StyleCop are active on both projects.

---

## Commit Messages

```
type: short imperative description
```

| Type | When to use |
|---|---|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Code restructure, no behaviour change |
| `test` | Adding or fixing tests |
| `docs` | Documentation only |
| `ci` | CI/CD pipeline changes |

---

## Reporting Issues

Use the GitHub Issue Templates in `.github/ISSUE_TEMPLATE/`:
- **Bug Report** — steps to reproduce, expected vs actual behaviour, environment details
- **Feature Request** — problem statement and acceptance criteria
