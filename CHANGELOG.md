# Changelog

All notable changes to this project are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]

---

## [2.3.0] — 2026-03-25

### Added

- **Email validation** — `IOrderValidator.ValidateEmail()` and `OrderValidator.ValidateEmail()` validate optional email format (`local@domain.tld`). `ValidateCustomer()` now also checks email. Inline green/red feedback on the Email field `Leave` event.
- **9 new email validation tests** — null, empty, whitespace (all valid/optional), standard address, subdomain address, missing `@`, missing domain, missing TLD, error message content. **Total tests: 95 → 104**.
- **Postal code input masking** — `txtPostalCode` now accepts digits only (non-digit keys silently blocked) and is capped at 4 characters, making invalid input impossible to type.
- **Global unhandled exception handler** in `Program.cs` — catches exceptions on both the UI thread (`Application.ThreadException`) and background threads (`AppDomain.UnhandledException`). Writes a timestamped crash log to `%APPDATA%\PizzaExpress\Logs\crash_*.log` and shows a user-friendly error dialog instead of a raw .NET crash box.
- **Version in title bar** — title now shows `Pizza Express New Zealand — v2.3.0` using `Application.ProductVersion` (reads from `AssemblyFileVersion`).
- **About dialog** — new "About" button on Tab 2 shows version, architecture summary, test count, CI note, and the local data directory path.
- **Column header sorting in Order History** — clicking any column header sorts by that column; clicking again toggles ascending/descending. Sort direction indicated by ▲/▼ arrow in the header text.
- **`AssemblyInfo.cs` updated** — `AssemblyTitle`, `AssemblyProduct`, `AssemblyCompany`, `AssemblyCopyright`, `AssemblyVersion`, and `AssemblyFileVersion` all set to reflect Pizza Express NZ v2.3.0.
- **`CONTRIBUTING.md` rewritten** — reflects current 104-test suite, interface-first architecture rule, StyleCop/Roslyn zero-warning requirement, and conventional commit format table.

---

## [2.2.0] — 2026-03-25

### Added

- **Service interfaces** — `IPromoEngine`, `IOrderValidator`, `IReceiptWriter`, `IOrderRepository` introduced; all service fields in `Form1` and `OrderHistoryForm` now use the interface type. Services are fully swappable without touching UI code.
- **Roslyn NetAnalyzers 8.0** — ~200 FxCop-style rules run at compile time on both projects (warnings surfaced in Visual Studio Error List).
- **StyleCop.Analyzers** — enforces consistent C# naming, file layout, and spacing rules across the codebase.
- **`.editorconfig`** — root-level config: 4-space indent, UTF-8-BOM, trailing-whitespace trimming, `_camelCase` private field convention; analyzer severity overrides tuned for WinForms desktop.
- **NSubstitute 5.1.0** — mocking library added to test project; Castle.Core 5.1.1 included as required proxy-generation dependency.
- **`ServiceInterfaceTests.cs`** — 9 new tests confirming every interface is correctly implemented by its concrete class and that NSubstitute can substitute each interface.
- **`OrderRepositoryTests.cs`** — 7 integration tests for `OrderRepository` using an isolated temp directory (never touches real user data). Covers save, multi-record persistence, field round-trip, missing file, corrupted file, directory auto-creation, and null guard.
- **Coverlet 6.0.2** — code coverage collector added to test project.
- **`release.yml`** CI workflow — automatically builds a Release configuration, runs all tests, and publishes the compiled `.exe` as a GitHub Release asset when a `v*.*.*` tag is pushed.
- **Dependabot** — weekly automated PRs for stale NuGet packages and GitHub Actions versions.
- **Status bar** — live strip at the bottom of the main window showing cart item count and running total (incl. GST) in real time.
- **Tooltips** — every non-obvious field now shows a concise hint on hover (postal code format, contact number rules, amount paid, etc.).
- **Confirm before clearing** — "Clear Order" now shows a warning dialog; cancellable.
- **Remove item from cart** — right-click any item in the Order Review ListView and choose "Remove selected item"; status bar updates instantly.
- **Keyboard shortcuts** — `Alt+C` (Confirm Order), `Alt+H` (Order History), `Alt+P` (Proceed to Payment), `Esc` (navigate back a tab).
- **Print Receipt** — "Print" button in the post-order receipt dialog; opens a full `PrintPreviewDialog` using `Courier New` monospace font.
- **Copy Receipt to Clipboard** — one-click copy of the full formatted receipt text.
- **Order History: live search** — text box filters by customer name, region, payment method, or date while typing.
- **Order History: date range filter** — optional From/To date pickers narrow the history list.
- **Order History: result count** — label shows how many orders match the current filter.
- **Order History: CSV export** — "Export CSV" button saves all visible rows to a `.csv` file (UTF-8).
- **XML `<summary>` doc comments** on all public models, service interfaces, and `OrderHistoryForm`.
- **GitHub issue templates** (Bug Report + Feature Request) under `.github/ISSUE_TEMPLATE/`.
- **PR template** — checklist enforcing tests, CHANGELOG update, and zero magic strings.
- **`SECURITY.md`** — responsible disclosure policy.
- **`CODE_OF_CONDUCT.md`** — Contributor Covenant v2.1.
- **`README.md`** completely rewritten — ASCII architecture diagram, full feature list, updated tech stack table, updated test count (95), project structure tree, and all new badges.
- **`OrderRepository` injectable data path** — constructor overload accepting a custom directory; enables clean integration tests without redirecting `%APPDATA%`.

