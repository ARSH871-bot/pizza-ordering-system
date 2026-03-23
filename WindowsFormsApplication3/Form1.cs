using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        // US-31: staged pizzas from "Add Pizza to Cart" clicks
        private List<ListViewItem> _stagedPizzas = new List<ListViewItem>();

        public Form1()
        {
            InitializeComponent();
        }

        // US-30/31: build pizza+topping ListViewItems for the current UI selection
        private List<ListViewItem> BuildCurrentPizzaItems()
        {
            var items = new List<ListViewItem>();
            int qty = (int)numericUpDown1.Value;

            string pizzaName = null;
            double pizzaUnitPrice = 0;

            if (radioButton1.Checked)      { pizzaName = "Small";       pizzaUnitPrice = 4.00; }
            else if (radioButton2.Checked) { pizzaName = "Medium";      pizzaUnitPrice = 7.00; }
            else if (radioButton3.Checked) { pizzaName = "Large";       pizzaUnitPrice = 10.00; }
            else if (radioButton4.Checked) { pizzaName = "Extra Large"; pizzaUnitPrice = 13.00; }

            string crustName = null;
            if (radioButton5.Checked)      crustName = "Normal Crust";
            else if (radioButton6.Checked) crustName = "Cheesy Crust";
            else if (radioButton7.Checked) crustName = "Sausage Crust";

            if (pizzaName != null && crustName != null)
            {
                double pizzaTotalPrice = pizzaUnitPrice * qty;
                ListViewItem pizzaItem = new ListViewItem(crustName + " " + pizzaName + " Pizza");
                pizzaItem.SubItems.Add(qty.ToString());
                pizzaItem.SubItems.Add(pizzaTotalPrice.ToString("F2"));
                items.Add(pizzaItem);
            }

            // Toppings (one per topping regardless of qty — same as original behaviour)
            if (checkBox1.Checked)  { var i = new ListViewItem("  Pepperoni Toppings");        i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox2.Checked)  { var i = new ListViewItem("  Extra Cheese Toppings");     i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox3.Checked)  { var i = new ListViewItem("  Mushroom Toppings");         i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox4.Checked)  { var i = new ListViewItem("  Ham Toppings");              i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox5.Checked)  { var i = new ListViewItem("  Bacon Toppings");            i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox6.Checked)  { var i = new ListViewItem("  Ground Beef Toppings");      i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox7.Checked)  { var i = new ListViewItem("  Jalapeno Toppings");         i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox8.Checked)  { var i = new ListViewItem("  Pineapple Toppings");        i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox9.Checked)  { var i = new ListViewItem("  Dried Shrimps Toppings");    i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox10.Checked) { var i = new ListViewItem("  Anchovies Toppings");        i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox11.Checked) { var i = new ListViewItem("  Sun Dried Tomatoes Toppings"); i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox12.Checked) { var i = new ListViewItem("  Spinach Toppings");          i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox13.Checked) { var i = new ListViewItem("  Roasted Garlic Toppings");   i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }
            if (checkBox14.Checked) { var i = new ListViewItem("  Shredded Chicken Toppings"); i.SubItems.Add(""); i.SubItems.Add("0.75"); items.Add(i); }

            return items;
        }

        // US-31: reset pizza size, crust, toppings, and qty to defaults
        private void ResetPizzaAndToppings()
        {
            radioButton1.Checked = true;
            radioButton5.Checked = true;
            numericUpDown1.Value = 1;
            for (int i = 1; i <= 14; i++)
            {
                var cb = this.Controls.Find("checkBox" + i, true);
                if (cb.Length > 0) ((CheckBox)cb[0]).Checked = false;
            }
        }

        // US-31: "Add Pizza to Cart" button handler
        private void button9_Click(object sender, EventArgs e)
        {
            var pizzaItems = BuildCurrentPizzaItems();
            if (pizzaItems.Count == 0)
            {
                MessageBox.Show("Please select a pizza size and crust before adding to cart.");
                return;
            }
            _stagedPizzas.AddRange(pizzaItems);
            MessageBox.Show("Pizza added to cart! You can now configure another pizza or click Confirm Order when ready.", "Pizza Added");
            ResetPizzaAndToppings();
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            // FIX-01 & FIX-08: Validate all checked drink quantities before processing anything
            int tempQty;
            if (checkBox15.Checked && (!int.TryParse(textBox1.Text, out tempQty) || tempQty <= 0))
            { MessageBox.Show("Please enter a valid quantity (greater than 0) for Coke."); return; }
            if (checkBox16.Checked && (!int.TryParse(textBox2.Text, out tempQty) || tempQty <= 0))
            { MessageBox.Show("Please enter a valid quantity (greater than 0) for Diet Coke."); return; }
            if (checkBox17.Checked && (!int.TryParse(textBox3.Text, out tempQty) || tempQty <= 0))
            { MessageBox.Show("Please enter a valid quantity (greater than 0) for Iced Tea."); return; }
            if (checkBox18.Checked && (!int.TryParse(textBox4.Text, out tempQty) || tempQty <= 0))
            { MessageBox.Show("Please enter a valid quantity (greater than 0) for Ginger Ale."); return; }
            if (checkBox19.Checked && (!int.TryParse(textBox5.Text, out tempQty) || tempQty <= 0))
            { MessageBox.Show("Please enter a valid quantity (greater than 0) for Sprite."); return; }
            if (checkBox20.Checked && (!int.TryParse(textBox6.Text, out tempQty) || tempQty <= 0))
            { MessageBox.Show("Please enter a valid quantity (greater than 0) for Root Beer."); return; }
            if (checkBox21.Checked && (!int.TryParse(textBox7.Text, out tempQty) || tempQty <= 0))
            { MessageBox.Show("Please enter a valid quantity (greater than 0) for Bottled Water."); return; }

            // FIX-03: Clear list before adding to prevent duplicate items on repeated clicks
            listView1.Items.Clear();

            // US-31: flush all staged pizzas from previous "Add Pizza to Cart" clicks
            foreach (var staged in _stagedPizzas)
                listView1.Items.Add(staged);

            // US-30/31: build the current pizza selection (may be empty if user only staged via button9)
            var currentPizzaItems = BuildCurrentPizzaItems();
            foreach (var item in currentPizzaItems)
                listView1.Items.Add(item);

            //Drink Selection

            if (checkBox15.Checked == true)
            {
                ListViewItem item = new ListViewItem("Coke - Can");
                int qty = int.Parse(textBox1.Text);
                double cost = qty * 1.45;
                item.SubItems.Add(textBox1.Text);
                item.SubItems.Add(cost.ToString("F2"));
                listView1.Items.Add(item);
            }
            else
            {
                textBox1.Text = "";
            }

            if (checkBox16.Checked == true)
            {
                ListViewItem item = new ListViewItem("Diet Coke - Can");
                int qty = int.Parse(textBox2.Text);
                double cost = qty * 1.45;
                item.SubItems.Add(textBox2.Text);
                item.SubItems.Add(cost.ToString("F2"));
                listView1.Items.Add(item);
            }
            else
            {
                textBox2.Text = "";
            }

            if (checkBox17.Checked == true)
            {
                ListViewItem item = new ListViewItem("Iced Tea - Can");
                int qty = int.Parse(textBox3.Text);
                double cost = qty * 1.45;
                item.SubItems.Add(textBox3.Text);
                item.SubItems.Add(cost.ToString("F2"));
                listView1.Items.Add(item);
            }
            else
            {
                textBox3.Text = "";
            }

            if (checkBox18.Checked == true)
            {
                ListViewItem item = new ListViewItem("Ginger Ale - Can");
                int qty = int.Parse(textBox4.Text);
                double cost = qty * 1.45;
                item.SubItems.Add(textBox4.Text);
                item.SubItems.Add(cost.ToString("F2"));
                listView1.Items.Add(item);
            }
            else
            {
                textBox4.Text = "";
            }

            if (checkBox19.Checked == true)
            {
                ListViewItem item = new ListViewItem("Sprite - Can");
                int qty = int.Parse(textBox5.Text);
                double cost = qty * 1.45;
                item.SubItems.Add(textBox5.Text);
                item.SubItems.Add(cost.ToString("F2"));
                listView1.Items.Add(item);
            }
            else
            {
                textBox5.Text = "";
            }

            if (checkBox20.Checked == true)
            {
                ListViewItem item = new ListViewItem("Root Beer - Can");
                int qty = int.Parse(textBox6.Text);
                double cost = qty * 1.45;
                item.SubItems.Add(textBox6.Text);
                item.SubItems.Add(cost.ToString("F2"));
                listView1.Items.Add(item);
            }
            else
            {
                textBox6.Text = "";
            }

            if (checkBox21.Checked == true)
            {
                ListViewItem item = new ListViewItem("Bottled Water");
                int qty = int.Parse(textBox7.Text);
                double cost = qty * 1.25;
                item.SubItems.Add(textBox7.Text);
                item.SubItems.Add(cost.ToString("F2"));
                listView1.Items.Add(item);
            }
            else
            {
                textBox7.Text = "";
            }

            //Other Items Selection

            if (checkBox22.Checked == true)
            {
                ListViewItem item = new ListViewItem("Chicken Wings");
                item.SubItems.Add("");
                item.SubItems.Add("3.00");
                listView1.Items.Add(item);
            }

            if (checkBox23.Checked == true)
            {
                ListViewItem item = new ListViewItem("Poutine");
                item.SubItems.Add("");
                item.SubItems.Add("3.00");
                listView1.Items.Add(item);
            }

            if (checkBox24.Checked == true)
            {
                ListViewItem item = new ListViewItem("Onion Rings");
                item.SubItems.Add("");
                item.SubItems.Add("3.00");
                listView1.Items.Add(item);
            }

            if (checkBox25.Checked == true)
            {
                ListViewItem item = new ListViewItem("Cheesy Garlic Bread");
                item.SubItems.Add("");
                item.SubItems.Add("3.00");
                listView1.Items.Add(item);
            }

            if (checkBox26.Checked == true)
            {
                ListViewItem item = new ListViewItem("Garlic Dip");
                item.SubItems.Add("");
                item.SubItems.Add("0.00");
                listView1.Items.Add(item);
            }

            if (checkBox27.Checked == true)
            {
                ListViewItem item = new ListViewItem("BBQ Dip");
                item.SubItems.Add("");
                item.SubItems.Add("0.00");
                listView1.Items.Add(item);
            }

            if (checkBox28.Checked == true)
            {
                ListViewItem item = new ListViewItem("Sour Cream Dip");
                item.SubItems.Add("");
                item.SubItems.Add("0.00");
                listView1.Items.Add(item);
            }

            // FIX-09 / US-31: block if no pizza was configured at all (neither staged nor current)
            bool hasPizza = listView1.Items.Cast<ListViewItem>()
                .Any(i => i.Text.EndsWith("Pizza"));
            if (!hasPizza)
            {
                MessageBox.Show("Please configure at least one pizza before proceeding.");
                return;
            }

            // FIX-09: Block empty order from proceeding (no items at all)
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("Please select at least one item before proceeding.");
                return;
            }

            double total = 0;
            double hst = 0;
            double totaldue = 0;

            foreach (ListViewItem item in listView1.Items)
            {
                total += Convert.ToDouble(item.SubItems[2].Text);
            }

            hst = total * 0.13;
            totaldue = hst + total;

            string hstDisplay = hst.ToString("c2");
            string totalDisplay = totaldue.ToString("c2");
            string amount = total.ToString("c2");

            textBox8.Text = amount;
            textBox9.Text = hstDisplay;
            textBox10.Text = totalDisplay;

            tabControl1.SelectTab("tabPage2");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // FIX-07: Invalidate confirmed payment when user goes back to modify order
            button8.Enabled = false;
            tabControl1.SelectTab("tabPage1");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage3");
            textBox19.Text = textBox10.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton5.Checked = true;
            numericUpDown1.Value = 1;
            textBox8.Enabled = false;
            textBox9.Enabled = false;
            textBox10.Enabled = false;
            textBox19.Enabled = false;
            textBox21.Enabled = false;
            textBox18.Enabled = false; // FIX-13: disabled until a card payment method is selected

            comboBox1.Items.Add("Alberta");
            comboBox1.Items.Add("British Columbia");
            comboBox1.Items.Add("Manitoba");
            comboBox1.Items.Add("New Brunswick");
            comboBox1.Items.Add("Newfoundland and Labrador");
            comboBox1.Items.Add("Ontario");
            comboBox1.Items.Add("Prince Edward Island");
            comboBox1.Items.Add("Quebec");
            comboBox1.Items.Add("Saskatchewan");

            comboBox2.Items.Add("Cash");
            comboBox2.Items.Add("Credit Card");
            comboBox2.Items.Add("Debit Card");
            comboBox2.Items.Add("Promo Card");

            button8.Enabled = false;
        }


        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // intentionally empty — event wired by Designer, no action needed
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            textBox8.Text = "";
            textBox9.Text = "";
            textBox10.Text = "";
            button8.Enabled = false; // FIX-07: reset confirm button when order is cleared
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // FIX-12: confirm before closing mid-order
            DialogResult confirm = MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo);
            if (confirm == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void textBox20_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            // FIX-02: block a second decimal point to prevent Convert.ToDouble crash
            if (q == 46 && textBox20.Text.Contains("."))
            {
                e.Handled = true;
                return;
            }
            if (!Char.IsDigit(q) && q != 8 && q != 46)
            {
                e.Handled = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage2");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // US-34: Promo Card has its own validation and discount flow
            if (comboBox2.Text == "Promo Card")
            {
                if (textBox11.Text == "" || textBox12.Text == "" || textBox13.Text == "" || textBox15.Text == "")
                {
                    MessageBox.Show("Please fill in required fields");
                    return;
                }

                // US-32: Validate Canadian postal code
                string postalCode = textBox15.Text.Trim().Replace(" ", "").ToUpper();
                if (!Regex.IsMatch(postalCode, @"^[A-Z]\d[A-Z]\d[A-Z]\d$"))
                {
                    MessageBox.Show("Please enter a valid Canadian postal code (e.g. A1A 1A1)");
                    return;
                }

                // US-33: Validate contact number if provided
                if (!string.IsNullOrWhiteSpace(textBox16.Text))
                {
                    string contactNo = textBox16.Text.Trim();
                    if (!Regex.IsMatch(contactNo, @"^\+?\d{7,15}$"))
                    {
                        MessageBox.Show("Please enter a valid contact number (digits only, 7-15 digits)");
                        return;
                    }
                }

                // US-34: Validate promo code
                string code = textBox18.Text.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show("Please enter a promo code.");
                    return;
                }

                double discount = 0;
                string discountLabel = "";
                switch (code)
                {
                    case "PIZZA10": discount = 0.10; discountLabel = "10% off"; break;
                    case "PIZZA20": discount = 0.20; discountLabel = "20% off"; break;
                    case "FREESHIP": discount = 1.00; discountLabel = "100% off (Free Order)"; break;
                    default:
                        MessageBox.Show("Invalid promo code. Please try again.");
                        return;
                }

                char[] dollars = { '$' };
                double originalTotal = Convert.ToDouble(textBox10.Text.TrimStart(dollars));
                double discountedTotal = originalTotal * (1 - discount);

                textBox19.Text = discountedTotal.ToString("c2");
                textBox20.Text = discountedTotal.ToString("F2");
                textBox21.Text = "$0.00";

                MessageBox.Show("Promo code applied! " + discountLabel + "\nNew total: " + discountedTotal.ToString("c2"), "Promo Applied");
                button8.Enabled = true;
                return;
            }

            // Standard payment flow (Cash, Credit Card, Debit Card)
            if (textBox11.Text == "" || textBox12.Text == "" || textBox13.Text == "" || textBox15.Text == "" || textBox20.Text == "" || comboBox2.Text == "")
            {
                MessageBox.Show("Please fill in required fields");
                return;
            }

            // US-32: Validate Canadian postal code
            string postal = textBox15.Text.Trim().Replace(" ", "").ToUpper();
            if (!Regex.IsMatch(postal, @"^[A-Z]\d[A-Z]\d[A-Z]\d$"))
            {
                MessageBox.Show("Please enter a valid Canadian postal code (e.g. A1A 1A1)");
                return;
            }

            // US-33: Validate contact number if provided
            if (!string.IsNullOrWhiteSpace(textBox16.Text))
            {
                string contactNo = textBox16.Text.Trim();
                if (!Regex.IsMatch(contactNo, @"^\+?\d{7,15}$"))
                {
                    MessageBox.Show("Please enter a valid contact number (digits only, 7-15 digits)");
                    return;
                }
            }

            string money = textBox19.Text;
            char[] dollarSign = { '$' };
            string paymoney = money.TrimStart(dollarSign);
            double paymentDue = Convert.ToDouble(paymoney);
            double amountPaid = Convert.ToDouble(textBox20.Text);
            double change = amountPaid - paymentDue;
            textBox21.Text = change.ToString("c2");

            if (change < 0)
            {
                MessageBox.Show("Please pay your balance");
                button8.Enabled = false; // FIX-04
            }
            else
            {
                button8.Enabled = true;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // US-35: Offer to save receipt before data is cleared
            DialogResult saveReceipt = MessageBox.Show("Would you like to save a receipt of this order?", "Save Receipt", MessageBoxButtons.YesNo);
            if (saveReceipt == DialogResult.Yes)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Text Files (*.txt)|*.txt";
                sfd.FileName = "Receipt_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("========================================");
                    sb.AppendLine("            PIZZA EXPRESS");
                    sb.AppendLine("========================================");
                    sb.AppendLine("Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sb.AppendLine();
                    sb.AppendLine("CUSTOMER INFORMATION");
                    sb.AppendLine("Name:        " + textBox11.Text + " " + textBox12.Text);
                    sb.AppendLine("Address:     " + textBox13.Text);
                    sb.AppendLine("City:        " + textBox14.Text);
                    sb.AppendLine("Province:    " + comboBox1.Text);
                    sb.AppendLine("Postal Code: " + textBox15.Text);
                    sb.AppendLine("Contact No:  " + textBox16.Text);
                    sb.AppendLine();
                    sb.AppendLine("ORDER SUMMARY");
                    sb.AppendLine(string.Format("{0,-38} {1,5} {2,10}", "Item", "Qty", "Price CAD"));
                    sb.AppendLine(new string('-', 55));
                    foreach (ListViewItem item in listView1.Items)
                    {
                        sb.AppendLine(string.Format("{0,-38} {1,5} {2,10}",
                            item.Text,
                            item.SubItems[1].Text,
                            item.SubItems[2].Text));
                    }
                    sb.AppendLine(new string('-', 55));
                    sb.AppendLine(string.Format("{0,-44} {1,10}", "Subtotal:", textBox8.Text));
                    sb.AppendLine(string.Format("{0,-44} {1,10}", "HST (13%):", textBox9.Text));
                    sb.AppendLine(string.Format("{0,-44} {1,10}", "Total Due:", textBox19.Text));
                    sb.AppendLine();
                    sb.AppendLine("PAYMENT");
                    sb.AppendLine("Method:       " + comboBox2.Text);
                    sb.AppendLine("Amount Paid:  " + textBox20.Text);
                    sb.AppendLine("Change:       " + textBox21.Text);
                    sb.AppendLine();
                    sb.AppendLine("========================================");
                    sb.AppendLine(" Thank you for ordering at Pizza Express!");
                    sb.AppendLine(" Delivery in approximately 30 minutes.");
                    sb.AppendLine("========================================");

                    File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("Receipt saved successfully.", "Receipt Saved");
                }
            }

            DialogResult dialog = MessageBox.Show("Thanks for ordering at Pizza Express. Your ordered items will be ready and delivered in 30 minutes. Do you want to order some more?", "Exit", MessageBoxButtons.YesNo);

            if (dialog == DialogResult.Yes)
            {
                //Clearing all data
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = false;
                checkBox5.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
                checkBox9.Checked = false;
                checkBox10.Checked = false;
                checkBox11.Checked = false;
                checkBox12.Checked = false;
                checkBox13.Checked = false;
                checkBox14.Checked = false;
                checkBox15.Checked = false;
                checkBox16.Checked = false;
                checkBox17.Checked = false;
                checkBox18.Checked = false;
                checkBox19.Checked = false;
                checkBox20.Checked = false;
                checkBox21.Checked = false;
                checkBox22.Checked = false;
                checkBox23.Checked = false;
                checkBox24.Checked = false;
                checkBox25.Checked = false;
                checkBox26.Checked = false;
                checkBox27.Checked = false;
                checkBox28.Checked = false;

                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";

                listView1.Items.Clear();
                textBox8.Text = "";
                textBox9.Text = "";
                textBox10.Text = "";

                textBox11.Text = "";
                textBox12.Text = "";
                textBox13.Text = "";
                textBox14.Text = "";
                textBox15.Text = "";
                textBox16.Text = "";
                textBox17.Text = "";
                textBox18.Text = "";
                textBox19.Text = "";
                textBox20.Text = "";
                textBox21.Text = "";
                comboBox1.Text = "";
                comboBox2.Text = "";

                // FIX-06: restore default pizza selections and reset payment state
                radioButton1.Checked = true;
                radioButton5.Checked = true;
                numericUpDown1.Value = 1;   // US-30: reset pizza qty
                _stagedPizzas.Clear();       // US-31: clear any staged pizzas
                button8.Enabled = false;
                textBox18.Enabled = false;
                label15.Text = "*Card No:"; // US-34: reset label after promo card use

                tabControl1.SelectTab("tabPage1");
            }

            else if (dialog == DialogResult.No)
            {
                this.Close();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.Text == "Cash")
            {
                textBox18.Enabled = false;
                label15.Text = "*Card No:"; // restore label
            }
            else if (comboBox2.Text == "Promo Card")
            {
                textBox18.Enabled = true;
                label15.Text = "*Promo Code:"; // US-34: contextual label change
            }
            else
            {
                textBox18.Enabled = true; // FIX-05: re-enable for Credit/Debit card
                label15.Text = "*Card No:"; // restore label
            }
        }
    }
}
//cpfn
