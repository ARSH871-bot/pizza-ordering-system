# Pizza Ordering System

![Build and Test](https://github.com/ARSH871-bot/pizza-ordering-system/actions/workflows/build-and-test.yml/badge.svg)
![Version](https://img.shields.io/badge/version-2.0.0-brightgreen)
![Tests](https://img.shields.io/badge/tests-79%20passing-success)
![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![Framework](https://img.shields.io/badge/.NET-4.8-purple)

A Windows Forms desktop application built in C# (.NET Framework 4.5) that simulates a pizza restaurant point-of-sale (POS) ordering system for **Pizza Express**.

## Features

- **Pizza Customization**
  - 4 sizes: Small ($4.00), Medium ($7.00), Large ($10.00), Extra Large ($13.00)
  - 3 crust types: Normal, Cheesy, Sausage
  - 14 toppings at $0.75 each (Pepperoni, Extra Cheese, Mushroom, Ham, Bacon, Ground Beef, Jalapeno, Pineapple, Dried Shrimps, Anchovies, Sun Dried Tomatoes, Spinach, Roasted Garlic, Shredded Chicken)
  - **Quantity selector (1–20)** per pizza — price scales automatically
  - **"Add Pizza to Cart"** button — stage a pizza, reset fields, and configure a completely different pizza in the same order

- **Drinks**
  - Coke, Diet Coke, Iced Tea, Ginger Ale, Sprite, Root Beer — $1.45/can
  - Bottled Water — $1.25
  - Custom quantity per drink

- **Extras**
  - Chicken Wings, Poutine, Onion Rings, Cheesy Garlic Bread — $3.00 each
  - Free dips: Garlic, BBQ, Sour Cream

- **Order Summary**
  - Itemised order list with Item, Quantity, and Price CAD columns
  - 13% HST calculation with subtotal and total due

- **Checkout & Validation**
  - Customer delivery info: First Name, Last Name, Address, City, Province (dropdown), Postal Code, Contact No, Email
  - **Canadian postal code validation** — enforces A1A 1A1 format
  - **Contact number validation** — digits only, 7–15 characters (optional field)
  - Payment methods: Cash, Credit Card, Debit Card, **Promo Card**
  - **Promo codes**: `PIZZA10` (10% off), `PIZZA20` (20% off), `FREESHIP` (free order)
  - Change calculation for cash payments
  - **Export receipt to `.txt`** — timestamped file with full order details

- **App Flow**
  - Confirm button gated on valid payment entry
  - "Order Again" resets the entire form without restarting the application
  - Exit confirmation dialog to prevent accidental data loss

## Screenshots

> UI is a 3-tab Windows Form: Order Selection → Order Review → Checkout

## Tech Stack

| Technology | Detail |
|---|---|
| Language | C# |
| Framework | .NET Framework 4.5 |
| UI | Windows Forms (WinForms) |
| IDE | Visual Studio 2015/2017 |

## Getting Started

### Prerequisites

- Windows OS
- Visual Studio 2015 or later (or any version supporting .NET Framework 4.5)
- .NET Framework 4.5

### Running the App

1. Clone the repository:
   ```bash
   git clone https://github.com/ARSH871-bot/pizza-ordering-system.git
   ```
2. Open `WindowsFormsApplication3.sln` in Visual Studio.
3. Build the solution (`Ctrl+Shift+B`).
4. Run the application (`F5`).

## Project Structure

```
PizzaOrderingSystemC#/
├── WindowsFormsApplication3/
│   ├── Form1.cs               # Main application logic
│   ├── Form1.Designer.cs      # Auto-generated UI layout
│   ├── Program.cs             # Application entry point
│   ├── App.config             # Application configuration
│   ├── images/                # App images/logos
│   └── Properties/            # Assembly info and resources
└── WindowsFormsApplication3.sln
```

## Changelog

All 31 user stories across 3 feature releases are implemented. See [CHANGELOG.md](CHANGELOG.md) for full details.

| Version | Highlights |
|---|---|
| **v1.3.0** | Pizza quantity selector (1–20); multi-pizza ordering via "Add Pizza to Cart" |
| **v1.2.0** | Postal code & phone validation; promo codes; receipt export to `.txt` |
| **v1.1.0** | 13 bug fixes (2 critical, 5 high, 4 medium, 2 low) |
| **v1.0.0** | Initial release |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for branching strategy, commit conventions, and how to submit a pull request.

## License

This project is open source and available under the [MIT License](LICENSE).
