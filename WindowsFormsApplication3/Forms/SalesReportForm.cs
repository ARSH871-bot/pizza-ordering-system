using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3.Forms
{
    /// <summary>
    /// Period sales report — shows summary KPIs, daily order breakdown,
    /// top-selling items, and payment-method split for any date range.
    /// Fully dark-themed, matches the rest of the application.
    /// </summary>
    public class SalesReportForm : Form
    {
        private readonly IOrderRepository _repo;

        // Controls
        private DateTimePicker _dtpFrom;
        private DateTimePicker _dtpTo;
        private Button         _btnToday;
        private Button         _btnWeek;
        private Button         _btnMonth;
        private Button         _btnRun;
        private Button         _btnExport;
        private Button         _btnClose;

        // KPI panels
        private Label _lblOrders;
        private Label _lblRevenue;
        private Label _lblGst;
        private Label _lblAvg;

        // Detail grids
        private ListView _lvDaily;
        private ListView _lvItems;
        private ListView _lvPayments;

        // ── Design tokens ─────────────────────────────────────────────────────
        private static readonly Color ClrBg      = Color.FromArgb(26,  26,  26);
        private static readonly Color ClrSurface = Color.FromArgb(38,  38,  38);
        private static readonly Color ClrBrand   = Color.FromArgb(200, 60,   0);
        private static readonly Color ClrNeutral = Color.FromArgb(55,  55,  55);
        private static readonly Color ClrText    = Color.FromArgb(240, 240, 240);
        private static readonly Color ClrMuted   = Color.FromArgb(160, 160, 160);
        private static readonly Color ClrAmber   = Color.FromArgb(255, 200, 140);
        private static readonly Color ClrGreen   = Color.FromArgb(100, 200, 100);

        private static readonly CultureInfo NZD = new CultureInfo("en-NZ");

        public SalesReportForm(IOrderRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException("repo");
            BuildUi();
            // Default: current month
            _dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _dtpTo.Value   = DateTime.Today;
            RunReport();
        }

        // ── UI construction ───────────────────────────────────────────────────

        private void BuildUi()
        {
            Text            = "Pizza Express — Sales Report";
            Size            = new Size(1020, 680);
            MinimumSize     = new Size(820, 560);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            BackColor       = ClrBg;
            ForeColor       = ClrText;
            Font            = new Font("Segoe UI", 9.5f);

            // ── Header ────────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 50,
                BackColor = Color.FromArgb(40, 20, 0),
            };
            header.Controls.Add(new Label
            {
                Text      = "Sales Report",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ClrAmber,
                AutoSize  = true,
                Location  = new Point(14, 12),
            });

            // ── Filter bar ────────────────────────────────────────────────────
            var filterPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 46,
                BackColor = ClrSurface,
                Padding   = new Padding(8, 6, 8, 6),
            };

            var lblFrom = MakeLabel("From:", new Point(8, 14));
            _dtpFrom = new DateTimePicker
            {
                Location = new Point(55, 11),
                Width    = 120,
                Format   = DateTimePickerFormat.Short,
            };
            var lblTo = MakeLabel("To:", new Point(184, 14));
            _dtpTo = new DateTimePicker
            {
                Location = new Point(208, 11),
                Width    = 120,
                Format   = DateTimePickerFormat.Short,
            };

            _btnToday  = MakeSmallButton("Today",       new Point(340, 10));
            _btnWeek   = MakeSmallButton("This Week",   new Point(400, 10));
            _btnMonth  = MakeSmallButton("This Month",  new Point(476, 10));
            _btnRun    = MakeSmallButton("Run Report",  new Point(562, 10), ClrBrand);

            _btnToday.Click += (s, e) => { _dtpFrom.Value = DateTime.Today; _dtpTo.Value = DateTime.Today; RunReport(); };
            _btnWeek.Click  += (s, e) =>
            {
                int dow = (int)DateTime.Today.DayOfWeek;
                _dtpFrom.Value = DateTime.Today.AddDays(-dow);
                _dtpTo.Value   = DateTime.Today;
                RunReport();
            };
            _btnMonth.Click += (s, e) =>
            {
                _dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                _dtpTo.Value   = DateTime.Today;
                RunReport();
            };
            _btnRun.Click += (s, e) => RunReport();

            filterPanel.Controls.AddRange(new Control[]
            {
                lblFrom, _dtpFrom, lblTo, _dtpTo,
                _btnToday, _btnWeek, _btnMonth, _btnRun,
            });

            // ── KPI row ───────────────────────────────────────────────────────
            var kpiPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 80,
                BackColor = ClrBg,
                Padding   = new Padding(8, 8, 8, 4),
            };
            _lblOrders  = MakeKpiBox(kpiPanel, "ORDERS",  0);
            _lblRevenue = MakeKpiBox(kpiPanel, "REVENUE", 1);
            _lblGst     = MakeKpiBox(kpiPanel, "GST",     2);
            _lblAvg     = MakeKpiBox(kpiPanel, "AVG ORDER", 3);

            // ── Three detail ListViews ─────────────────────────────────────────
            var detailPanel = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 3,
                RowCount    = 1,
                BackColor   = ClrBg,
                Padding     = new Padding(6),
            };
            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            _lvDaily    = MakeDetailList(new[] { "Date", "Orders", "Revenue", "GST" },
                                         new[] { 110, 65, 100, 85 });
            _lvItems    = MakeDetailList(new[] { "Item", "Qty", "Revenue" },
                                         new[] { 200, 55, 100 });
            _lvPayments = MakeDetailList(new[] { "Method", "Orders", "Revenue" },
                                         new[] { 120, 70, 100 });

            detailPanel.Controls.Add(WrapInGroup("Daily Breakdown",    _lvDaily),    0, 0);
            detailPanel.Controls.Add(WrapInGroup("Top Items",          _lvItems),    1, 0);
            detailPanel.Controls.Add(WrapInGroup("Payment Methods",    _lvPayments), 2, 0);

            // ── Button bar ────────────────────────────────────────────────────
            _btnExport = new Button { Text = "Export CSV", Width = 110, Height = 34 };
            _btnExport.Click += (s, e) => ExportCsv();
            ApplyBtnStyle(_btnExport, ClrNeutral, ClrText);

            _btnClose = new Button { Text = "Close", Width = 90, Height = 34 };
            _btnClose.Click += (s, e) => Close();
            ApplyBtnStyle(_btnClose, ClrNeutral, ClrText);

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(6),
                BackColor     = ClrSurface,
            };
            btnPanel.Controls.Add(_btnClose);
            btnPanel.Controls.Add(_btnExport);

            Controls.Add(detailPanel);
            Controls.Add(kpiPanel);
            Controls.Add(filterPanel);
            Controls.Add(header);
            Controls.Add(btnPanel);
        }

        // ── Report execution ──────────────────────────────────────────────────

        private void RunReport()
        {
            DateTime from = _dtpFrom.Value.Date;
            DateTime to   = _dtpTo.Value.Date;
            if (from > to) { from = to; _dtpFrom.Value = to; }

            // KPIs
            var summary = _repo.GetSummaryForPeriod(from, to.AddDays(1).AddTicks(-1));
            _lblOrders.Text  = summary.TotalOrders.ToString();
            _lblRevenue.Text = summary.TotalRevenue.ToString("C2", NZD);
            _lblGst.Text     = (summary.TotalRevenue - summary.TotalRevenue / 1.15m)
                                    .ToString("C2", NZD);
            _lblAvg.Text     = summary.AverageOrderValue.ToString("C2", NZD);

            // Daily breakdown
            _lvDaily.Items.Clear();
            List<DailySummary> daily = _repo.GetDailySummaries(from, to);
            foreach (var d in daily)
            {
                var item = new ListViewItem(d.Day.ToString("yyyy-MM-dd"));
                item.SubItems.Add(d.OrderCount.ToString());
                item.SubItems.Add(d.Revenue.ToString("C2", NZD));
                item.SubItems.Add(d.Gst.ToString("C2", NZD));
                _lvDaily.Items.Add(item);
            }

            // Top items
            _lvItems.Items.Clear();
            List<TopItem> items = _repo.GetTopItems(from, to.AddDays(1).AddTicks(-1), 20);
            foreach (var t in items)
            {
                var item = new ListViewItem(t.Item.Trim());
                item.SubItems.Add(t.TotalQty > 0 ? t.TotalQty.ToString() : "1");
                item.SubItems.Add(t.TotalRevenue.ToString("C2", NZD));
                _lvItems.Items.Add(item);
            }

            // Payment breakdown
            _lvPayments.Items.Clear();
            List<PaymentSplit> payments = _repo.GetPaymentBreakdown(from, to.AddDays(1).AddTicks(-1));
            foreach (var p in payments)
            {
                var item = new ListViewItem(p.PaymentMethod ?? "Unknown");
                item.SubItems.Add(p.OrderCount.ToString());
                item.SubItems.Add(p.Revenue.ToString("C2", NZD));
                _lvPayments.Items.Add(item);
            }
        }

        // ── Export ────────────────────────────────────────────────────────────

        private void ExportCsv()
        {
            using (var dlg = new SaveFileDialog
            {
                Filter   = "CSV files (*.csv)|*.csv",
                FileName = $"SalesReport_{_dtpFrom.Value:yyyyMMdd}_{_dtpTo.Value:yyyyMMdd}.csv",
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("=== PIZZA EXPRESS — SALES REPORT ===");
                sb.AppendLine($"Period: {_dtpFrom.Value:yyyy-MM-dd} to {_dtpTo.Value:yyyy-MM-dd}");
                sb.AppendLine($"Orders: {_lblOrders.Text},  Revenue: {_lblRevenue.Text},  GST: {_lblGst.Text},  Avg: {_lblAvg.Text}");
                sb.AppendLine();
                sb.AppendLine("DATE,ORDERS,REVENUE,GST");
                foreach (ListViewItem r in _lvDaily.Items)
                    sb.AppendLine($"{r.Text},{r.SubItems[1].Text},{r.SubItems[2].Text},{r.SubItems[3].Text}");
                sb.AppendLine();
                sb.AppendLine("ITEM,QTY,REVENUE");
                foreach (ListViewItem r in _lvItems.Items)
                    sb.AppendLine($"\"{r.Text}\",{r.SubItems[1].Text},{r.SubItems[2].Text}");
                sb.AppendLine();
                sb.AppendLine("PAYMENT METHOD,ORDERS,REVENUE");
                foreach (ListViewItem r in _lvPayments.Items)
                    sb.AppendLine($"{r.Text},{r.SubItems[1].Text},{r.SubItems[2].Text}");
                System.IO.File.WriteAllText(dlg.FileName, sb.ToString());
                MessageBox.Show("Report exported successfully.", "Exported",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private Label MakeKpiBox(Panel parent, string caption, int index)
        {
            int w = 180;
            int x = 8 + index * (w + 8);
            var box = new Panel
            {
                Location  = new Point(x, 6),
                Size      = new Size(w, 62),
                BackColor = ClrSurface,
            };
            box.Controls.Add(new Label
            {
                Text      = caption,
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = ClrMuted,
                AutoSize  = true,
                Location  = new Point(10, 8),
            });
            var value = new Label
            {
                Text      = "—",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = ClrAmber,
                AutoSize  = true,
                Location  = new Point(10, 26),
            };
            box.Controls.Add(value);
            parent.Controls.Add(box);
            return value;
        }

        private ListView MakeDetailList(string[] headers, int[] widths)
        {
            var lv = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
                BackColor     = Color.FromArgb(32, 32, 32),
                ForeColor     = ClrText,
                BorderStyle   = BorderStyle.None,
                Font          = new Font("Segoe UI", 9f),
            };
            lv.HeaderStyle = ColumnHeaderStyle.Clickable;
            for (int i = 0; i < headers.Length; i++)
                lv.Columns.Add(headers[i], widths[i]);

            lv.OwnerDraw = false;
            return lv;
        }

        private Panel WrapInGroup(string title, ListView lv)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = ClrBg, Padding = new Padding(4) };
            var lbl = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ClrAmber,
                Dock      = DockStyle.Top,
                Height    = 22,
            };
            lv.Dock = DockStyle.Fill;
            p.Controls.Add(lv);
            p.Controls.Add(lbl);
            return p;
        }

        private Label MakeLabel(string text, Point loc)
            => new Label
            {
                Text      = text,
                AutoSize  = true,
                Location  = loc,
                ForeColor = ClrText,
            };

        private Button MakeSmallButton(string text, Point loc, Color? back = null)
        {
            var btn = new Button
            {
                Text     = text,
                Location = loc,
                Size     = new Size(text.Length < 7 ? 52 : text.Length < 10 ? 76 : 96, 26),
            };
            ApplyBtnStyle(btn, back ?? ClrNeutral, ClrText);
            return btn;
        }

        private static void ApplyBtnStyle(Button btn, Color back, Color fore)
        {
            btn.FlatStyle                 = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor                 = back;
            btn.ForeColor                 = fore;
            btn.Font                      = new Font("Segoe UI", 9f, FontStyle.Bold);
            btn.Cursor                    = Cursors.Hand;
            btn.UseVisualStyleBackColor   = false;
        }
    }
}
