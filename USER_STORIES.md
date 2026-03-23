# User Stories — Pizza Ordering System

**Project:** Pizza Express Ordering System
**Platform:** C# Windows Forms (.NET Framework 4.5)
**Last Updated:** 2026-03-23

---

## Table of Contents

1. [Pizza Selection](#pizza-selection)
2. [Drinks](#drinks)
3. [Extras & Sides](#extras--sides)
4. [Order Review](#order-review)
5. [Checkout & Payment](#checkout--payment)
6. [Order Confirmation](#order-confirmation)
7. [System Behaviour](#system-behaviour)

---

## Legend

| Field | Description |
|---|---|
| **ID** | Unique story identifier |
| **Priority** | High / Medium / Low |
| **Status** | Implemented / Partial / Not Implemented |
| **Story Points** | Relative effort estimate (1 = trivial, 8 = complex) |

---

## Pizza Selection

---

### US-01 — Select Pizza Size

| Field | Detail |
|---|---|
| **ID** | US-01 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to select a pizza size, so that I can order a pizza that fits my appetite and budget.

**Acceptance Criteria**
- [x]Four size options are available: Small, Medium, Large, Extra Large
- [x]Only one size can be selected at a time (mutually exclusive radio buttons)
- [x]Prices are fixed per size: Small $4.00, Medium $7.00, Large $10.00, Extra Large $13.00
- [x]A size is always pre-selected by default (Small) so the form is never in an invalid state
- [x]The selected size is reflected correctly in the order summary list

**Notes**
- Default selection is Small on application load
- Size and crust combine to form one order line item (e.g., "Normal Crust Small Pizza")

---

### US-02 — Choose Crust Type

| Field | Detail |
|---|---|
| **ID** | US-02 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to choose a crust type, so that I can customize my pizza to my preference.

**Acceptance Criteria**
- [x]Three crust options are available: Normal, Cheesy, Sausage
- [x]Only one crust type can be selected at a time (mutually exclusive radio buttons)
- [x]Normal crust is pre-selected by default
- [x]Crust type does not affect the price — only the label in the order summary changes
- [x]The selected crust name is included in the order line item description

**Notes**
- Crust and size are combined into a single order list entry (e.g., "Cheesy Crust Large Pizza")

---

### US-03 — Add Toppings

| Field | Detail |
|---|---|
| **ID** | US-03 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 3 |

**User Story**
As a customer, I want to add toppings to my pizza, so that I can personalize my order with my favorite ingredients.

**Acceptance Criteria**
- [x]14 topping options are available: Pepperoni, Extra Cheese, Mushroom, Ham, Bacon, Ground Beef, Jalapeno, Pineapple, Dried Shrimps, Anchovies, Sun Dried Tomatoes, Spinach, Roasted Garlic, Shredded Chicken
- [x]Multiple toppings can be selected simultaneously (independent checkboxes)
- [x]Each selected topping appears as a separate line item in the order summary
- [x]Unselected toppings do not appear in the order summary
- [x]Each topping is priced at $0.75

**Notes**
- Toppings are listed with a leading indent in the order summary to visually distinguish them from the main pizza item

---

### US-04 — View Topping Prices

| Field | Detail |
|---|---|
| **ID** | US-04 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to see the price of each topping, so that I know how much extra I will be charged before confirming my order.

**Acceptance Criteria**
- [x]Every topping shows a price of $0.75 in the order summary
- [x]The price is visible in the Price column of the order list
- [x]All toppings are uniformly priced (no exceptions)

---

### US-30 — Pizza Quantity Selection

| Field | Detail |
|---|---|
| **ID** | US-30 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 3 |

**User Story**
As a customer, I want to specify how many of the same pizza I want, so that I can order multiple identical pizzas without having to go back and re-configure the same pizza again.

**Acceptance Criteria**
- [x]A numeric quantity input (min 1, max 20, default 1) is displayed in the Pizza Size group
- [x]The quantity input accepts only positive integers (1–20)
- [x]The default quantity is 1 on load and after "Order Again"
- [x]When qty > 1, the ListView shows the correct quantity (e.g., "3") in the Quantity column
- [x]The Price column for the pizza row reflects the full cost (unit price × quantity) rounded to 2 decimal places
- [x]A qty of 1 behaves identically to the previous single-pizza flow

**Notes**
- Implemented as a `NumericUpDown` control inside the Pizza Size groupbox

---

### US-31 — Order Multiple Different Pizzas

| Field | Detail |
|---|---|
| **ID** | US-31 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 5 |

**User Story**
As a customer, I want to add multiple different pizzas to a single order, so that I can order a variety of sizes and crust types for a group without placing separate orders.

**Acceptance Criteria**
- [x]An "Add Pizza to Cart" button is available on the Order Selection tab
- [x]Clicking "Add Pizza to Cart" stages the current pizza (size + crust + qty + selected toppings) to an internal cart list and shows a confirmation message
- [x]After staging, size/crust radio buttons reset to defaults (Small, Normal) and all topping checkboxes are cleared, allowing a new pizza to be configured
- [x]The quantity field resets to 1 after each "Add Pizza to Cart"
- [x]"Confirm Order" processes all staged pizzas plus the current selection (if any) along with drinks and sides
- [x]If no pizza has been staged and no pizza size/crust is selected, "Confirm Order" shows an appropriate error
- [x]The staged pizza list is cleared on "Order Again" and on application load

**Notes**
- Staged pizzas are held in a `List<ListViewItem>` field (`_stagedPizzas`) that is flushed into `listView1` at the start of `button1_Click`

---

## Drinks

---

### US-05 — Select Canned Drinks

| Field | Detail |
|---|---|
| **ID** | US-05 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to select from a variety of canned drinks, so that I can include a beverage with my meal.

**Acceptance Criteria**
- [x]6 canned drink options are available: Coke, Diet Coke, Iced Tea, Ginger Ale, Sprite, Root Beer
- [x]Each drink is activated via a checkbox
- [x]Each drink can is priced at $1.45
- [x]The drink only appears in the order summary if its checkbox is checked
- [x]The total cost = quantity × $1.45 and is shown in the order summary

---

### US-06 — Order Bottled Water

| Field | Detail |
|---|---|
| **ID** | US-06 |
| **Priority** | Low |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to order Bottled Water as an alternative to canned drinks, so that I have a non-carbonated beverage option.

**Acceptance Criteria**
- [x]Bottled Water is available as a separate drink option
- [x]It is priced at $1.25 (different from canned drinks at $1.45)
- [x]A quantity field is available for Bottled Water
- [x]Total cost = quantity × $1.25

---

### US-07 — Specify Drink Quantity

| Field | Detail |
|---|---|
| **ID** | US-07 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to specify the quantity for each drink I order, so that I can order multiple cans of the same drink.

**Acceptance Criteria**
- [x]Each drink has a dedicated quantity text field
- [x]The quantity field is associated 1:1 with its drink checkbox
- [x]If the drink checkbox is unchecked, the quantity field is cleared automatically
- [x]The total price in the order summary reflects the entered quantity
- [x]If no quantity is entered for a selected drink, the system should handle the conversion gracefully

**Notes**
- If a drink is unchecked, its textbox is cleared in the `else` branch of the button click handler

---

### US-08 — Numeric-Only Drink Quantity Input

| Field | Detail |
|---|---|
| **ID** | US-08 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want the drink quantity fields to only accept numeric input, so that I cannot accidentally enter invalid characters.

**Acceptance Criteria**
- [x]Only digit characters (0–9) are accepted in drink quantity fields
- [x]Backspace key is allowed for correction
- [x]Any non-numeric keypress is silently blocked (not entered)
- [x]This validation applies to all 7 drink quantity fields

---

## Extras & Sides

---

### US-09 — Add Side Items

| Field | Detail |
|---|---|
| **ID** | US-09 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to add side items to my order, so that I can make it a complete meal.

**Acceptance Criteria**
- [x]4 side items are available: Chicken Wings, Poutine, Onion Rings, Cheesy Garlic Bread
- [x]Each is selectable via a checkbox
- [x]Each side item is priced at $3.00
- [x]Selected sides appear as individual line items in the order summary
- [x]Unselected sides do not appear in the order summary

---

### US-10 — Add Free Dipping Sauces

| Field | Detail |
|---|---|
| **ID** | US-10 |
| **Priority** | Low |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to add free dipping sauces to my order, so that I can enhance my meal at no extra cost.

**Acceptance Criteria**
- [x]3 dip options are available: Garlic Dip, BBQ Dip, Sour Cream Dip
- [x]Each is selectable via a checkbox
- [x]All dips are priced at $0.00
- [x]Selected dips appear in the order summary with a $0.00 price
- [x]Dips do not affect the subtotal or total

---

## Order Review

---

### US-11 — View Itemized Order Summary

| Field | Detail |
|---|---|
| **ID** | US-11 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 3 |

**User Story**
As a customer, I want to see an itemized list of everything I've ordered, so that I can verify my order before paying.

**Acceptance Criteria**
- [x]Order summary is displayed in a 3-column list: Item Name, Quantity, Price
- [x]Every selected item (pizza, toppings, drinks, sides, dips) appears as a separate row
- [x]Item names accurately reflect the user's selections (e.g., size + crust combination)
- [x]Prices shown per row match the defined pricing for each item
- [x]The list is populated when the user clicks "Next" / proceeds from Tab 1

---

### US-12 — View Price Breakdown with Tax

| Field | Detail |
|---|---|
| **ID** | US-12 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to see the subtotal, HST, and total due, so that I understand the full cost of my order.

**Acceptance Criteria**
- [x]Subtotal is the sum of all item prices in the order list
- [x]HST is calculated as 13% of the subtotal
- [x]Total due = Subtotal + HST
- [x]All three values are displayed in currency format (e.g., $12.75)
- [x]The fields are read-only and cannot be edited by the user

---

### US-13 — Go Back to Order Page

| Field | Detail |
|---|---|
| **ID** | US-13 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to go back to the order page from the order summary, so that I can make changes before proceeding to checkout.

**Acceptance Criteria**
- [x]An "Order Again" button is available on the Order Review tab (actual button label is "Order Again", not "Back")
- [x]Clicking it navigates back to Tab 1 (Order Selection)
- [x]Existing selections on Tab 1 remain intact after navigating back
- [x]The Confirm Order button is disabled when navigating back (payment re-validation required)

---

### US-14 — Clear Order and Start Over

| Field | Detail |
|---|---|
| **ID** | US-14 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to clear my entire order and start fresh, so that I can correct mistakes without closing the application.

**Acceptance Criteria**
- [x]A "Clear" button is available on the Order Review tab
- [x]Clicking it removes all items from the order list
- [x]Subtotal, HST, and Total Due fields are cleared
- [x]The user remains on the Order Review tab after clearing

**Notes**
- This only clears the order list and totals — Tab 1 selections (checkboxes, radio buttons) are not reset by this button. Full reset only happens after order confirmation.

---

## Checkout & Payment

---

### US-15 — Enter Delivery Information

| Field | Detail |
|---|---|
| **ID** | US-15 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to enter my delivery details, so that my order can be delivered to the correct location.

**Acceptance Criteria**
- [x]Fields available: First Name*, Last Name*, Address*, City, Province (dropdown), Postal Code*, Contact No, Email (* = required)
- [x]Required fields enforced: First Name, Last Name, Address, Postal Code
- [x]Optional fields: City, Province, Contact No, Email
- [x]If any required field is empty, a validation message is shown: "Please fill in required fields"
- [x]All fields accept free-text input; Province uses a dropdown

---

### US-16 — Select Province from Dropdown

| Field | Detail |
|---|---|
| **ID** | US-16 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to select my province from a dropdown list, so that I can provide my location without typing errors.

**Acceptance Criteria**
- [x]A dropdown (comboBox) lists Canadian provinces: Alberta, British Columbia, Manitoba, New Brunswick, Newfoundland and Labrador, Ontario, Prince Edward Island, Quebec, Saskatchewan
- [x]Only one province can be selected at a time
- [x]Province selection is not part of required-field validation (optional field)

---

### US-17 — Choose Payment Method

| Field | Detail |
|---|---|
| **ID** | US-17 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to choose my payment method, so that I can pay using whichever method I prefer.

**Acceptance Criteria**
- [x]Four payment methods are available: Cash, Credit Card, Debit Card, Promo Card
- [x]Payment method selection is required before processing payment
- [x]If no payment method is selected, the validation message is shown
- [x]Selecting "Cash" disables the card number field (textBox18) automatically

---

### US-18 — Enter Cash Payment Amount

| Field | Detail |
|---|---|
| **ID** | US-18 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to enter the amount I am paying in cash, so that the system can calculate my change automatically.

**Acceptance Criteria**
- [x]A text field accepts the cash amount entered by the customer
- [x]The field accepts decimal values (digits and decimal point only)
- [x]Non-numeric and non-decimal characters are blocked on keypress
- [x]The amount entered is compared against the total due when "Pay" is clicked

---

### US-19 — View Change Due

| Field | Detail |
|---|---|
| **ID** | US-19 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to see the change owed to me after paying, so that I know exactly how much to expect back.

**Acceptance Criteria**
- [x]Change = Amount Paid − Total Due
- [x]Change is displayed in currency format (e.g., $3.25)
- [x]The change field is read-only and cannot be edited
- [x]Change is only calculated and displayed after the "Pay" button is clicked
- [x]Change field is blank on initial load

---

### US-20 — Insufficient Payment Warning

| Field | Detail |
|---|---|
| **ID** | US-20 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to be warned if I haven't paid enough, so that I know I need to provide a higher amount before the order is confirmed.

**Acceptance Criteria**
- [x]If change < 0 (amount paid is less than total due), a message box is shown: "Please pay your balance"
- [x]The "Confirm Order" button remains disabled when payment is insufficient
- [x]The customer can re-enter a higher amount and click "Pay" again

---

### US-21 — Required Field Validation Before Payment

| Field | Detail |
|---|---|
| **ID** | US-21 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As a customer, I want to be prevented from confirming the order if required fields are missing, so that incomplete orders are not accidentally submitted.

**Acceptance Criteria**
- [x]Required fields: First Name, Last Name, Address, Postal Code, Amount Paid, Payment Method
- [x]If any required field is empty when "Pay" is clicked, a message box displays: "Please fill in required fields"
- [x]Payment calculation and confirmation are blocked until all required fields are populated
- [x]Validation is triggered on "Pay" button click

---

## Order Confirmation

---

### US-22 — Receive Order Confirmation Message

| Field | Detail |
|---|---|
| **ID** | US-22 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to receive a confirmation message when my order is placed, so that I know it has been successfully submitted and when to expect my delivery.

**Acceptance Criteria**
- [x]A dialog box is shown after clicking "Confirm Order"
- [x]The message reads: "Thanks for ordering at Pizza Express. Your ordered items will be ready and delivered in 30 minutes. Do you want to order some more?"
- [x]Dialog presents Yes / No options
- [x]Confirmation dialog only appears when payment has been validated (change >= 0)

---

### US-23 — Place Another Order Without Restarting

| Field | Detail |
|---|---|
| **ID** | US-23 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 3 |

**User Story**
As a customer, I want to place another order immediately after confirming, so that I can reorder without restarting the application.

**Acceptance Criteria**
- [x]Selecting "Yes" in the confirmation dialog resets the entire application state
- [x]All checkboxes (toppings, drinks, sides, dips) are unchecked
- [x]All drink quantity fields are cleared
- [x]Order list is cleared
- [x]All totals (subtotal, HST, total due) are cleared
- [x]All customer/payment fields are cleared
- [x]Province and payment method dropdowns are reset
- [x]The app navigates back to Tab 1 (Order Selection)
- [x]Default pizza size (Small) and crust (Normal) are re-selected

**Notes**
- Reset also restores default radio button selections (Small size, Normal crust), disables the Confirm button, and disables the card number field — fully resolved by FIX-06.

---

### US-24 — Exit Application After Order

| Field | Detail |
|---|---|
| **ID** | US-24 |
| **Priority** | Low |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As a customer, I want to exit the application after my order is confirmed if I choose not to order again, so that I can close the system cleanly.

**Acceptance Criteria**
- [x]Selecting "No" in the confirmation dialog closes the application
- [x]No unsaved state warnings are shown
- [x]The application terminates gracefully

---

## System Behaviour

---

### US-25 — Default Pizza Configuration on Startup

| Field | Detail |
|---|---|
| **ID** | US-25 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As the system, I want to pre-select Small size and Normal crust by default on startup, so that the form is always in a valid state when the customer begins ordering.

**Acceptance Criteria**
- [x]On application load, radioButton1 (Small) is checked
- [x]On application load, radioButton5 (Normal Crust) is checked
- [x]No other size or crust radio buttons are pre-selected
- [x]This ensures clicking "Next" without changing anything produces a valid order item

---

### US-26 — Read-Only Calculated Fields

| Field | Detail |
|---|---|
| **ID** | US-26 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As the system, I want calculated fields to be non-editable, so that totals and change values cannot be manually altered by the user.

**Acceptance Criteria**
- [x]Subtotal field (textBox8) is disabled/read-only
- [x]HST field (textBox9) is disabled/read-only
- [x]Total Due field (textBox10) is disabled/read-only
- [x]Change field (textBox21) is disabled/read-only
- [x]These fields are set as disabled on Form Load

---

### US-27 — Full State Reset on New Order

| Field | Detail |
|---|---|
| **ID** | US-27 |
| **Priority** | Medium |
| **Status** | Implemented |
| **Story Points** | 2 |

**User Story**
As the system, I want to reset all UI fields when the customer chooses to order again, so that the next order starts with a completely clean state.

**Acceptance Criteria**
- [x]All 28 checkboxes are unchecked
- [x]All 7 drink quantity textboxes are cleared
- [x]Order list (ListView) is cleared
- [x]Subtotal, HST, Total Due fields are cleared
- [x]All customer info fields (textBox11–textBox18) are cleared
- [x]Amount Paid and Change fields are cleared
- [x]Province and payment method dropdowns are reset to empty
- [x]Navigation returns to Tab 1

---

### US-28 — Contextual Card Number Field

| Field | Detail |
|---|---|
| **ID** | US-28 |
| **Priority** | Low |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As the system, I want to disable the card number field when "Cash" is selected as the payment method, so that irrelevant fields are hidden contextually and the form stays clean.

**Acceptance Criteria**
- [x]When payment method dropdown changes to "Cash", textBox18 (card number) is disabled
- [x]For all other payment methods, the card number field remains enabled
- [x]This behaviour is triggered on the comboBox SelectedIndexChanged event

---

### US-29 — Confirm Button Gated on Payment Validation

| Field | Detail |
|---|---|
| **ID** | US-29 |
| **Priority** | High |
| **Status** | Implemented |
| **Story Points** | 1 |

**User Story**
As the system, I want to keep the "Confirm Order" button disabled until payment is successfully validated, so that orders cannot be finalized before payment is verified.

**Acceptance Criteria**
- [x]"Confirm Order" button (button8) is disabled on application load
- [x]It remains disabled until "Pay" is clicked and change >= 0
- [x]If the customer re-enters a lower amount causing change < 0, the button is disabled again — resolved by FIX-04
- [x]Button becomes enabled only after successful payment validation

---

## Summary Table

| ID | Title | Priority | Status | Points |
|---|---|---|---|---|
| US-01 | Select Pizza Size | High | Implemented | 2 |
| US-02 | Choose Crust Type | High | Implemented | 2 |
| US-03 | Add Toppings | High | Implemented | 3 |
| US-04 | View Topping Prices | Medium | Implemented | 1 |
| US-05 | Select Canned Drinks | Medium | Implemented | 2 |
| US-06 | Order Bottled Water | Low | Implemented | 1 |
| US-07 | Specify Drink Quantity | Medium | Implemented | 2 |
| US-08 | Numeric-Only Drink Input | Medium | Implemented | 1 |
| US-09 | Add Side Items | Medium | Implemented | 2 |
| US-10 | Add Free Dipping Sauces | Low | Implemented | 1 |
| US-11 | View Itemized Order Summary | High | Implemented | 3 |
| US-12 | View Price Breakdown with Tax | High | Implemented | 2 |
| US-13 | Go Back to Order Page | Medium | Implemented | 1 |
| US-14 | Clear Order and Start Over | Medium | Implemented | 2 |
| US-15 | Enter Delivery Information | High | Implemented | 2 |
| US-16 | Select Province from Dropdown | Medium | Implemented | 1 |
| US-17 | Choose Payment Method | High | Implemented | 1 |
| US-18 | Enter Cash Payment Amount | High | Implemented | 2 |
| US-19 | View Change Due | High | Implemented | 2 |
| US-20 | Insufficient Payment Warning | High | Implemented | 1 |
| US-21 | Required Field Validation | High | Implemented | 2 |
| US-22 | Order Confirmation Message | High | Implemented | 1 |
| US-23 | Place Another Order | Medium | Implemented | 3 |
| US-24 | Exit After Order | Low | Implemented | 1 |
| US-25 | Default Pizza Config on Startup | Medium | Implemented | 1 |
| US-26 | Read-Only Calculated Fields | Medium | Implemented | 1 |
| US-27 | Full State Reset on New Order | Medium | Implemented | 2 |
| US-28 | Contextual Card Number Field | Low | Implemented | 1 |
| US-29 | Confirm Button Gated on Payment | High | Implemented | 1 |
| US-30 | Pizza Quantity Selection | High | Implemented | 3 |
| US-31 | Order Multiple Different Pizzas | High | Implemented | 5 |

**Total Story Points: 57**
**Total User Stories: 29**
**All stories: Implemented**
