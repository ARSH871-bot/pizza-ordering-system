# Changelog

All notable changes to this project are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased] — v1.2.0 Planned

### Planned Features

- **US-30** — Pizza Quantity Selection: allow ordering more than 1 of the same pizza per order
- **US-31** — Order Multiple Different Pizzas: add multiple size/crust configurations to a single order
- **US-32** — Postal Code Format Validation: enforce Canadian A1A 1A1 format on Postal Code field
- **US-33** — Contact Number Validation: digits only, 7–15 digit length when Contact No is provided
- **US-34** — Promo Code Discount: activate the "Promo Card" payment method with real discount codes
- **US-35** — Export Order Receipt: save a timestamped .txt receipt file after payment is confirmed

See the [v1.2.0 milestone](https://github.com/ARSH871-bot/pizza-ordering-system/milestone/1) for full details.

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
