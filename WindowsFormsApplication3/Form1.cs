using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WindowsFormsApplication3.Config;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        // ── Services ─────────────────────────────────────────────────────────
        private readonly ILogger          _logger         = new FileLogger();
        private readonly ICartService     _cart           = new CartService();
        private readonly IPromoEngine     _promoEngine    = new PromoEngine();
        private readonly IOrderValidator  _validator      = new OrderValidator();
        private readonly IReceiptWriter   _receiptWriter  = new ReceiptWriter();
        private readonly IOrderRepository _repo           = new OrderRepository();

        // ── State ─────────────────────────────────────────────────────────────
        private readonly List<ListViewItem> _stagedPizzas = new List<ListViewItem>();
        private string _lastReceiptText;   // cached for clipboard / print

        // Validation colours
        private static readonly Color ColourValid   = Color.Honeydew;
        private static readonly Color ColourInvalid = Color.MistyRose;

        // UI components added programmatically
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _liveTotalLabel;
        private ContextMenuStrip     _lvContextMenu;
        private ToolTip              _toolTip;

        public Form1()
        {
            InitializeComponent();
        }

        // =====================================================================
        // Form load
        // =====================================================================

        private void Form1_Load(object sender, EventArgs e)
        {
            // Show version in title bar
            this.Text = $"Pizza Express New Zealand  —  v{Application.ProductVersion}";
            _logger.Info($"Application started  v{Application.ProductVersion}");

            rbSizeSmall.Checked = true;
            rbCrustNormal.Checked = true;
            nudPizzaQty.Value = 1;

            txtSubtotal.Enabled    = false;
            txtTax.Enabled         = false;
            txtTotalDue.Enabled    = false;
            txtAmountDue.Enabled   = false;
            txtChange.Enabled      = false;
            txtCardOrPromo.Enabled = false;

            foreach (string region in AppConfig.NZRegions)
                cboRegion.Items.Add(region);

            foreach (string method in AppConfig.PaymentMethods)
                cboPaymentMethod.Items.Add(method);

            btnSubmitOrder.Enabled = false;

            // ── Input length limits (security + data quality) ─────────────────
            txtFirstName.MaxLength  = 50;
            txtLastName.MaxLength   = 50;
            txtAddress.MaxLength    = 100;
            txtCity.MaxLength       = 60;
            txtPostalCode.MaxLength = 4;
            txtContactNo.MaxLength  = 15;
            txtEmail.MaxLength      = 100;
            txtCardOrPromo.MaxLength = 30;
            txtAmountPaid.MaxLength  = 12;

            // ── Accessibility labels ───────────────────────────────────────────
            txtFirstName.AccessibleName  = "First Name";
            txtLastName.AccessibleName   = "Last Name";
            txtAddress.AccessibleName    = "Delivery Address";
            txtCity.AccessibleName       = "City";
            cboRegion.AccessibleName     = "Region";
            txtPostalCode.AccessibleName = "Postal Code, 4 digits";
            txtContactNo.AccessibleName  = "Contact Number, optional";
            txtEmail.AccessibleName      = "Email Address, optional";
            cboPaymentMethod.AccessibleName = "Payment Method";
            txtCardOrPromo.AccessibleName   = "Card Number or Promo Code";
            txtAmountPaid.AccessibleName    = "Amount Paid";
            txtAmountDue.AccessibleName     = "Amount Due";
            txtChange.AccessibleName        = "Change";
            txtSubtotal.AccessibleName      = "Subtotal";
            txtTax.AccessibleName           = "GST Amount";
            txtTotalDue.AccessibleName      = "Total Due";
            nudPizzaQty.AccessibleName      = "Pizza Quantity, 1 to 20";
            btnConfirmOrder.AccessibleName   = "Confirm Order, Alt C";
            btnCheckOut.AccessibleName       = "Proceed to Checkout, Alt P";
            btnGoBack.AccessibleName         = "Go Back, Escape";
            btnSubmitOrder.AccessibleName    = "Submit and Pay";
            btnPay.AccessibleName            = "Validate Payment";
            btnClearOrder.AccessibleName     = "Clear Order";
            btnAddPizzaToCart.AccessibleName = "Add Pizza to Cart";
            lvOrder.AccessibleName           = "Order Items. Right-click to remove.";

            // ── Postal code input masking (digits only, max 4 chars) ──────────
            txtPostalCode.KeyPress += (s, ev) =>
            {
                if (!char.IsDigit(ev.KeyChar) && ev.KeyChar != '\b') ev.Handled = true;
            };
            txtPostalCode.TextChanged += (s, ev) =>
            {
                if (txtPostalCode.Text.Length > 4)
                    txtPostalCode.Text = txtPostalCode.Text.Substring(0, 4);
                txtPostalCode.SelectionStart = txtPostalCode.Text.Length;
            };

            // ── Inline validation wiring ───────────────────────────────────────
            txtPostalCode.Leave += txtPostalCode_Leave;
            txtContactNo.Leave  += txtContactNo_Leave;
            txtEmail.Leave      += txtEmail_Leave;

            // ── Status bar ─────────────────────────────────────────────────────
            var statusStrip = new StatusStrip { SizingGrip = false };
            _statusLabel = new ToolStripStatusLabel
            {
                Text      = "Cart is empty",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Spring    = true,
            };
            _liveTotalLabel = new ToolStripStatusLabel
            {
                Text      = "Current pizza: $0.00",
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                ForeColor = Color.DarkGreen,
                Font      = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
            };
            statusStrip.Items.Add(_statusLabel);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(_liveTotalLabel);
            this.Controls.Add(statusStrip);

            // ── Live total: wire all Tab 1 controls ───────────────────────────
            EventHandler liveUpdate = (s, ev) => RecalculateLiveTotal();
            KeyEventHandler liveUpdateKey = (s, ev) => RecalculateLiveTotal();

            // Size + crust radio buttons
            rbSizeSmall.CheckedChanged      += liveUpdate; rbSizeMedium.CheckedChanged   += liveUpdate;
            rbSizeLarge.CheckedChanged      += liveUpdate; rbSizeExtraLarge.CheckedChanged += liveUpdate;
            rbCrustNormal.CheckedChanged    += liveUpdate; rbCrustCheesy.CheckedChanged  += liveUpdate;
            rbCrustSausage.CheckedChanged   += liveUpdate;
            nudPizzaQty.ValueChanged        += liveUpdate;

            // Toppings
            cbPepperoni.CheckedChanged      += liveUpdate; cbExtraCheese.CheckedChanged   += liveUpdate;
            cbMushroom.CheckedChanged       += liveUpdate; cbHam.CheckedChanged           += liveUpdate;
            cbBacon.CheckedChanged          += liveUpdate; cbGroundBeef.CheckedChanged    += liveUpdate;
            cbJalapeno.CheckedChanged       += liveUpdate; cbPineapple.CheckedChanged     += liveUpdate;
            cbDriedShrimps.CheckedChanged   += liveUpdate; cbAnchovies.CheckedChanged     += liveUpdate;
            cbSunDriedTomatoes.CheckedChanged += liveUpdate; cbSpinach.CheckedChanged     += liveUpdate;
            cbRoastedGarlic.CheckedChanged  += liveUpdate; cbShreddedChicken.CheckedChanged += liveUpdate;

            // Drinks
            cbCoke.CheckedChanged     += liveUpdate; cbDietCoke.CheckedChanged  += liveUpdate;
            cbIcedTea.CheckedChanged  += liveUpdate; cbGingerAle.CheckedChanged += liveUpdate;
            cbSprite.CheckedChanged   += liveUpdate; cbRootBeer.CheckedChanged  += liveUpdate;
            cbWater.CheckedChanged    += liveUpdate;
            txtQtyCoke.TextChanged    += liveUpdate; txtQtyDietCoke.TextChanged  += liveUpdate;
            txtQtyIcedTea.TextChanged += liveUpdate; txtQtyGingerAle.TextChanged += liveUpdate;
            txtQtySprite.TextChanged  += liveUpdate; txtQtyRootBeer.TextChanged  += liveUpdate;
            txtQtyWater.TextChanged   += liveUpdate;

            // Sides / dips
            cbChickenWings.CheckedChanged   += liveUpdate; cbPoutine.CheckedChanged      += liveUpdate;
            cbOnionRings.CheckedChanged     += liveUpdate; cbCheesyGarlicBread.CheckedChanged += liveUpdate;
            cbGarlicDip.CheckedChanged      += liveUpdate; cbBBQDip.CheckedChanged       += liveUpdate;
            cbSourCreamDip.CheckedChanged   += liveUpdate;

            RecalculateLiveTotal();

            // ── ListView right-click: Remove selected item ─────────────────────
            _lvContextMenu = new ContextMenuStrip();
            var menuRemove = new ToolStripMenuItem("Remove selected item");
            menuRemove.Click += (s, ev) =>
            {
                if (lvOrder.SelectedItems.Count > 0)
                {
                    lvOrder.Items.Remove(lvOrder.SelectedItems[0]);
                    UpdateStatusBar();
                }
            };
            _lvContextMenu.Items.Add(menuRemove);
            lvOrder.ContextMenuStrip = _lvContextMenu;

            // ── Tooltips ───────────────────────────────────────────────────────
            _toolTip = new ToolTip { AutoPopDelay = 5000, InitialDelay = 500 };
            _toolTip.SetToolTip(txtPostalCode,  "Enter your 4-digit NZ postal code (e.g. 1010)");
            _toolTip.SetToolTip(txtContactNo,   "Optional — digits only, 7–15 characters (e.g. +6421234567)");
            _toolTip.SetToolTip(txtEmail,       "Optional — used for email receipt in future");
            _toolTip.SetToolTip(nudPizzaQty,    "Select quantity 1–20. Price scales automatically.");
            _toolTip.SetToolTip(txtAmountPaid,  "Enter the amount the customer hands over");
            _toolTip.SetToolTip(btnAddPizzaToCart, "Stage this pizza and configure another. All staged pizzas go into the same order.");
            _toolTip.SetToolTip(btnConfirmOrder,   "Lock in your order and proceed to review (Alt+C)");
            _toolTip.SetToolTip(btnCheckOut,       "Proceed to payment (Alt+P)");
            _toolTip.SetToolTip(btnGoBack,         "Return to order review (Esc)");

            // ── Order History button (programmatic, bottom-right of Tab 2) ────
            var btnHistory = new Button
            {
                Text     = "Order History",
                Width    = 110,
                Height   = 26,
                Location = new Point(btnCheckOut.Left - 120, btnCheckOut.Top),
                Parent   = btnCheckOut.Parent,
            };
            _toolTip.SetToolTip(btnHistory, "View all past orders (Alt+H)");
            btnHistory.Click += (s, ev) =>
            {
                using (var f = new OrderHistoryForm(_repo))
                    f.ShowDialog(this);
            };
            btnCheckOut.Parent.Controls.Add(btnHistory);

            // ── About button (far right, same row as History) ─────────────────
            var btnAbout = new Button
            {
                Text     = "About",
                Width    = 70,
                Height   = 26,
                Location = new Point(btnCheckOut.Right + 8, btnCheckOut.Top),
                Parent   = btnCheckOut.Parent,
            };
            btnAbout.Click += (s, ev) => ShowAboutDialog();
            btnCheckOut.Parent.Controls.Add(btnAbout);
        }

        private void ShowAboutDialog()
        {
            string msg =
                $"Pizza Express New Zealand\n" +
                $"Version {Application.ProductVersion}\n\n" +
                $"A Windows Forms POS system built in C# (.NET Framework 4.8).\n\n" +
                $"Architecture: 3-layer (Models / Services / UI)\n" +
                $"Test suite:   95 unit + integration tests\n" +
                $"CI/CD:        GitHub Actions\n\n" +
                $"Data saved to: {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\PizzaExpress\\";

            MessageBox.Show(msg, "About Pizza Express NZ",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // =====================================================================
        // Keyboard shortcuts
        // =====================================================================

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Alt | Keys.C:
                    btnConfirmOrder_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Alt | Keys.H:
                    using (var f = new OrderHistoryForm(_repo))
                        f.ShowDialog(this);
                    return true;

                case Keys.Alt | Keys.P:
                    if (tabControl1.SelectedTab.Name == "tabPage2")
                        btnCheckOut_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Escape:
                    if (tabControl1.SelectedTab.Name == "tabPage3")
                        tabControl1.SelectTab("tabPage2");
                    else if (tabControl1.SelectedTab.Name == "tabPage2")
                        tabControl1.SelectTab("tabPage1");
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
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

            txtSubtotal.Text = subtotal.ToString("C2");
            txtTax.Text      = tax.ToString("C2");
            txtTotalDue.Text = totalDue.ToString("C2");

            UpdateStatusBar();
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
            RecalculateLiveTotal();
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
            if (lvOrder.Items.Count == 0) return;

            if (MessageBox.Show(
                    "This will remove all items from the order. Are you sure?",
                    "Clear Order", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes)
                return;

            lvOrder.Items.Clear();
            txtSubtotal.Text  = "";
            txtTax.Text       = "";
            txtTotalDue.Text  = "";
            btnSubmitOrder.Enabled = false;
            UpdateStatusBar();
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
                _logger.Info($"Promo applied  code={code}  discount={promoResult.DiscountedTotal:C2}");
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
            _lastReceiptText = _receiptWriter.Build(order);

            // Persist order to JSON history
            try
            {
                var record = BuildOrderRecord(order);
                _repo.Save(record);
                _logger.Info($"Order saved  id={record.Id}  customer={order.Customer.FullName}" +
                             $"  total={order.Total:C2}  method={order.PaymentMethod}");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to persist order to history", ex);
                // History persistence is non-critical — never block the user
            }

            // ── Receipt options dialog ─────────────────────────────────────────
            using (var dlg = new Form())
            {
                dlg.Text            = "Order Confirmed — Receipt Options";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.StartPosition  = FormStartPosition.CenterParent;
                dlg.Size           = new Size(320, 160);

                var btnSave = new Button { Text = "Save to File",     Width = 120, Location = new Point(16,  20) };
                var btnCopy = new Button { Text = "Copy to Clipboard", Width = 120, Location = new Point(16,  60) };
                var btnPrint = new Button { Text = "Print",           Width = 120, Location = new Point(16, 100) };
                var btnSkip = new Button  { Text = "Skip",            Width = 120, Location = new Point(160, 60) };

                btnSave.Click += (s, ev) =>
                {
                    using (var sfd = new SaveFileDialog())
                    {
                        sfd.Filter   = "Text Files (*.txt)|*.txt";
                        sfd.FileName = $"Receipt_{order.OrderDate:yyyyMMdd_HHmmss}.txt";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllText(sfd.FileName, _lastReceiptText);
                            MessageBox.Show("Receipt saved.", "Saved");
                        }
                    }
                };

                btnCopy.Click += (s, ev) =>
                {
                    Clipboard.SetText(_lastReceiptText);
                    MessageBox.Show("Receipt copied to clipboard.", "Copied");
                };

                btnPrint.Click += (s, ev) => PrintReceipt(order);

                btnSkip.Click  += (s, ev) => dlg.Close();

                dlg.Controls.AddRange(new Control[] { btnSave, btnCopy, btnPrint, btnSkip });
                dlg.ShowDialog(this);
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

        private void PrintReceipt(Order order)
        {
            var pd = new PrintDocument();
            pd.PrintPage += (s, ev) =>
            {
                var font = new Font("Courier New", 9f);
                ev.Graphics.DrawString(_lastReceiptText, font, Brushes.Black,
                    ev.MarginBounds.Left, ev.MarginBounds.Top);
                font.Dispose();
            };

            using (var preview = new PrintPreviewDialog { Document = pd, Width = 700, Height = 900 })
                preview.ShowDialog(this);
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

        // =====================================================================
        // Inline field validation (Leave events)
        // =====================================================================

        private void txtPostalCode_Leave(object sender, EventArgs e)
        {
            var result = _validator.ValidatePostalCode(txtPostalCode.Text);
            txtPostalCode.BackColor = result.IsValid ? ColourValid : ColourInvalid;
        }

        private void txtContactNo_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtContactNo.Text))
            {
                txtContactNo.BackColor = SystemColors.Window;
                return;
            }
            var result = _validator.ValidateContactNo(txtContactNo.Text);
            txtContactNo.BackColor = result.IsValid ? ColourValid : ColourInvalid;
        }

        private void txtEmail_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.BackColor = SystemColors.Window;
                return;
            }
            var result = _validator.ValidateEmail(txtEmail.Text);
            txtEmail.BackColor = result.IsValid ? ColourValid : ColourInvalid;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
                this.Close();
        }

        // =====================================================================
        // Private helpers
        // =====================================================================

        private void RecalculateLiveTotal()
        {
            if (_liveTotalLabel == null) return;

            decimal running = 0m;

            // Staged pizzas already confirmed
            foreach (var staged in _stagedPizzas)
            {
                decimal p;
                decimal.TryParse(staged.SubItems[2].Text, out p);
                running += p;
            }

            // Current pizza configuration
            PizzaSize? size  = null;
            CrustType? crust = null;
            if (rbSizeSmall.Checked)      size  = PizzaSize.Small;
            else if (rbSizeMedium.Checked)     size  = PizzaSize.Medium;
            else if (rbSizeLarge.Checked)      size  = PizzaSize.Large;
            else if (rbSizeExtraLarge.Checked) size  = PizzaSize.ExtraLarge;
            if (rbCrustNormal.Checked)    crust = CrustType.Normal;
            else if (rbCrustCheesy.Checked)    crust = CrustType.Cheesy;
            else if (rbCrustSausage.Checked)   crust = CrustType.Sausage;

            if (size.HasValue && crust.HasValue)
            {
                var toppings = CollectSelectedToppingNames();
                var items    = _cart.BuildPizzaItems(size.Value, crust.Value, (int)nudPizzaQty.Value, toppings);
                running += _cart.CalculateSubtotal(items);
            }

            // Drinks
            running += DrinkRunning(cbCoke,      txtQtyCoke,      AppConfig.DrinkCanPrice);
            running += DrinkRunning(cbDietCoke,  txtQtyDietCoke,  AppConfig.DrinkCanPrice);
            running += DrinkRunning(cbIcedTea,   txtQtyIcedTea,   AppConfig.DrinkCanPrice);
            running += DrinkRunning(cbGingerAle, txtQtyGingerAle, AppConfig.DrinkCanPrice);
            running += DrinkRunning(cbSprite,    txtQtySprite,    AppConfig.DrinkCanPrice);
            running += DrinkRunning(cbRootBeer,  txtQtyRootBeer,  AppConfig.DrinkCanPrice);
            running += DrinkRunning(cbWater,     txtQtyWater,     AppConfig.WaterPrice);

            // Sides
            if (cbChickenWings.Checked)    running += AppConfig.SidePrice;
            if (cbPoutine.Checked)         running += AppConfig.SidePrice;
            if (cbOnionRings.Checked)      running += AppConfig.SidePrice;
            if (cbCheesyGarlicBread.Checked) running += AppConfig.SidePrice;

            decimal total = _cart.CalculateTotal(running);
            _liveTotalLabel.Text = $"Live total (incl. GST):  {total:C2}";
            _liveTotalLabel.ForeColor = total > 0 ? Color.DarkGreen : Color.Gray;
        }

        private static decimal DrinkRunning(CheckBox cb, TextBox tb, decimal unitPrice)
        {
            if (!cb.Checked) return 0m;
            int qty;
            return int.TryParse(tb.Text, out qty) && qty > 0 ? qty * unitPrice : 0m;
        }

        private void UpdateStatusBar()
        {
            int count = lvOrder.Items.Count;
            if (count == 0)
            {
                _statusLabel.Text = "Cart is empty";
                return;
            }

            decimal subtotal = 0m;
            foreach (ListViewItem lvi in lvOrder.Items)
            {
                decimal p;
                decimal.TryParse(lvi.SubItems[2].Text, out p);
                subtotal += p;
            }
            decimal total = subtotal + Math.Round(subtotal * AppConfig.TaxRate, 2);
            _statusLabel.Text = $"{count} item{(count == 1 ? "" : "s")} in cart  |  Total (incl. GST): {total:C2}";
        }

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

        private List<string> CollectSelectedToppingNames()
        {
            var names = new List<string>();
            var toppingMap = new (CheckBox cb, string name)[]
            {
                (cbPepperoni,       "Pepperoni"),
                (cbExtraCheese,     "Extra Cheese"),
                (cbMushroom,        "Mushroom"),
                (cbHam,             "Ham"),
                (cbBacon,           "Bacon"),
                (cbGroundBeef,      "Ground Beef"),
                (cbJalapeno,        "Jalapeno"),
                (cbPineapple,       "Pineapple"),
                (cbDriedShrimps,    "Dried Shrimps"),
                (cbAnchovies,       "Anchovies"),
                (cbSunDriedTomatoes,"Sun Dried Tomatoes"),
                (cbSpinach,         "Spinach"),
                (cbRoastedGarlic,   "Roasted Garlic"),
                (cbShreddedChicken, "Shredded Chicken"),
            };
            foreach (var (cb, name) in toppingMap)
                if (cb.Checked) names.Add(name);
            return names;
        }

        private List<ListViewItem> BuildCurrentPizzaItems()
        {
            var lvItems = new List<ListViewItem>();

            PizzaSize? size  = null;
            CrustType? crust = null;

            if (rbSizeSmall.Checked)       size  = PizzaSize.Small;
            else if (rbSizeMedium.Checked)      size  = PizzaSize.Medium;
            else if (rbSizeLarge.Checked)       size  = PizzaSize.Large;
            else if (rbSizeExtraLarge.Checked)  size  = PizzaSize.ExtraLarge;
            if (rbCrustNormal.Checked)     crust = CrustType.Normal;
            else if (rbCrustCheesy.Checked)     crust = CrustType.Cheesy;
            else if (rbCrustSausage.Checked)    crust = CrustType.Sausage;

            if (!size.HasValue || !crust.HasValue) return lvItems;

            var orderItems = _cart.BuildPizzaItems(size.Value, crust.Value,
                                                   (int)nudPizzaQty.Value,
                                                   CollectSelectedToppingNames());
            foreach (var oi in orderItems)
            {
                var lvi = new ListViewItem(oi.Name);
                lvi.SubItems.Add(oi.Quantity > 0 ? oi.Quantity.ToString() : "");
                lvi.SubItems.Add(oi.TotalPrice.ToString("F2"));
                lvItems.Add(lvi);
            }
            return lvItems;
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

        private OrderRecord BuildOrderRecord(Order order)
        {
            var record = new OrderRecord
            {
                Id            = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                OrderDate     = order.OrderDate,
                CustomerName  = order.Customer.FullName,
                Address       = order.Customer.Address,
                City          = order.Customer.City,
                Region        = order.Customer.Region,
                PostalCode    = order.Customer.PostalCode,
                PaymentMethod = order.PaymentMethod,
                Subtotal      = order.Subtotal,
                Tax           = order.Tax,
                Total         = order.Total,
            };
            foreach (var item in order.Items)
                record.Lines.Add(new OrderLineRecord
                {
                    Item     = item.Name,
                    Quantity = item.Quantity,
                    Price    = item.TotalPrice,
                });
            return record;
        }

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
            btnSubmitOrder.Enabled = false;
            txtCardOrPromo.Enabled = false;
            lblCardOrPromo.Text    = "*Card No:";

            // Reset inline-validation colours
            txtPostalCode.BackColor = SystemColors.Window;
            txtContactNo.BackColor  = SystemColors.Window;
            txtEmail.BackColor      = SystemColors.Window;

            // Reset status bar
            UpdateStatusBar();
        }
    }
}
