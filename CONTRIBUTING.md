# Contributing to Pizza Ordering System

Thank you for your interest in contributing. This document explains how to get started, what conventions to follow, and how to submit changes.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Development Setup](#development-setup)
3. [Branching Strategy](#branching-strategy)
4. [Commit Message Convention](#commit-message-convention)
5. [Submitting a Pull Request](#submitting-a-pull-request)
6. [Reporting Bugs](#reporting-bugs)
7. [Requesting Features](#requesting-features)
8. [Code Style](#code-style)

---

## Getting Started

1. **Fork** the repository on GitHub
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/pizza-ordering-system.git
   ```
3. Open `WindowsFormsApplication3.sln` in Visual Studio 2015 or later
4. Build the solution (`Ctrl+Shift+B`) to verify everything compiles

---

## Development Setup

| Requirement | Version |
|---|---|
| Visual Studio | 2015 or later |
| .NET Framework | 4.5 |
| OS | Windows |

No external packages or NuGet dependencies are required.

---

## Branching Strategy

| Branch | Purpose |
|---|---|
| `master` | Stable, release-ready code |
| `feature/US-XX-short-description` | New user story or feature |
| `fix/short-description` | Bug fix |
| `docs/short-description` | Documentation changes only |

Always branch off `master` and target `master` in your pull request.

```bash
git checkout master
git pull origin master
git checkout -b feature/US-30-your-feature-name
```

---

## Commit Message Convention

Follow this format:

```
type: short description (max 72 chars)

Optional longer explanation if needed.
```

| Type | When to use |
|---|---|
| `feat` | A new feature or user story |
| `fix` | A bug fix |
| `docs` | Documentation only changes |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `chore` | Build process, config, or tooling changes |

**Examples:**
```
feat: add order history screen (US-30)
fix: prevent crash when no payment method selected
docs: update README with screenshot
```

---

## Submitting a Pull Request

1. Push your branch to your fork:
   ```bash
   git push origin feature/US-XX-your-feature
   ```
2. Open a Pull Request against `master` on the main repository
3. Fill in the PR template completely
4. Link the relevant GitHub Issue in the PR description (e.g. `Closes #30`)
5. Ensure the project builds successfully before submitting
6. Wait for review — do not merge your own PR

---

## Reporting Bugs

Open a GitHub Issue using the **Bug Report** template. Include:

- Steps to reproduce the bug exactly
- What you expected to happen
- What actually happened
- Your Visual Studio and Windows version

---

## Requesting Features

Open a GitHub Issue using the **Feature Request** template. Include:

- A clear description of the feature
- The problem it solves or the user story it satisfies
- Any mockups or examples if applicable

---

## Code Style

- Follow existing C# naming and formatting conventions already in the codebase
- Use `int.TryParse` instead of `Convert.ToInt32` for any user input parsing
- Use `.ToString("F2")` for monetary values stored in list columns
- Use `.ToString("c2")` for monetary values displayed to the user
- Do not commit build artifacts (`bin/`, `obj/`, `.vs/`) — they are in `.gitignore`
- Keep event handlers small; extract logic into named methods if a handler exceeds ~20 lines