### Changed

- Total test count: **79 → 95** (16 new tests across 2 new test classes).
- `OrderRepository` refactored from static file path fields to instance fields; data directory now injectable via constructor.

---

## [2.1.0] — 2026-03-24

### Added

- **Order History persistence** — every submitted order is automatically saved to `%APPDATA%\PizzaExpress\orders.json` using `System.Web.Script.Serialization.JavaScriptSerializer` (built-in, no extra NuGet packages). Records include a short unique ID, timestamp, customer details, itemised lines, and totals.
- **Order History viewer** — a new "Order History" button on the Order Review tab opens a resizable dialog listing all past orders (most-recent first) with Date/Time, Customer, Region, Payment, and Total columns. Double-clicking or pressing "View Details" shows the full formatted receipt in a message box.
- **Inline field validation with visual feedback** — the Postal Code and Contact Number fields now colour their background on `Leave`:
  - Green (Honeydew) when the value passes validation
  - Red (MistyRose) when it fails
  - Background resets to the system default on "Order Again"
- **`OrderRecord` / `OrderLineRecord` models** — lightweight, serialisable snapshots decoupled from the live `Order` domain model.
- **`OrderRepository` service** — encapsulates all JSON read/write logic; creates the data directory on first use; gracefully returns an empty list if the file is missing or corrupt.
- **`OrderHistoryForm`** — fully code-constructed WinForms dialog (no `.Designer.cs`).

---

## [2.0.0] — 2026-03-24

### Added

- Three-layer architecture: domain **Models** (`Order`, `OrderItem`, `Customer`, `PizzaSize`, `CrustType`), **Config** (`AppConfig`), and **Services** (`PromoEngine`, `OrderValidator`, `ReceiptWriter`, `ValidationResult`, `PromoResult`).
- `AppConfig` as single source of truth for all constants (tax rate, prices, regions, promo codes, payment methods, delivery time).
- **MSTest unit-test project** (`PizzaExpress.Tests`) — 79 tests covering all service classes and models, 100% passing.
- **GitHub Actions CI pipeline** (`build-and-test.yml`) — builds on `windows-latest` and runs all 79 tests on every push and pull request to master.
- **NZ localisation** — GST 15%, 16 NZ regions (Province dropdown), 4-digit NZ postal code validation, "Price NZD" column header, NZD currency formatting throughout.
- **Resizable window** — `FormBorderStyle.Sizable` + `MaximizeBox = true` with enforced `MinimumSize`.
- **Descriptive control names** — all 60+ generic control names (`textBox1`, `checkBox1`, `button1`, …) renamed to meaningful identifiers (`txtFirstName`, `cbPepperoni`, `btnConfirmOrder`, …) via a PowerShell word-boundary regex script.
- **`System.Web.Extensions` assembly reference** for built-in JSON serialisation.

### Changed

- `TargetFrameworkVersion` upgraded from `v4.5` to `v4.8`.
- Replaced hard-coded magic numbers/strings with `AppConfig` constants across all handlers.
- Replaced `Controls.Find` loops (broken after rename) with direct typed field references.

### Removed

- Sourcecodester attribution from UI footer.
- All Canadian localisation (provinces, HST 13%, CAD currency, Canadian postal code format).

---

## [1.3.0] — 2026-03-23

### Added

- **US-30** — Pizza Quantity Selection: a `NumericUpDown` control (min 1, max 20, default 1) is now displayed in the Pizza Size groupbox. The pizza ListView row shows the chosen quantity and the price reflects `unit price × qty`. Qty resets to 1 on "Order Again".
- **US-31** — Order Multiple Different Pizzas: a new "Add Pizza to Cart" button stages the current pizza (size + crust + qty + toppings), resets those fields to defaults, and allows the customer to configure another pizza before clicking "Confirm Order". All staged pizzas are flushed into the order list when "Confirm Order" is clicked. Staged list clears on "Order Again".

---

