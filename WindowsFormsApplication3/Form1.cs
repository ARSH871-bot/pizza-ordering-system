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
            radioButton1.Checked = true;
            radioButton5.Checked = true;
            numericUpDown1.Value = 1;

            textBox8.Enabled  = false;
            textBox9.Enabled  = false;
            textBox10.Enabled = false;
            textBox19.Enabled = false;
            textBox21.Enabled = false;
            textBox18.Enabled = false;

            foreach (string region in AppConfig.NZRegions)
                comboBox1.Items.Add(region);

            foreach (string method in AppConfig.PaymentMethods)
                comboBox2.Items.Add(method);

            button8.Enabled = false;
        }

        // =====================================================================
        // Tab 1 — Order Selection
        // =====================================================================

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Validate drink quantities
            if (!ValidateDrinkQuantities()) return;

            listView1.Items.Clear();

            // Flush staged pizzas
            foreach (var staged in _stagedPizzas)
                listView1.Items.Add(staged);

            // Build current pizza + toppings
            foreach (var item in BuildCurrentPizzaItems())
                listView1.Items.Add(item);

            // Add drinks
            AddDrinkIfChecked(checkBox15, textBox1,  "Coke - Can",        AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(checkBox16, textBox2,  "Diet Coke - Can",   AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(checkBox17, textBox3,  "Iced Tea - Can",    AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(checkBox18, textBox4,  "Ginger Ale - Can",  AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(checkBox19, textBox5,  "Sprite - Can",      AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(checkBox20, textBox6,  "Root Beer - Can",   AppConfig.DrinkCanPrice);
            AddDrinkIfChecked(checkBox21, textBox7,  "Bottled Water",     AppConfig.WaterPrice);

            // Add sides / dips
            AddSideIfChecked(checkBox22, "Chicken Wings",      AppConfig.SidePrice);
            AddSideIfChecked(checkBox23, "Poutine",            AppConfig.SidePrice);
            AddSideIfChecked(checkBox24, "Onion Rings",        AppConfig.SidePrice);
            AddSideIfChecked(checkBox25, "Cheesy Garlic Bread",AppConfig.SidePrice);
            AddSideIfChecked(checkBox26, "Garlic Dip",         0m);
            AddSideIfChecked(checkBox27, "BBQ Dip",            0m);
            AddSideIfChecked(checkBox28, "Sour Cream Dip",     0m);

            // Validate the assembled order
            var items = new List<OrderItem>();
            foreach (ListViewItem lvi in listView1.Items)
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
                listView1.Items.Clear();
                MessageBox.Show(orderResult.ErrorMessage);
                return;
            }

            // Compute totals using AppConfig tax rate
            decimal subtotal = 0m;
            foreach (ListViewItem lvi in listView1.Items)
            {
                decimal p;
                decimal.TryParse(lvi.SubItems[2].Text, out p);
                subtotal += p;
            }

            decimal tax      = Math.Round(subtotal * AppConfig.TaxRate, 2);
            decimal totalDue = subtotal + tax;

            textBox8.Text  = subtotal.ToString("C2");
            textBox9.Text  = tax.ToString("C2");
            textBox10.Text = totalDue.ToString("C2");

            tabControl1.SelectTab("tabPage2");
        }

        // ── "Add Pizza to Cart" ───────────────────────────────────────────────
        private void button9_Click(object sender, EventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
        {
            button8.Enabled = false;
            tabControl1.SelectTab("tabPage1");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage3");
            textBox19.Text = textBox10.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            textBox8.Text  = "";
            textBox9.Text  = "";
            textBox10.Text = "";
            button8.Enabled = false;
        }

        // =====================================================================
        // Tab 3 — Checkout
        // =====================================================================

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage2");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var customer = BuildCustomer();

            // Validate customer fields
            var customerResult = _validator.ValidateCustomer(customer);
            if (!customerResult.IsValid) { MessageBox.Show(customerResult.ErrorMessage); return; }

            if (comboBox2.Text == "Promo Card")
            {
                string code = textBox18.Text.Trim();
                char[] dollar = { '$' };
                decimal originalTotal = Convert.ToDecimal(textBox10.Text.TrimStart(dollar));

                var promoResult = _promoEngine.Apply(code, originalTotal);
                if (!promoResult.Success) { MessageBox.Show(promoResult.Message); return; }

                textBox19.Text = promoResult.DiscountedTotal.ToString("C2");
                textBox20.Text = promoResult.DiscountedTotal.ToString("F2");
                textBox21.Text = "$0.00";
                MessageBox.Show(promoResult.Message, "Promo Applied");
                button8.Enabled = true;
                return;
            }

            // Standard payment flow
            if (string.IsNullOrWhiteSpace(comboBox2.Text) || string.IsNullOrWhiteSpace(textBox20.Text))
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

            char[] dollarSign = { '$' };
            decimal totalDue   = Convert.ToDecimal(textBox19.Text.TrimStart(dollarSign));
            decimal amountPaid;
            if (!decimal.TryParse(textBox20.Text, out amountPaid))
            {
                MessageBox.Show("Please enter a valid payment amount.");
                return;
            }

            var payResult = _validator.ValidatePayment(comboBox2.Text, amountPaid, totalDue);
            if (!payResult.IsValid) { MessageBox.Show(payResult.ErrorMessage); button8.Enabled = false; return; }

            textBox21.Text  = (amountPaid - totalDue).ToString("C2");
            button8.Enabled = true;
        }

        private void button8_Click(object sender, EventArgs e)
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

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isPromo = comboBox2.Text == "Promo Card";
            bool isCash  = comboBox2.Text == "Cash";

            textBox18.Enabled = !isCash;
            label15.Text      = isPromo ? "*Promo Code:" : "*Card No:";
        }

        // =====================================================================
        // Drink quantity key-press guards (digits only)
        // =====================================================================

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);
        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)  => AllowDigitsOnly(e);

        private void textBox20_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.' && textBox20.Text.Contains(".")) { e.Handled = true; return; }
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '.') e.Handled = true;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }

        private void button5_Click(object sender, EventArgs e)
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
                (checkBox15, textBox1, "Coke"),
                (checkBox16, textBox2, "Diet Coke"),
                (checkBox17, textBox3, "Iced Tea"),
                (checkBox18, textBox4, "Ginger Ale"),
                (checkBox19, textBox5, "Sprite"),
                (checkBox20, textBox6, "Root Beer"),
                (checkBox21, textBox7, "Bottled Water"),
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
            int qty   = (int)numericUpDown1.Value;

            PizzaSize? size  = null;
            CrustType? crust = null;

            if (radioButton1.Checked) size  = PizzaSize.Small;
            else if (radioButton2.Checked) size = PizzaSize.Medium;
            else if (radioButton3.Checked) size = PizzaSize.Large;
            else if (radioButton4.Checked) size = PizzaSize.ExtraLarge;

            if (radioButton5.Checked) crust  = CrustType.Normal;
            else if (radioButton6.Checked) crust = CrustType.Cheesy;
            else if (radioButton7.Checked) crust = CrustType.Sausage;

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
                (checkBox1,  "  Pepperoni Toppings"),
                (checkBox2,  "  Extra Cheese Toppings"),
                (checkBox3,  "  Mushroom Toppings"),
                (checkBox4,  "  Ham Toppings"),
                (checkBox5,  "  Bacon Toppings"),
                (checkBox6,  "  Ground Beef Toppings"),
                (checkBox7,  "  Jalapeno Toppings"),
                (checkBox8,  "  Pineapple Toppings"),
                (checkBox9,  "  Dried Shrimps Toppings"),
                (checkBox10, "  Anchovies Toppings"),
                (checkBox11, "  Sun Dried Tomatoes Toppings"),
                (checkBox12, "  Spinach Toppings"),
                (checkBox13, "  Roasted Garlic Toppings"),
                (checkBox14, "  Shredded Chicken Toppings"),
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
            listView1.Items.Add(item);
        }

        private void AddSideIfChecked(CheckBox cb, string name, decimal price)
        {
            if (!cb.Checked) return;
            var item = new ListViewItem(name);
            item.SubItems.Add("");
            item.SubItems.Add(price.ToString("F2"));
            listView1.Items.Add(item);
        }

        private Customer BuildCustomer() => new Customer
        {
            FirstName  = textBox11.Text.Trim(),
            LastName   = textBox12.Text.Trim(),
            Address    = textBox13.Text.Trim(),
            City       = textBox14.Text.Trim(),
            Region     = comboBox1.Text,
            PostalCode = textBox15.Text.Trim(),
            ContactNo  = textBox16.Text.Trim(),
            Email      = textBox17.Text.Trim(),
        };

        private Order BuildOrderForReceipt()
        {
            var order = new Order
            {
                Customer      = BuildCustomer(),
                PaymentMethod = comboBox2.Text,
            };

            char[] dollar = { '$' };
            decimal.TryParse(textBox20.Text, out decimal paid);
            order.AmountPaid = paid;

            foreach (ListViewItem lvi in listView1.Items)
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
            radioButton1.Checked = true;
            radioButton5.Checked = true;
            numericUpDown1.Value = 1;

            for (int i = 1; i <= 14; i++)
            {
                var found = this.Controls.Find("checkBox" + i, true);
                if (found.Length > 0) ((CheckBox)found[0]).Checked = false;
            }
        }

        private void ResetFullForm()
        {
            // Toppings, drinks, sides
            for (int i = 1; i <= 28; i++)
            {
                var found = this.Controls.Find("checkBox" + i, true);
                if (found.Length > 0) ((CheckBox)found[0]).Checked = false;
            }

            // Drink qty boxes
            for (int i = 1; i <= 7; i++)
            {
                var found = this.Controls.Find("textBox" + i, true);
                if (found.Length > 0) ((TextBox)found[0]).Text = "";
            }

            // Order summary
            listView1.Items.Clear();
            textBox8.Text  = "";
            textBox9.Text  = "";
            textBox10.Text = "";

            // Customer / payment
            textBox11.Text = ""; textBox12.Text = ""; textBox13.Text = "";
            textBox14.Text = ""; textBox15.Text = ""; textBox16.Text = "";
            textBox17.Text = ""; textBox18.Text = ""; textBox19.Text = "";
            textBox20.Text = ""; textBox21.Text = "";
            comboBox1.Text = ""; comboBox2.Text = "";

            // Pizza defaults
            radioButton1.Checked  = true;
            radioButton5.Checked  = true;
            numericUpDown1.Value  = 1;
            _stagedPizzas.Clear();

            // Payment state
            button8.Enabled   = false;
            textBox18.Enabled = false;
            label15.Text      = "*Card No:";
        }
    }
}
