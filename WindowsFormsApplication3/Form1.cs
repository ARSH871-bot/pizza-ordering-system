using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApplication3.Config;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        // ── Services (injected via constructor / field initialiser) ───────────
        private readonly PromoEngine    _promoEngine    = new PromoEngine();
        private readonly OrderValidator _validator      = new OrderValidator();
        private readonly ReceiptWriter  _receiptWriter  = new ReceiptWriter();

        // ── State ─────────────────────────────────────────────────────────────
        private readonly List<ListViewItem> _stagedPizzas = new List<ListViewItem>();

        public Form1()
        {
            InitializeComponent();
        }

        // =====================================================================
        // Form load
        // =====================================================================

        private void Form1_Load(object sender, EventArgs e)
        {
            rbSizeSmall.Checked = true;
            rbCrustNormal.Checked = true;
            nudPizzaQty.Value = 1;

            txtSubtotal.Enabled  = false;
            txtTax.Enabled  = false;
            txtTotalDue.Enabled = false;
            txtAmountDue.Enabled = false;
            txtChange.Enabled = false;
            txtCardOrPromo.Enabled = false;

            foreach (string region in AppConfig.NZRegions)
                cboRegion.Items.Add(region);

            foreach (string method in AppConfig.PaymentMethods)
                cboPaymentMethod.Items.Add(method);

            btnSubmitOrder.Enabled = false;
        }

        // =====================================================================
        // Tab 1 — Order Selection
        // =====================================================================

        private void btnConfirmOrder_Click(object sender, EventArgs e)
        {
            // Validate drink quantities
            if (!ValidateDrinkQuantities()) return;

            lvOrder.Items.Clear();

            // Flush staged pizzas
            foreach (var staged in _stagedPizzas)
                lvOrder.Items.Add(staged);

            // Build current pizza + toppings
            foreach (var item in BuildCurrentPizzaItems())
                lvOrder.Items.Add(item);

            // Add drinks
            AddDrinkIfChecked(cbCoke, txtQtyCoke,  "Coke - Can",        AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(cbDietCoke, txtQtyDietCoke,  "Diet Coke - Can",   AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(cbIcedTea, txtQtyIcedTea,  "Iced Tea - Can",    AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(cbGingerAle, txtQtyGingerAle,  "Ginger Ale - Can",  AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(cbSprite, txtQtySprite,  "Sprite - Can",      AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(cbRootBeer, txtQtyRootBeer,  "Root Beer - Can",   AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(cbWater, txtQtyWater,  "Bottled Water",     AppConfig.WaterPrice);

            // Add sides / dips
            AddSideIfChecked(cbChickenWings, "Chicken Wings",      AppConfig.SidePrice);
            AddSideIfChecked(cbPoutine, "Poutine",            AppConfig.SidePrice);
            AddSideIfChecked(cbOnionRings, "Onion Rings",        AppConfig.SidePrice);
            AddSideIfChecked(cbCheesyGarlicBread, "Cheesy Garlic Bread",AppConfig.SidePrice);
            AddSideIfChecked(cbGarlicDip, "Garlic Dip",         0m);
            AddSideIfChecked(cbBBQDip, "BBQ Dip",            0m);
            AddSideIfChecked(cbSourCreamDip, "Sour Cream Dip",     0m);

            // Validate the assembled order
            var items = new List<OrderItem>();
            foreach (ListViewItem lvi in lvOrder.Items)
            {
                decimal price;
                decimal.TryParse(lvi.SubItems[2].Text, out price);
                int qty;
                int.TryParse(lvi.SubItems[1].Text, out qty);
                items.Add(new OrderItem(lvi.Text, qty, price));
            }

            var orderResult = _validator.ValidateOrder(items);
            if (!orderResult.IsValid)
            {
                lvOrder.Items.Clear();
                MessageBox.Show(orderResult.ErrorMessage);
                return;
            }

            // Compute totals using AppConfig tax rate
            decimal subtotal = 0m;
            foreach (ListViewItem lvi in lvOrder.Items)
            {
                decimal p;
                decimal.TryParse(lvi.SubItems[2].Text, out p);
                subtotal += p;
            }

            decimal tax      = Math.Round(subtotal * AppConfig.TaxRate, 2);
            decimal totalDue = subtotal + tax;

            txtSubtotal.Text  = subtotal.ToString("C2");
            txtTax.Text  = tax.ToString("C2");
            txtTotalDue.Text = totalDue.ToString("C2");

            tabControl1.SelectTab("tabPage2");
        }

        // ── "Add Pizza to Cart" ───────────────────────────────────────────────
        private void btnAddPizzaToCart_Click(object sender, EventArgs e)
        {
            var pizzaItems = BuildCurrentPizzaItems();
            if (pizzaItems.Count == 0)
            {
                MessageBox.Show("Please select a pizza size and crust before adding to cart.");
                return;
            }
            _stagedPizzas.AddRange(pizzaItems);
            MessageBox.Show(
                "Pizza added to cart! Configure another pizza or click Confirm Order when ready.",
                "Pizza Added");
            ResetPizzaAndToppings();
        }

        // =====================================================================
        // Tab 2 — Order Review
        // =====================================================================

        private void btnOrderAgain_Click(object sender, EventArgs e)
        {
            btnSubmitOrder.Enabled = false;
            tabControl1.SelectTab("tabPage1");
        }

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage3");
            txtAmountDue.Text = txtTotalDue.Text;
        }

        private void btnClearOrder_Click(object sender, EventArgs e)
        {
            lvOrder.Items.Clear();
            txtSubtotal.Text  = "";
            txtTax.Text  = "";
            txtTotalDue.Text = "";
            btnSubmitOrder.Enabled = false;
        }

        // =====================================================================
        // Tab 3 — Checkout
        // =====================================================================

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage2");
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            var customer = BuildCustomer();

            // Validate customer fields
            var customerResult = _validator.ValidateCustomer(customer);
            if (!customerResult.IsValid) { MessageBox.Show(customerResult.ErrorMessage); return; }

            if (cboPaymentMethod.Text == "Promo Card")
            {
                string code = txtCardOrPromo.Text.Trim();
                char[] dollar = { '$' };
                decimal originalTotal = Convert.ToDecimal(txtTotalDue.Text.TrimStart(dollar));

                var promoResult = _promoEngine.Apply(code, originalTotal);
                if (!promoResult.Success) { MessageBox.Show(promoResult.Message); return; }

                txtAmountDue.Text = promoResult.DiscountedTotal.ToString("C2");
                txtAmountPaid.Text = promoResult.DiscountedTotal.ToString("F2");
                txtChange.Text = "$0.00";
                MessageBox.Show(promoResult.Message, "Promo Applied");
                btnSubmitOrder.Enabled = true;
                return;
            }

            // Standard payment flow
            if (string.IsNullOrWhiteSpace(cboPaymentMethod.Text) || string.IsNullOrWhiteSpace(txtAmountPaid.Text))
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

            char[] dollarSign = { '$' };
            decimal totalDue   = Convert.ToDecimal(txtAmountDue.Text.TrimStart(dollarSign));
            decimal amountPaid;
            if (!decimal.TryParse(txtAmountPaid.Text, out amountPaid))
            {
                MessageBox.Show("Please enter a valid payment amount.");
                return;
            }

            var payResult = _validator.ValidatePayment(cboPaymentMethod.Text, amountPaid, totalDue);
            if (!payResult.IsValid) { MessageBox.Show(payResult.ErrorMessage); btnSubmitOrder.Enabled = false; return; }

            txtChange.Text  = (amountPaid - totalDue).ToString("C2");
            btnSubmitOrder.Enabled = true;
        }

        private void btnSubmitOrder_Click(object sender, EventArgs e)
        {
            // Build the Order object for the receipt service
            var order = BuildOrderForReceipt();

            // Offer to save receipt
            if (MessageBox.Show("Would you like to save a receipt of this order?",
                    "Save Receipt", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter   = "Text Files (*.txt)|*.txt";
                    sfd.FileName = $"Receipt_{order.OrderDate:yyyyMMdd_HHmmss}.txt";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        _receiptWriter.SaveToFile(order, sfd.FileName);
                        MessageBox.Show("Receipt saved successfully.", "Receipt Saved");
                    }
                }
            }

            // Order again or exit
            if (MessageBox.Show(
                    $"Thanks for ordering at Pizza Express!\n" +
                    $"Your order will be delivered in approx. {AppConfig.DeliveryMinutes} minutes.\n\n" +
                    "Would you like to place another order?",
                    "Order Complete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ResetFullForm();
                tabControl1.SelectTab("tabPage1");
            }
            else
            {
                this.Close();
            }
        }

        private void cboPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isPromo = cboPaymentMethod.Text == "Promo Card";
            bool isCash  = cboPaymentMethod.Text == "Cash";

            txtCardOrPromo.Enabled = !isCash;
            lblCardOrPromo.Text      = isPromo ? "*Promo Code:" : "*Card No:";
        }

        // =====================================================================
        // Drink quantity key-press guards (digits only)
        // =====================================================================

        private void txtQtyCoke_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void txtQtyDietCoke_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void txtQtyIcedTea_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void txtQtyGingerAle_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void txtQtySprite_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void txtQtyRootBeer_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void txtQtyWater_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);

        private void txtAmountPaid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.' && txtAmountPaid.Text.Contains(".")) { e.Handled = true; return; }
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '.') e.Handled = true;
        }

        private void lvOrder_SelectedIndexChanged(object sender, EventArgs e) { }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
                this.Close();
        }

        // =====================================================================
        // Private helpers
        // =====================================================================

        private static void AllowDigitsOnly(KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true;
        }

        private bool ValidateDrinkQuantities()
        {
            var checks = new (CheckBox cb, TextBox tb, string name)[]
            {
                (cbCoke, txtQtyCoke, "Coke"),
                (cbDietCoke, txtQtyDietCoke, "Diet Coke"),
                (cbIcedTea, txtQtyIcedTea, "Iced Tea"),
                (cbGingerAle, txtQtyGingerAle, "Ginger Ale"),
                (cbSprite, txtQtySprite, "Sprite"),
                (cbRootBeer, txtQtyRootBeer, "Root Beer"),
                (cbWater, txtQtyWater, "Bottled Water"),
            };

            foreach (var (cb, tb, name) in checks)
            {
                int qty;
                if (cb.Checked && (!int.TryParse(tb.Text, out qty) || qty <= 0))
                {
                    MessageBox.Show($"Please enter a valid quantity (greater than 0) for {name}.");
                    return false;
                }
            }
            return true;
        }

        private List<ListViewItem> BuildCurrentPizzaItems()
        {
            var items = new List<ListViewItem>();
            int qty   = (int)nudPizzaQty.Value;

            PizzaSize? size  = null;
            CrustType? crust = null;

            if (rbSizeSmall.Checked) size  = PizzaSize.Small;
            else if (rbSizeMedium.Checked) size = PizzaSize.Medium;
            else if (rbSizeLarge.Checked) size = PizzaSize.Large;
            else if (rbSizeExtraLarge.Checked) size = PizzaSize.ExtraLarge;

            if (rbCrustNormal.Checked) crust  = CrustType.Normal;
            else if (rbCrustCheesy.Checked) crust = CrustType.Cheesy;
            else if (rbCrustSausage.Checked) crust = CrustType.Sausage;

            if (size.HasValue && crust.HasValue)
            {
                decimal unitPrice  = AppConfig.PizzaPrices[size.Value];
                decimal totalPrice = unitPrice * qty;
                string  sizeName   = size.Value == PizzaSize.ExtraLarge ? "Extra Large" : size.Value.ToString();
                string  crustName  = crust.Value.ToString();

                var pizzaItem = new ListViewItem($"{crustName} Crust {sizeName} Pizza");
                pizzaItem.SubItems.Add(qty.ToString());
                pizzaItem.SubItems.Add(totalPrice.ToString("F2"));
                items.Add(pizzaItem);
            }

            // Toppings
            var toppingMap = new (CheckBox cb, string name)[]
            {
                (cbPepperoni,  "  Pepperoni Toppings"),
                (cbExtraCheese,  "  Extra Cheese Toppings"),
                (cbMushroom,  "  Mushroom Toppings"),
                (cbHam,  "  Ham Toppings"),
                (cbBacon,  "  Bacon Toppings"),
                (cbGroundBeef,  "  Ground Beef Toppings"),
                (cbJalapeno,  "  Jalapeno Toppings"),
                (cbPineapple,  "  Pineapple Toppings"),
                (cbDriedShrimps,  "  Dried Shrimps Toppings"),
                (cbAnchovies, "  Anchovies Toppings"),
                (cbSunDriedTomatoes, "  Sun Dried Tomatoes Toppings"),
                (cbSpinach, "  Spinach Toppings"),
                (cbRoastedGarlic, "  Roasted Garlic Toppings"),
                (cbShreddedChicken, "  Shredded Chicken Toppings"),
            };

            foreach (var (cb, name) in toppingMap)
            {
                if (!cb.Checked) continue;
                var t = new ListViewItem(name);
                t.SubItems.Add("");
                t.SubItems.Add(AppConfig.ToppingPrice.ToString("F2"));
                items.Add(t);
            }

            return items;
        }

        private void AddDrinkIfChecked(CheckBox cb, TextBox qtyBox, string name, decimal unitPrice)
        {
            if (!cb.Checked) { qtyBox.Text = ""; return; }
            int qty = int.Parse(qtyBox.Text);
            var item = new ListViewItem(name);
            item.SubItems.Add(qty.ToString());
            item.SubItems.Add((qty * unitPrice).ToString("F2"));
            lvOrder.Items.Add(item);
        }

        private void AddSideIfChecked(CheckBox cb, string name, decimal price)
        {
            if (!cb.Checked) return;
            var item = new ListViewItem(name);
            item.SubItems.Add("");
            item.SubItems.Add(price.ToString("F2"));
            lvOrder.Items.Add(item);
        }

        private Customer BuildCustomer() => new Customer
        {
            FirstName  = txtFirstName.Text.Trim(),
            LastName   = txtLastName.Text.Trim(),
            Address    = txtAddress.Text.Trim(),
            City       = txtCity.Text.Trim(),
            Region     = cboRegion.Text,
            PostalCode = txtPostalCode.Text.Trim(),
            ContactNo  = txtContactNo.Text.Trim(),
            Email      = txtEmail.Text.Trim(),
        };

        private Order BuildOrderForReceipt()
        {
            var order = new Order
            {
                Customer      = BuildCustomer(),
                PaymentMethod = cboPaymentMethod.Text,
            };

            char[] dollar = { '$' };
            decimal.TryParse(txtAmountPaid.Text, out decimal paid);
            order.AmountPaid = paid;

            foreach (ListViewItem lvi in lvOrder.Items)
            {
                decimal price;
                decimal.TryParse(lvi.SubItems[2].Text, out price);
                int qty;
                int.TryParse(lvi.SubItems[1].Text, out qty);
                order.Items.Add(new OrderItem(lvi.Text, qty, price));
            }

            return order;
        }

        private void ResetPizzaAndToppings()
        {
            rbSizeSmall.Checked   = true;
            rbCrustNormal.Checked = true;
            nudPizzaQty.Value     = 1;

            // Toppings
            cbPepperoni.Checked = false;       cbExtraCheese.Checked = false;
            cbMushroom.Checked  = false;       cbHam.Checked         = false;
            cbBacon.Checked     = false;       cbGroundBeef.Checked  = false;
            cbJalapeno.Checked  = false;       cbPineapple.Checked   = false;
            cbDriedShrimps.Checked = false;    cbAnchovies.Checked   = false;
            cbSunDriedTomatoes.Checked = false; cbSpinach.Checked    = false;
            cbRoastedGarlic.Checked = false;   cbShreddedChicken.Checked = false;
        }

        private void ResetFullForm()
        {
            // Toppings
            cbPepperoni.Checked = false;       cbExtraCheese.Checked = false;
            cbMushroom.Checked  = false;       cbHam.Checked         = false;
            cbBacon.Checked     = false;       cbGroundBeef.Checked  = false;
            cbJalapeno.Checked  = false;       cbPineapple.Checked   = false;
            cbDriedShrimps.Checked = false;    cbAnchovies.Checked   = false;
            cbSunDriedTomatoes.Checked = false; cbSpinach.Checked    = false;
            cbRoastedGarlic.Checked = false;   cbShreddedChicken.Checked = false;

            // Drinks
            cbCoke.Checked = false;    cbDietCoke.Checked = false;
            cbIcedTea.Checked = false; cbGingerAle.Checked = false;
            cbSprite.Checked = false;  cbRootBeer.Checked = false;
            cbWater.Checked = false;

            // Sides / dips
            cbChickenWings.Checked = false;    cbPoutine.Checked = false;
            cbOnionRings.Checked = false;      cbCheesyGarlicBread.Checked = false;
            cbGarlicDip.Checked = false;       cbBBQDip.Checked = false;
            cbSourCreamDip.Checked = false;

            // Drink qty boxes
            txtQtyCoke.Text = ""; txtQtyDietCoke.Text = ""; txtQtyIcedTea.Text = "";
            txtQtyGingerAle.Text = ""; txtQtySprite.Text = "";
            txtQtyRootBeer.Text = ""; txtQtyWater.Text = "";

            // Order summary
            lvOrder.Items.Clear();
            txtSubtotal.Text  = "";
            txtTax.Text  = "";
            txtTotalDue.Text = "";

            // Customer / payment
            txtFirstName.Text = ""; txtLastName.Text = ""; txtAddress.Text = "";
            txtCity.Text = ""; txtPostalCode.Text = ""; txtContactNo.Text = "";
            txtEmail.Text = ""; txtCardOrPromo.Text = ""; txtAmountDue.Text = "";
            txtAmountPaid.Text = ""; txtChange.Text = "";
            cboRegion.Text = ""; cboPaymentMethod.Text = "";

            // Pizza defaults
            rbSizeSmall.Checked  = true;
            rbCrustNormal.Checked  = true;
            nudPizzaQty.Value  = 1;
            _stagedPizzas.Clear();

            // Payment state
            btnSubmitOrder.Enabled   = false;
            txtCardOrPromo.Enabled = false;
            lblCardOrPromo.Text      = "*Card No:";
        }
    }
}
