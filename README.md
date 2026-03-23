# Pizza Ordering System

A Windows Forms desktop application built in C# (.NET Framework 4.5) that simulates a pizza restaurant point-of-sale (POS) ordering system for **Pizza Express**.

## Features

- **Pizza Customization**
  - 4 sizes: Small ($4.00), Medium ($7.00), Large ($10.00), Extra Large ($13.00)
  - 3 crust types: Normal, Cheesy, Sausage
  - 14 toppings available at $0.75 each (Pepperoni, Extra Cheese, Mushroom, Ham, Bacon, Ground Beef, Jalapeno, Pineapple, Dried Shrimps, Anchovies, Sun Dried Tomatoes, Spinach, Roasted Garlic, Shredded Chicken)

- **Drinks**
  - Coke, Diet Coke, Iced Tea, Ginger Ale, Sprite, Root Beer — $1.45/can
  - Bottled Water — $1.25
  - Custom quantity per drink

- **Extras**
  - Chicken Wings, Poutine, Onion Rings, Cheesy Garlic Bread — $3.00 each
  - Free dips: Garlic, BBQ, Sour Cream

- **Order Summary**
  - Itemized order list with quantities and prices
  - 13% HST (tax) calculation
  - Subtotal and total due display

- **Checkout**
  - Customer delivery information (name, address, city, province, postal code, phone)
  - Payment methods: Cash, Credit Card, Debit Card, Promo Card
  - Change calculation for cash payments
  - New order flow without restarting the application

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

## License

This project is open source and available under the [MIT License](LICENSE).
