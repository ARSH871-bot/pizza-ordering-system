using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3
{
    /// <summary>
    /// Displays all past orders loaded from the JSON store.
    /// Built entirely in code — no Designer file.
    /// </summary>
    public class OrderHistoryForm : Form
    {
        private readonly OrderRepository _repo;
        private ListView _listView;
        private Button   _btnDetails;
        private Button   _btnClose;

        public OrderHistoryForm(OrderRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException("repo");
            BuildUi();
            LoadOrders();
        }

        // ── UI construction ───────────────────────────────────────────────────
        private void BuildUi()
        {
            Text            = "Pizza Express — Order History";
            Size            = new Size(820, 480);
            MinimumSize     = new Size(600, 360);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            Font            = new Font("Segoe UI", 9f);

            _listView = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
            };
            _listView.Columns.Add("Date / Time",    150);
            _listView.Columns.Add("Customer",        160);
            _listView.Columns.Add("Region",          120);
            _listView.Columns.Add("Payment",         110);
            _listView.Columns.Add("Total (NZD)",     100);
            _listView.DoubleClick += (s, e) => ShowDetails();

            _btnDetails = new Button { Text = "View Details", Width = 120, Height = 30 };
            _btnDetails.Click += (s, e) => ShowDetails();

            _btnClose = new Button { Text = "Close", Width = 80, Height = 30 };
            _btnClose.Click += (s, e) => Close();

            var panel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(4),
            };
            panel.Controls.Add(_btnClose);
            panel.Controls.Add(_btnDetails);

            Controls.Add(_listView);
            Controls.Add(panel);
        }

        // ── Data loading ──────────────────────────────────────────────────────
        private void LoadOrders()
        {
            _listView.Items.Clear();
            List<OrderRecord> orders = _repo.LoadAll();

            // Most-recent first
            orders.Sort((a, b) => b.OrderDate.CompareTo(a.OrderDate));

            foreach (OrderRecord r in orders)
            {
                var item = new ListViewItem(r.OrderDate.ToString("yyyy-MM-dd  HH:mm:ss"));
                item.SubItems.Add(r.CustomerName);
                item.SubItems.Add(r.Region);
                item.SubItems.Add(r.PaymentMethod);
                item.SubItems.Add(r.Total.ToString("C2", new CultureInfo("en-NZ")));
                item.Tag = r;
                _listView.Items.Add(item);
            }

            if (_listView.Items.Count == 0)
            {
                var placeholder = new ListViewItem("No orders found.");
                _listView.Items.Add(placeholder);
            }
        }

        // ── Detail view ───────────────────────────────────────────────────────
        private void ShowDetails()
        {
            if (_listView.SelectedItems.Count == 0) return;

            var record = _listView.SelectedItems[0].Tag as OrderRecord;
            if (record == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("=== Pizza Express — Order Receipt ===");
            sb.AppendLine($"Order ID    : {record.Id}");
            sb.AppendLine($"Date / Time : {record.OrderDate:yyyy-MM-dd  HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("--- Customer ---");
            sb.AppendLine($"Name        : {record.CustomerName}");
            sb.AppendLine($"Address     : {record.Address}");
            sb.AppendLine($"City        : {record.City},  {record.Region}  {record.PostalCode}");
            sb.AppendLine();
            sb.AppendLine("--- Items ---");
            foreach (var line in record.Lines)
                sb.AppendLine($"  {line.Item,-35}  x{line.Quantity,2}   {line.Price,8:C2}");
            sb.AppendLine();
            sb.AppendLine($"{"Subtotal",-40}  {record.Subtotal,8:C2}");
            sb.AppendLine($"{"GST (15%)",-40}  {record.Tax,8:C2}");
            sb.AppendLine($"{"TOTAL (NZD)",-40}  {record.Total,8:C2}");
            sb.AppendLine();
            sb.AppendLine($"Payment     : {record.PaymentMethod}");

            MessageBox.Show(sb.ToString(), "Order Details",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
