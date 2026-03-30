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
    /// Displays all past orders loaded from the SQLite store.
    /// Supports live SQL-backed text search, date range filtering, column sorting, and CSV export.
    /// Built entirely in code — no Designer file.
    /// </summary>
    public class OrderHistoryForm : Form
    {
        private readonly IOrderRepository _repo;
        private List<OrderRecord> _currentOrders = new List<OrderRecord>();

        // Sort state
        private int  _sortColumn    = 0;   // default: Date
        private bool _sortAscending = false; // default: newest first

        // UI controls
        private ListView         _listView;
        private TextBox          _txtSearch;
        private DateTimePicker   _dtpFrom;
        private DateTimePicker   _dtpTo;
        private CheckBox         _chkDateFilter;
        private Label            _lblResultCount;
        private Label            _lblStats;
        private Button           _btnDetails;
        private Button           _btnVoid;
        private Button           _btnDelete;
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
        // ── Design tokens ─────────────────────────────────────────────────────
        private static readonly Color _clrBackground  = Color.FromArgb(26,  26,  26);
        private static readonly Color _clrSurface     = Color.FromArgb(38,  38,  38);
        private static readonly Color _clrBrand       = Color.FromArgb(200, 60,   0);
        private static readonly Color _clrDanger      = Color.FromArgb(160, 30,  30);
        private static readonly Color _clrNeutral     = Color.FromArgb(55,  55,  55);
        private static readonly Color _clrTextPrimary = Color.FromArgb(240, 240, 240);
        private static readonly Color _clrTextMuted   = Color.FromArgb(160, 160, 160);

        private static void ApplyHistoryButtonStyle(Button btn, Color back, Color fore)
        {
            btn.FlatStyle                 = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor                 = back;
            btn.ForeColor                 = fore;
            btn.Font                      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Cursor                    = Cursors.Hand;
            btn.UseVisualStyleBackColor   = false;
        }

        private void BuildUi()
        {
            Text            = "Pizza Express — Order History";
            Size            = new Size(940, 600);
            MinimumSize     = new Size(700, 460);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            Font            = new Font("Segoe UI", 9.5f);
            BackColor       = _clrBackground;
            ForeColor       = _clrTextPrimary;
            KeyPreview      = true;   // form receives key events before child controls
            KeyDown        += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete) { DeleteSelectedOrder(); e.Handled = true; }
                if (e.KeyCode == Keys.Enter)  { ShowDetails();         e.Handled = true; }
                if (e.KeyCode == Keys.V)      { VoidSelectedOrder();   e.Handled = true; }
            };

            // ── Filter bar ────────────────────────────────────────────────────
            var filterPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 84,
                Padding   = new Padding(10, 8, 10, 4),
                BackColor = _clrSurface,
            };

            // Row 1: search box
            var lblSearch = new Label
            {
                Text      = "Search:",
                AutoSize  = true,
                Location  = new Point(10, 14),
                ForeColor = _clrTextPrimary,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            };
            _txtSearch = new TextBox
            {
                Location  = new Point(70, 11),
                Width     = 300,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = _clrTextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            _lblResultCount = new Label
            {
                AutoSize  = true,
                ForeColor = _clrTextMuted,
                Location  = new Point(380, 14),
            };

            // Row 2: date range
            _chkDateFilter = new CheckBox
            {
                Text      = "Date range:",
                Location  = new Point(10, 50),
                AutoSize  = true,
                Checked   = false,
                ForeColor = _clrTextPrimary,
            };
            _chkDateFilter.CheckedChanged += (s, e) =>
            {
                _dtpFrom.Enabled = _chkDateFilter.Checked;
                _dtpTo.Enabled   = _chkDateFilter.Checked;
                ApplyFilter();
            };

            _dtpFrom = new DateTimePicker
            {
                Location  = new Point(110, 48),
                Width     = 130,
                Format    = DateTimePickerFormat.Short,
                Value     = DateTime.Today.AddMonths(-1),
                Enabled   = false,
                CalendarForeColor = _clrTextPrimary,
            };
            _dtpFrom.ValueChanged += (s, e) => ApplyFilter();

            var lblTo = new Label
            {
                Text      = "to",
                AutoSize  = true,
                Location  = new Point(248, 52),
                ForeColor = _clrTextPrimary,
            };

            _dtpTo = new DateTimePicker
            {
                Location  = new Point(266, 48),
                Width     = 130,
                Format    = DateTimePickerFormat.Short,
                Value     = DateTime.Today,
                Enabled   = false,
                CalendarForeColor = _clrTextPrimary,
            };
            _dtpTo.ValueChanged += (s, e) => ApplyFilter();

            filterPanel.Controls.AddRange(new Control[]
            {
                lblSearch, _txtSearch, _lblResultCount,
                _chkDateFilter, _dtpFrom, lblTo, _dtpTo,
            });

            // ── Stats bar ─────────────────────────────────────────────────────
            var statsPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 32,
                Padding   = new Padding(10, 6, 10, 0),
                BackColor = Color.FromArgb(40, 20, 0),    // dark brand tint
            };
            _lblStats = new Label
            {
                AutoSize  = true,
                ForeColor = Color.FromArgb(255, 200, 140),  // warm amber text
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location  = new Point(10, 7),
            };
            statsPanel.Controls.Add(_lblStats);

            // ── Order list ────────────────────────────────────────────────────
            _listView = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
                BackColor     = Color.FromArgb(32, 32, 32),
                ForeColor     = _clrTextPrimary,
                BorderStyle   = BorderStyle.None,
            };
            _listView.Columns.Add("Date / Time ▼",  150);
            _listView.Columns.Add("Customer",       160);
            _listView.Columns.Add("Region",         110);
            _listView.Columns.Add("Payment",        110);
            _listView.Columns.Add("Total (NZD)",    100);
            _listView.Columns.Add("Status",          80);
            _listView.DoubleClick      += (s, e) => ShowDetails();
            _listView.ColumnClick      += ListView_ColumnClick;

            // Right-click context menu
            var ctxMenu   = new ContextMenuStrip();
            var ctxView   = new ToolStripMenuItem("View Details");
            var ctxVoid   = new ToolStripMenuItem("Void Order");
            var ctxDelete = new ToolStripMenuItem("Delete Order");
            ctxView.Click   += (s, e) => ShowDetails();
            ctxVoid.Click   += (s, e) => VoidSelectedOrder();
            ctxDelete.Click += (s, e) => DeleteSelectedOrder();
            ctxMenu.Items.Add(ctxView);
            ctxMenu.Items.Add(new ToolStripSeparator());
            ctxMenu.Items.Add(ctxVoid);
            ctxMenu.Items.Add(new ToolStripSeparator());
            ctxMenu.Items.Add(ctxDelete);
            _listView.ContextMenuStrip = ctxMenu;

            // ── Button bar ────────────────────────────────────────────────────
            _btnDetails = new Button { Text = "View Details", Width = 120, Height = 34 };
            _btnDetails.Click += (s, e) => ShowDetails();
            ApplyHistoryButtonStyle(_btnDetails, _clrBrand, Color.White);

            _btnVoid = new Button { Text = "Void Order", Width = 110, Height = 34 };
            _btnVoid.Click += (s, e) => VoidSelectedOrder();
            ApplyHistoryButtonStyle(_btnVoid, Color.FromArgb(140, 80, 0), Color.White);

            _btnDelete = new Button { Text = "Delete Order", Width = 120, Height = 34 };
            _btnDelete.Click += (s, e) => DeleteSelectedOrder();
            ApplyHistoryButtonStyle(_btnDelete, _clrDanger, Color.White);

            _btnExport = new Button { Text = "Export CSV", Width = 110, Height = 34 };
            _btnExport.Click += (s, e) => ExportCsv();
            ApplyHistoryButtonStyle(_btnExport, _clrNeutral, Color.White);

            _btnClose = new Button { Text = "Close", Width = 90, Height = 34 };
            _btnClose.Click += (s, e) => Close();
            ApplyHistoryButtonStyle(_btnClose, _clrNeutral, Color.White);

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(6),
                BackColor     = _clrSurface,
            };
            btnPanel.Controls.Add(_btnClose);
            btnPanel.Controls.Add(_btnDetails);
            btnPanel.Controls.Add(_btnVoid);
            btnPanel.Controls.Add(_btnDelete);
            btnPanel.Controls.Add(_btnExport);

            // ── Tooltips ──────────────────────────────────────────────────────
            var tip = new ToolTip { AutoPopDelay = 5000, InitialDelay = 400 };
            tip.SetToolTip(_lblStats,   "All-time totals across the entire database — unaffected by the current filter.");
            tip.SetToolTip(_btnVoid,    "Mark the selected order as Voided — keeps it in the log but excludes it from revenue (V)");
            tip.SetToolTip(_btnDelete,  "Permanently delete the selected order (Del)");
            tip.SetToolTip(_btnDetails, "Show full receipt for the selected order (Enter)");
            tip.SetToolTip(_btnExport,  "Save the currently visible rows to a CSV file");

            Controls.Add(_listView);
            Controls.Add(statsPanel);
            Controls.Add(filterPanel);
            Controls.Add(btnPanel);
        }

        // ── Column-header click sorting ───────────────────────────────────────
        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_sortColumn == e.Column)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn    = e.Column;
                _sortAscending = true;
            }

            // Update column header arrows
            for (int i = 0; i < _listView.Columns.Count; i++)
            {
                string baseName = _listView.Columns[i].Text
                    .Replace(" ▲", string.Empty)
                    .Replace(" ▼", string.Empty);
                _listView.Columns[i].Text = i == _sortColumn
                    ? baseName + (_sortAscending ? " ▲" : " ▼")
                    : baseName;
            }

            SortOrders();
            ApplyFilter();
        }

        private void SortOrders()
        {
            int dir = _sortAscending ? 1 : -1;
            _currentOrders.Sort((a, b) =>
            {
                switch (_sortColumn)
                {
                    case 0: return dir * a.OrderDate.CompareTo(b.OrderDate);
                    case 1: return dir * string.Compare(a.CustomerName,  b.CustomerName,  StringComparison.OrdinalIgnoreCase);
                    case 2: return dir * string.Compare(a.Region,        b.Region,        StringComparison.OrdinalIgnoreCase);
                    case 3: return dir * string.Compare(a.PaymentMethod, b.PaymentMethod, StringComparison.OrdinalIgnoreCase);
                    case 4: return dir * a.Total.CompareTo(b.Total);
                    default: return 0;
                }
            });
        }

        // ── Data loading ──────────────────────────────────────────────────────
        private void LoadOrders()
        {
            RefreshStats();
            ApplyFilter();
        }

        private void RefreshStats()
        {
            if (_lblStats == null) return;
            var summary = _repo.GetSummary();
            _lblStats.Text = summary.TotalOrders == 0
                ? "No orders yet."
                : $"All time:  {summary.TotalOrders} order{(summary.TotalOrders == 1 ? "" : "s")}  |  " +
                  $"Revenue: {summary.TotalRevenue.ToString("C2", new CultureInfo("en-NZ"))}  |  " +
                  $"Avg: {summary.AverageOrderValue.ToString("C2", new CultureInfo("en-NZ"))}";
        }

        // ── Filtering (SQL-backed) ────────────────────────────────────────────
        private void ApplyFilter()
        {
            string text    = (_txtSearch?.Text ?? string.Empty).Trim();
            bool   useDate = _chkDateFilter?.Checked ?? false;
            DateTime? from = useDate ? _dtpFrom?.Value.Date                    : (DateTime?)null;
            DateTime? to   = useDate ? _dtpTo?.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null;

            _currentOrders = _repo.Search(text, from, to);
            SortOrders();

            _listView.Items.Clear();

            foreach (OrderRecord r in _currentOrders)
            {
                var item = new ListViewItem(r.OrderDate.ToString("yyyy-MM-dd  HH:mm:ss"));
                item.SubItems.Add(r.CustomerName);
                item.SubItems.Add(r.Region);
                item.SubItems.Add(r.PaymentMethod);
                item.SubItems.Add(r.Total.ToString("C2", new CultureInfo("en-NZ")));
                item.SubItems.Add(r.Status ?? "Active");
                item.Tag = r;

                // Dim voided orders so they stand out without being removed
                bool isVoided = string.Equals(r.Status, "Voided", StringComparison.OrdinalIgnoreCase);
                if (isVoided)
                {
                    item.ForeColor = Color.FromArgb(100, 100, 100);
                    item.Font      = new Font("Segoe UI", 9f, FontStyle.Italic);
                }

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
            sb.AppendLine($"Status      : {record.Status ?? "Active"}");

            MessageBox.Show(sb.ToString(), "Order Details",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Delete order ──────────────────────────────────────────────────────
        private void DeleteSelectedOrder()
        {
            if (_listView.SelectedItems.Count == 0) return;

            var record = _listView.SelectedItems[0].Tag as OrderRecord;
            if (record == null) return;

            var confirm = MessageBox.Show(
                $"Permanently delete the order for {record.CustomerName} on {record.OrderDate:yyyy-MM-dd}?\n\nThis cannot be undone.",
                "Delete Order",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            _repo.Delete(record.Id);
            RefreshStats();
            ApplyFilter();
        }

        // ── Void order ────────────────────────────────────────────────────────
        private void VoidSelectedOrder()
        {
            if (_listView.SelectedItems.Count == 0) return;

            var record = _listView.SelectedItems[0].Tag as OrderRecord;
            if (record == null) return;

            if (string.Equals(record.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "This order is already voided.",
                    "Already Voided",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Void the order for {record.CustomerName} on {record.OrderDate:yyyy-MM-dd}?\n\n" +
                "The order will remain in the log but will be excluded from all revenue reports.",
                "Void Order",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            _repo.VoidOrder(record.Id);
            RefreshStats();
            ApplyFilter();
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