## [1.2.0] — 2026-03-23

### Added

- **US-34** — Promo Code Discount: the "Promo Card" payment method now accepts real discount codes.
  - Supported codes: `PIZZA10` (10% off), `PIZZA20` (20% off), `FREESHIP` (100% off / free order)
  - The card number label dynamically changes to "*Promo Code:" when Promo Card is selected
  - Invalid codes are rejected with a clear error message
- **US-35** — Export Order Receipt: after clicking Submit Order a Yes/No dialog offers to save a timestamped `.txt` receipt.
  - Receipt includes store header, customer delivery info, itemised order list, subtotal / HST / total, and payment method
  - Default filename: `Receipt_yyyyMMdd_HHmmss.txt`

### Improved

- **US-32** — Postal Code Format Validation: the Pay button now enforces the Canadian A1A 1A1 format (regex `^[A-Z]\d[A-Z]\d[A-Z]\d$`) before processing any payment. Spaces and lowercase input are normalised automatically.
- **US-33** — Contact Number Validation: when a contact number is entered it is validated for digits only and a length of 7–15 digits (regex `^\+?\d{7,15}$`). The field remains optional.

---

## [1.1.0] — 2026-03-23

### Fixed

#### Critical
- **FIX-01** — App crashed with `FormatException` when a drink checkbox was ticked but its quantity field was left empty. All 7 drink quantity fields are now validated with `int.TryParse` before any processing begins. A specific message is shown per drink (e.g. "Please enter a valid quantity (greater than 0) for Coke.").
- **FIX-02** — App crashed when the amount paid field contained multiple decimal points (e.g. `1.2.3`). The `KeyPress` handler now blocks a second decimal point if one already exists in the field.

#### High
- **FIX-03** — Clicking "Next" multiple times duplicated every order item in the list, causing incorrect totals. The order list is now cleared at the start of each "Next" click before items are re-added.
- **FIX-04** — Once the Confirm button was enabled by a valid payment, re-entering an insufficient amount showed the warning but left the Confirm button enabled. `button8.Enabled = false` is now set in the insufficient payment branch.
- **FIX-05** — The card number field stayed disabled after switching away from "Cash" to any card-based payment method, making card payments impossible. The `comboBox2_SelectedIndexChanged` handler now re-enables the field for all non-Cash selections.
- **FIX-06** — On "Order Again", the pizza size and crust radio buttons were not reset to their defaults (Small, Normal Crust), leaving the next order with no pre-selection. Defaults are now restored, and the Confirm button and card field are also reset.
- **FIX-07** — The Confirm button stayed enabled when the user went back from the Order Review tab or cleared the order, allowing a stale payment to confirm a modified order. Both the Back button and the Clear button now disable `button8`.

#### Medium
- **FIX-08** — A drink quantity of `0` was accepted and added a $0.00 line item with no actual drink. Quantity is now validated to be greater than 0.
- **FIX-09** — An empty order (no items in list) could proceed through to the checkout tab showing $0.00 totals. A guard now blocks navigation if the order list is empty after building.
- **FIX-10** — The `listView1_SelectedIndexChanged` handler was an empty auto-generated stub. Clarified with a comment that it is intentionally empty (Designer-wired, no action required).
- **FIX-11** — Drink costs were stored as raw double strings (e.g. `"2.9"`) while all other items used 2 decimal places. All drink costs now use `.ToString("F2")` for consistency.

#### Low
- **FIX-12** — The Exit button on Tab 1 closed the application immediately with no confirmation, risking accidental data loss mid-order. A Yes/No confirmation dialog is now shown before closing.
- **FIX-13** — The card number field was enabled by default on application load even before any payment method was selected. It is now disabled on load and only enabled when a card-based payment method is chosen.

---

## [1.0.0] — 2026-03-23

### Added
- Initial release of Pizza Express Ordering System
- 3-tab Windows Forms UI: Order Selection, Order Review, Checkout
- Pizza customisation: 4 sizes, 3 crust types
- 14 topping options at $0.75 each
- 7 drink options with individual quantity fields
- 4 side items and 3 free dipping sauces
- Itemised order summary with ListView (Item, Qty, Price columns)
- 13% HST calculation with subtotal and total due display
- Customer delivery information form with Canadian province dropdown
- Payment methods: Cash, Credit Card, Debit Card, Promo Card
- Change calculation for cash payments
- Required field validation before payment processing
- Order confirmation dialog with option to place another order or exit
- Full state reset on "Order Again" flow
- Numeric-only input validation on all quantity fields
- `.gitignore` for Visual Studio / .NET build artifacts
- `README.md` with setup instructions and project structure
- `USER_STORIES.md` with 29 detailed user stories, acceptance criteria, priorities and story points
