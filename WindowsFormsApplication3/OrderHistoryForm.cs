using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3
{
    /// <summary>
    /// Displays all past orders loaded from the JSON store.
    /// Supports live text search, date range filtering, and CSV export.
    /// Built entirely in code — no Designer file.
    /// </summary>
    public class OrderHistoryForm : Form
    {
        private readonly IOrderRepository _repo;
        private List<OrderRecord> _allOrders = new List<OrderRecord>();

        // UI controls
        private ListView         _listView;
        private TextBox          _txtSearch;
        private DateTimePicker   _dtpFrom;
        private DateTimePicker   _dtpTo;
        private CheckBox         _chkDateFilter;
        private Label            _lblResultCount;
        private Button           _btnDetails;
        private Button           _btnExport;
        private Button           _btnClose;

        /// <summary>Initialises the history form backed by the given repository.</summary>
        public OrderHistoryForm(IOrderRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException("repo");
            BuildUi();
            LoadOrders();
        }

        // ── UI construction ───────────────────────────────────────────────────
        private void BuildUi()
        {
            Text            = "Pizza Express — Order History";
            Size            = new Size(860, 560);
            MinimumSize     = new Size(640, 400);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            Font            = new Font("Segoe UI", 9f);

            // ── Filter bar ────────────────────────────────────────────────────
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 78, Padding = new Padding(8, 6, 8, 4) };

            // Row 1: search box
            var lblSearch = new Label { Text = "Search:", AutoSize = true, Location = new Point(8, 10) };
            _txtSearch = new TextBox { Location = new Point(60, 7), Width = 280 };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            _lblResultCount = new Label
            {
                AutoSize  = true,
                ForeColor = Color.Gray,
                Location  = new Point(354, 10),
            };

            // Row 2: date range
            _chkDateFilter = new CheckBox
            {
                Text     = "Date range:",
                Location = new Point(8, 44),
                AutoSize = true,
                Checked  = false,
            };
            _chkDateFilter.CheckedChanged += (s, e) =>
            {
                _dtpFrom.Enabled = _chkDateFilter.Checked;
                _dtpTo.Enabled   = _chkDateFilter.Checked;
                ApplyFilter();
            };

            _dtpFrom = new DateTimePicker
            {
                Location = new Point(100, 42),
                Width    = 130,
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today.AddMonths(-1),
                Enabled  = false,
            };
            _dtpFrom.ValueChanged += (s, e) => ApplyFilter();

            var lblTo = new Label { Text = "to", AutoSize = true, Location = new Point(238, 46) };

            _dtpTo = new DateTimePicker
            {
                Location = new Point(255, 42),
                Width    = 130,
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today,
                Enabled  = false,
            };
            _dtpTo.ValueChanged += (s, e) => ApplyFilter();

            filterPanel.Controls.AddRange(new Control[]
            {
                lblSearch, _txtSearch, _lblResultCount,
                _chkDateFilter, _dtpFrom, lblTo, _dtpTo,
            });

            // ── Order list ────────────────────────────────────────────────────
            _listView = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
            };
            _listView.Columns.Add("Date / Time",   150);
            _listView.Columns.Add("Customer",       170);
            _listView.Columns.Add("Region",         120);
            _listView.Columns.Add("Payment",        110);
            _listView.Columns.Add("Total (NZD)",    100);
            _listView.DoubleClick += (s, e) => ShowDetails();

            // ── Button bar ────────────────────────────────────────────────────
            _btnDetails = new Button { Text = "View Details", Width = 110, Height = 30 };
            _btnDetails.Click += (s, e) => ShowDetails();

            _btnExport = new Button { Text = "Export CSV", Width = 100, Height = 30 };
            _btnExport.Click += (s, e) => ExportCsv();

            _btnClose = new Button { Text = "Close", Width = 80, Height = 30 };
            _btnClose.Click += (s, e) => Close();

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(4),
            };
            btnPanel.Controls.Add(_btnClose);
            btnPanel.Controls.Add(_btnDetails);
            btnPanel.Controls.Add(_btnExport);

            Controls.Add(_listView);
            Controls.Add(filterPanel);
            Controls.Add(btnPanel);
        }

        // ── Data loading ──────────────────────────────────────────────────────
        private void LoadOrders()
        {
            _allOrders = _repo.LoadAll();
            _allOrders.Sort((a, b) => b.OrderDate.CompareTo(a.OrderDate));
            ApplyFilter();
        }

        // ── Filtering ─────────────────────────────────────────────────────────
        private void ApplyFilter()
        {
            string search = (_txtSearch?.Text ?? string.Empty).Trim().ToLowerInvariant();
            bool   useDate = _chkDateFilter?.Checked ?? false;
            DateTime from  = _dtpFrom?.Value.Date ?? DateTime.MinValue;
            DateTime to    = (_dtpTo?.Value.Date ?? DateTime.MaxValue).AddDays(1); // inclusive

            _listView.Items.Clear();

            foreach (OrderRecord r in _allOrders)
            {
                // Date range filter
                if (useDate && (r.OrderDate < from || r.OrderDate >= to))
                    continue;

                // Text search (customer, region, payment method)
                if (!string.IsNullOrEmpty(search))
                {
                    bool match = (r.CustomerName  ?? string.Empty).ToLowerInvariant().Contains(search)
                              || (r.Region        ?? string.Empty).ToLowerInvariant().Contains(search)
                              || (r.PaymentMethod ?? string.Empty).ToLowerInvariant().Contains(search)
                              || r.OrderDate.ToString("yyyy-MM-dd").Contains(search);
                    if (!match) continue;
                }

                var item = new ListViewItem(r.OrderDate.ToString("yyyy-MM-dd  HH:mm:ss"));
                item.SubItems.Add(r.CustomerName);
                item.SubItems.Add(r.Region);
                item.SubItems.Add(r.PaymentMethod);
                item.SubItems.Add(r.Total.ToString("C2", new CultureInfo("en-NZ")));
                item.Tag = r;
                _listView.Items.Add(item);
            }

            int count = _listView.Items.Count;
            if (count == 0)
            {
                _listView.Items.Add(new ListViewItem("No matching orders found."));
                if (_lblResultCount != null) _lblResultCount.Text = "0 results";
            }
            else
            {
                if (_lblResultCount != null)
                    _lblResultCount.Text = $"{count} order{(count == 1 ? "" : "s")}";
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

        // ── CSV export ────────────────────────────────────────────────────────
        private void ExportCsv()
        {
            if (_listView.Items.Count == 0) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter   = "CSV Files (*.csv)|*.csv";
                sfd.FileName = $"OrderHistory_{DateTime.Today:yyyyMMdd}.csv";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("Date/Time,Customer,Region,Payment,Total NZD");

                foreach (ListViewItem item in _listView.Items)
                {
                    if (item.Tag == null) continue;  // skip placeholder rows
                    var r = (OrderRecord)item.Tag;
                    sb.AppendLine(
                        $"\"{r.OrderDate:yyyy-MM-dd HH:mm:ss}\"," +
                        $"\"{r.CustomerName}\"," +
                        $"\"{r.Region}\"," +
                        $"\"{r.PaymentMethod}\"," +
                        $"{r.Total:F2}");
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Exported {_listView.Items.Count} orders to CSV.", "Export Complete");
            }
        }
    }
}
