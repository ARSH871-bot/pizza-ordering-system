# Pizza Express — New Zealand

[![Build and Test](https://github.com/ARSH871-bot/pizza-ordering-system/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/ARSH871-bot/pizza-ordering-system/actions/workflows/build-and-test.yml)
![Version](https://img.shields.io/badge/version-2.3.0-brightgreen)
![Tests](https://img.shields.io/badge/tests-104%20passing-success)
![Coverage](https://img.shields.io/badge/coverage-%3E80%25-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![Framework](https://img.shields.io/badge/.NET-4.8-purple)

A Windows Forms desktop POS (Point of Sale) application for **Pizza Express New Zealand**, built in C# (.NET Framework 4.8) with a clean three-layer architecture, 95 unit + integration tests, and a fully automated CI pipeline.

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                     UI Layer                        │
│   Form1.cs  │  OrderHistoryForm.cs                  │
│             (WinForms — no business logic)          │
└────────────────────┬────────────────────────────────┘
                     │ depends on interfaces
┌────────────────────▼────────────────────────────────┐
│                  Service Layer                      │
│  IPromoEngine      →  PromoEngine                   │
│  IOrderValidator   →  OrderValidator                │
│  IReceiptWriter    →  ReceiptWriter                 │
│  IOrderRepository  →  OrderRepository (JSON)        │
└────────────────────┬────────────────────────────────┘
                     │ operates on
┌────────────────────▼────────────────────────────────┐
│                  Domain Models                      │
│  Order  │  OrderItem  │  Customer                   │
│  OrderRecord  │  OrderLineRecord                    │
│  AppConfig  (single source of truth for all values) │
└─────────────────────────────────────────────────────┘
```

---

## Features

### Pizza & Menu
- 4 sizes: Small (NZD $4.00), Medium ($7.00), Large ($10.00), Extra Large ($13.00)
- 3 crust types: Normal, Cheesy, Sausage
- 14 toppings at $0.75 each
- Quantity selector (1–20) per pizza — price scales automatically
- **Multi-pizza cart** — stage each pizza independently before checkout
- Drinks: Coke, Diet Coke, Iced Tea, Ginger Ale, Sprite, Root Beer ($1.45), Bottled Water ($1.25)
- Extras: Chicken Wings, Poutine, Onion Rings, Cheesy Garlic Bread ($3.00)

### Checkout & Validation
- **NZ localisation** — GST 15%, 16 NZ regions, 4-digit NZ postal code validation
- **Inline field validation** — visual green/red feedback on postal code and phone fields
- Payment methods: Cash, Credit Card, Debit Card, Promo Card
- **Promo codes**: `PIZZA10` (10% off), `PIZZA20` (20% off), `FREESHIP` (free order)
- Change calculation, receipt export to `.txt`

### Order History
- All confirmed orders persisted to `%APPDATA%\PizzaExpress\orders.json`
- History viewer with Date, Customer, Region, Payment, Total columns
- "View Details" shows full order breakdown

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# (.NET Framework 4.8) |
| UI | Windows Forms (WinForms) |
| Persistence | JSON via `JavaScriptSerializer` |
| Testing | MSTest 3.3.1 · NSubstitute 5.1.0 · Coverlet 6.0.2 |
| Static Analysis | Microsoft.CodeAnalysis.NetAnalyzers 8.0 · StyleCop.Analyzers |
| CI/CD | GitHub Actions (windows-latest) |

---

## Getting Started

### Prerequisites
- Windows 10/11
- Visual Studio 2022 (Community — free)
- .NET Framework 4.8 (pre-installed on Windows 10/11)

### Run Locally

```bash
git clone https://github.com/ARSH871-bot/pizza-ordering-system.git
cd pizza-ordering-system/PizzaOrderingSystemC#
```

Open `WindowsFormsApplication3.sln` in Visual Studio, press `F5`.

### Run Tests

```powershell
msbuild WindowsFormsApplication3.sln /p:Configuration=Debug
vstest.console.exe PizzaExpress.Tests\bin\Debug\PizzaExpress.Tests.dll
```

---

## Project Structure

```
PizzaOrderingSystemC#/
├── WindowsFormsApplication3/
│   ├── Config/
│   │   └── AppConfig.cs           # All prices, tax rate, regions, promo codes
│   ├── Models/
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   └── OrderRecord.cs         # Serialisable snapshot for JSON persistence
│   ├── Services/
│   │   ├── IPromoEngine.cs        # Interface → PromoEngine.cs
│   │   ├── IOrderValidator.cs     # Interface → OrderValidator.cs
│   │   ├── IReceiptWriter.cs      # Interface → ReceiptWriter.cs
│   │   ├── IOrderRepository.cs    # Interface → OrderRepository.cs
│   │   └── ValidationResult.cs
│   ├── Form1.cs                   # Main POS form (UI only)
│   └── OrderHistoryForm.cs        # Order history viewer
├── PizzaExpress.Tests/
│   └── Tests/                     # 95 tests across 9 test classes
├── .github/
│   ├── workflows/build-and-test.yml
│   ├── dependabot.yml
│   ├── ISSUE_TEMPLATE/
│   └── pull_request_template.md
└── .editorconfig                  # Code style rules
```

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for full release history.

| Version | Highlights |
|---|---|
| **v2.3.0** | Email validation · postal code masking · column sorting in history · crash logger · version in title bar · About dialog · 104 tests |
| **v2.2.0** | Service interfaces · Roslyn/StyleCop analyzers · NSubstitute mocks · 95 tests · Coverlet coverage · Dependabot |
| **v2.0.0** | Full architecture rewrite — Models / Services / AppConfig · 79 unit tests · GitHub Actions CI |
| **v1.3.0** | Pizza quantity selector; multi-pizza ordering |
| **v1.2.0** | Postal code & phone validation; promo codes; receipt export |

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for branching strategy and commit conventions.
Please use the [issue templates](.github/ISSUE_TEMPLATE/) for bug reports and feature requests.
All PRs must pass CI (build + 95 tests) before merging.

## Security

See [SECURITY.md](SECURITY.md) for the responsible disclosure policy.

## License

MIT — see [LICENSE](LICENSE).
