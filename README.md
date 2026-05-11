# Pizza Express — New Zealand

[![Build and Test](https://github.com/ARSH871-bot/pizza-ordering-system/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/ARSH871-bot/pizza-ordering-system/actions/workflows/build-and-test.yml)
![Version](https://img.shields.io/badge/version-2.16.0-brightgreen)
![Tests](https://img.shields.io/badge/tests-207%20passing-success)
![Coverage](https://img.shields.io/badge/coverage-%3E70%25%20gated-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![Framework](https://img.shields.io/badge/.NET-4.8-purple)

A Windows Forms desktop POS (Point of Sale) application for **Pizza Express New Zealand**, built in C# (.NET Framework 4.8) with a clean three-layer architecture, SQLite-backed local persistence, and a fully automated CI pipeline.

---

## Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                           UI Layer                               │
│  Form1.cs          OrderHistoryForm.cs   SalesReportForm.cs      │
│  PinLoginForm.cs   SettingsForm.cs       EndOfDayForm.cs         │
│                   (WinForms — no business logic)                 │
└─────────────────────────┬────────────────────────────────────────┘
                          │ depends on interfaces
┌─────────────────────────▼────────────────────────────────────────┐
│                       Service Layer                              │
│  IPromoEngine       →  PromoEngine                               │
│  IOrderValidator    →  OrderValidator                            │
│  IReceiptWriter     →  ReceiptWriter                             │
│  IOrderRepository   →  OrderRepository (SQLite + Dapper)         │
│  ICartService       →  CartService (dynamic DB pricing)          │
│  ISettingsRepository → SettingsRepository (SQLite)               │
│  ILogger            →  FileLogger / NullLogger                   │
└─────────────────────────┬────────────────────────────────────────┘
                          │ operates on
┌─────────────────────────▼────────────────────────────────────────┐
│                       Domain Models                              │
│  Order │ OrderItem │ Customer │ OrderRecord │ OrderLineRecord     │
│  DailySummary │ TopItem │ PaymentSplit │ OrderSummary             │
│  AppConfig  (compile-time fallbacks; DB values take precedence)  │
└──────────────────────────────────────────────────────────────────┘
                          │ migrated by
┌─────────────────────────▼────────────────────────────────────────┐
│                    Infrastructure                                │
│  DatabaseMigrator  — SchemaHistory table; idempotent; no DbUp    │
└──────────────────────────────────────────────────────────────────┘
```

---

## Features

### Pizza & Menu
- 4 sizes: Small (NZD $4.00), Medium ($7.00), Large ($10.00), Extra Large ($13.00)
- 3 crust types: Normal, Cheesy, Sausage
- 14 toppings at $0.75 each
- Quantity selector (1–20) per pizza — price scales automatically
- **Multi-pizza cart** — stage each pizza independently before checkout
- Drinks: Coke, Diet Coke, Iced Tea, Ginger Ale, Sprite, Root Beer (default $1.45), Bottled Water (default $1.25)
- Extras: Chicken Wings, Poutine, Onion Rings, Cheesy Garlic Bread (default $3.00)

### Checkout & Validation
- **NZ localisation** — GST 15%, 16 NZ regions, 4-digit NZ postal code validation
- **Inline field validation** — visual green/red feedback on postal code, phone and email fields
- **Live running total** — subtotal + GST updates in real time as items are added
- Payment methods: Cash, Credit Card, Debit Card, Promo Card
- **Promo codes**: `PIZZA10` (10% off), `PIZZA20` (20% off), `FREESHIP` (free order)
- Non-cash payments use a reference field only; the app does not store real card numbers
- Change calculation; receipt export to `.txt`, copy to clipboard, or print preview
- Keyboard shortcuts: `Alt+C` confirm, `Alt+H` history, `Esc` back
- **Crash logger** — unhandled exceptions written to `%APPDATA%\PizzaExpress\Logs\crash_*.log`

### Order History & Reporting
- All confirmed orders persisted to `%APPDATA%\PizzaExpress\orders.db` (SQLite, ACID transactions)
- Automatic migration from legacy `orders.ndjson` and `orders.json` formats on first launch
- History viewer with Date, Customer, Region, Payment, Total, **Status** columns
- **SQL-backed live search** — text filters run a SQL `LIKE` query on CustomerName, Region, PaymentMethod; date range adds a `WHERE OrderDate BETWEEN` clause
- **Stats bar** — always-visible all-time summary: total orders, total revenue, average order value
- **Delete Order** — permanent removal with confirmation; **Void Order** — marks as Voided (excluded from revenue, kept for audit); voided rows rendered in italic grey
- Column sorting (click any header); CSV export of filtered results

### Sales Reports
- **Period Sales Report** (`Alt+R`) — KPI boxes + daily breakdown + top items + payment split for any date range; Today / This Week / This Month shortcuts; CSV export
- **End-of-Day Z-Report** (`Alt+E`) — cashier shift-close summary: orders, revenue, GST, average, payment-method reconciliation, top items; printable via print preview

### Administration
- **Settings form** (`Alt+W`) — pizza prices, topping price, drink prices, side price, and delivery time are stored in SQLite; changes take effect on the next order, no restart required
- **Staff PIN login** — configurable numeric PIN shown at startup; bypass when blank (single-operator mode); shake animation + keyboard support; set via Settings form
- **Dynamic delivery estimate** — order confirmation reads delivery minutes from the database setting rather than a compile-time constant

### Keyboard Shortcuts
- `F1` — keyboard shortcuts help overlay (full shortcut map)
- `Alt+C` confirm order · `Alt+P` checkout · `Alt+H` history
- `Alt+R` sales report · `Alt+E` end of day · `Alt+W` settings
- `Escape` navigate back · `Del` delete order · `V` void order

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# (.NET Framework 4.8) |
| UI | Windows Forms (WinForms) |
| Persistence | SQLite 1.0.118.0 + Dapper 2.1.35 (embedded, zero-install) |
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
cd pizza-ordering-system
```

Open `WindowsFormsApplication3.sln` in Visual Studio, press `F5`.

### Run Tests

```powershell
dotnet restore WindowsFormsApplication3.sln
dotnet build WindowsFormsApplication3.sln --configuration Debug
dotnet test PizzaExpress.Tests\PizzaExpress.Tests.csproj --configuration Debug --settings coverlet.runsettings --collect "XPlat Code Coverage"
```

---

## Project Structure

```
pizza-ordering-system/
├── WindowsFormsApplication3/
│   ├── Config/
│   │   └── AppConfig.cs              # All prices, tax rate, regions, promo codes
│   ├── Models/
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── OrderRecord.cs            # Serialisable snapshot for SQLite persistence
│   │   └── OrderSummary.cs           # Aggregate stats (count, revenue, avg)
│   ├── Forms/
│   │   ├── SettingsForm.cs           # Admin price/config editor (Alt+W)
│   │   ├── SalesReportForm.cs        # Period KPI + daily + items + payments (Alt+R)
│   │   ├── EndOfDayForm.cs           # Z-Report shift summary, printable (Alt+E)
│   │   └── PinLoginForm.cs           # Staff PIN numpad login dialog
│   ├── Infrastructure/
│   │   └── DatabaseMigrator.cs       # Lightweight migration runner (SchemaHistory)
│   ├── Models/
│   │   ├── Customer.cs
│   │   ├── Order.cs / OrderItem.cs
│   │   ├── OrderRecord.cs            # Serialisable snapshot; Status (Active/Voided)
│   │   ├── OrderSummary.cs           # Aggregate stats
│   │   └── ReportModels.cs           # DailySummary, TopItem, PaymentSplit
│   ├── Services/
│   │   ├── IPromoEngine.cs           # Interface → PromoEngine.cs
│   │   ├── IOrderValidator.cs        # Interface → OrderValidator.cs
│   │   ├── IReceiptWriter.cs         # Interface → ReceiptWriter.cs
│   │   ├── IOrderRepository.cs       # Interface → OrderRepository.cs (SQLite)
│   │   ├── ICartService.cs           # Interface → CartService.cs (dynamic pricing)
│   │   ├── ISettingsRepository.cs    # Interface → SettingsRepository.cs (SQLite)
│   │   ├── ILogger.cs                # Interface → FileLogger.cs / NullLogger.cs
│   │   └── ValidationResult.cs
│   ├── Form1.cs                      # Main POS form (UI only)
│   └── OrderHistoryForm.cs           # History viewer — search, filter, void, sort, CSV
├── PizzaExpress.Tests/
│   └── Tests/                        # automated regression tests
│       ├── OrderItemTests.cs
│       ├── OrderTests.cs
│       ├── CustomerTests.cs
│       ├── PromoEngineTests.cs
│       ├── OrderValidatorTests.cs
│       ├── ReceiptWriterTests.cs
│       ├── AppConfigTests.cs
│       ├── CartServiceTests.cs
│       ├── FileLoggerTests.cs
│       ├── OrderRepositoryTests.cs
│       └── ServiceInterfaceTests.cs
├── .github/
│   ├── workflows/
│   │   ├── build-and-test.yml        # CI: build, test, coverage gate (no third party)
│   │   └── release.yml               # Auto-publish .exe on vX.Y.Z tag
│   ├── dependabot.yml                # Weekly NuGet + Actions updates
│   ├── ISSUE_TEMPLATE/
│   └── pull_request_template.md
├── Directory.Build.props             # Shared LangVersion, Nullable, Deterministic, analyzers
├── Directory.Packages.props          # Central Package Management — all NuGet versions here
├── global.json                       # Pins .NET SDK to 9.0.x for reproducible builds
├── NuGet.Config                      # Explicit nuget.org-only package source
├── .editorconfig                     # Code style + Roslyn/StyleCop severity overrides
├── .gitattributes                    # CRLF normalisation
└── coverlet.runsettings              # OpenCover format, excludes test assembly
```

---

## Troubleshooting

- `dotnet restore` fails: confirm internet access to `https://api.nuget.org`, then retry from the repository root.
- Orders or settings look wrong: inspect `%APPDATA%\PizzaExpress\orders.db`; the app stores all local data there.
- A local build passes but GitHub Actions fails: check `.github/workflows/*.yml` for path drift relative to the repo root.

---

## Known Limitations

- This is a single-workstation, local-first POS; it does not include cloud sync or multi-terminal coordination.
- Payment handling is reference-based only; there is no payment gateway integration or PCI card storage.
- Packaging is currently a raw Windows executable release rather than an installer/MSI experience.

---

## Roadmap

- Broaden regression coverage around operator workflows and reporting edge cases.
- Improve release packaging and contributor automation while staying on free tooling.

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for full release history.

| Version | Highlights |
|---|---|
| **v2.16.0** | `DatabaseBackupService` · daily auto-backup on startup · Backup/Restore UI in `SettingsForm` · namespace closure fixes |
| **v2.13.0** | Composition root · `DatabaseMigrator` · `Settings` table · dynamic prices · `SettingsForm` admin UI |
| **v2.12.0** | Critical receipt pricing bug fixed · world-class dark-theme UI/UX overhaul (Form1 + OrderHistoryForm) · 177 tests |
| **v2.11.0** | Central Package Management (`Directory.Packages.props`) · tooltips on history controls |
| **v2.10.0** | `Directory.Build.props` · `global.json` · `NuGet.Config` · `Delete`/`Enter` keys in history |
| **v2.9.1** | Zero-warning CI gate (`-warnaserror`) · all StyleCop categories suppressed · 12 new CA suppressions |
| **v2.9.0** | Delete Order (button + right-click + confirmation) · SQLite indexes · 175 tests |
| **v2.8.0** | SQL-backed search · `GetSummary()` + stats bar in Order History · `OrderSummary` model · 170 tests |
| **v2.7.0** | SQLite + Dapper replaces NDJSON · SDK-style csproj · dotnet CLI in CI · NetAnalyzers 9.0 · 161 tests |
| **v2.6.0** | 16 new tests covering null customer, receipt fields, promo messages, empty file, Large pizza price · CONTRIBUTING.md updated · 161 tests |
| **v2.5.0** | Lifetime-free CI coverage gate · .gitattributes · NullLogger · FileLogger tests · ILogger/ICartService mocks · Release config · 145 tests |
| **v2.4.0** | CartService · live running total · input limits · accessibility · NDJSON append-only · ILogger/FileLogger · 123 tests |
| **v2.3.0** | Email validation · postal code masking · column sorting in history · crash logger · version in title bar · About dialog · 104 tests |
| **v2.2.0** | Service interfaces · Roslyn/StyleCop analyzers · NSubstitute mocks · 95 tests · Coverlet coverage · Dependabot |
| **v2.0.0** | Full architecture rewrite — Models / Services / AppConfig · 79 unit tests · GitHub Actions CI |
| **v1.3.0** | Pizza quantity selector; multi-pizza ordering |
| **v1.2.0** | Postal code & phone validation; promo codes; receipt export |

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for branching strategy and commit conventions.
Please use the [issue templates](.github/ISSUE_TEMPLATE/) for bug reports and feature requests.
All PRs must pass CI (build + automated tests + ≥70% line coverage) before merging.

## Security

See [SECURITY.md](SECURITY.md) for the responsible disclosure policy.

## License

MIT — see [LICENSE](LICENSE).
