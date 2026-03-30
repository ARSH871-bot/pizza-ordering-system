using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3.Forms
{
    /// <summary>
    /// End-of-Day (Z-Report) form — shows a full shift summary for the current
    /// calendar day: order count, revenue, GST, average, payment-method breakdown,
    /// and top-selling items.  Printable with a single button click.
    ///
    /// Standard on every professional POS system; used by the cashier at close
    /// of shift to reconcile the till.
    /// </summary>
    public class EndOfDayForm : Form
    {
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

        private readonly IOrderRepository _repo;
        private readonly DateTime         _day;   // the day being reported

        // Report data (loaded once)
        private OrderSummary         _summary;
        private List<PaymentSplit>   _payments;
        private List<TopItem>        _topItems;

        // Cached plain-text version for printing
        private string _reportText;

        // ── Controls ──────────────────────────────────────────────────────────
        private Label _lblOrders;
        private Label _lblRevenue;
        private Label _lblGst;
        private Label _lblAvg;

        private ListView _lvPayments;
        private ListView _lvItems;

        public EndOfDayForm(IOrderRepository repo, DateTime? day = null)
        {
            _repo = repo ?? throw new ArgumentNullException("repo");
            _day  = (day ?? DateTime.Today).Date;
            BuildUi();
            LoadData();
        }

        // ── UI construction ───────────────────────────────────────────────────

        private void BuildUi()
        {
            Text            = $"Pizza Express — End of Day Report  ({_day:dddd d MMMM yyyy})";
            Size            = new Size(820, 640);
            MinimumSize     = new Size(680, 520);
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
                Height    = 56,
                BackColor = Color.FromArgb(40, 20, 0),
            };
            header.Controls.Add(new Label
            {
                Text     = "Z-Report  —  End of Day",
                Font     = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ClrAmber,
                AutoSize  = true,
                Location  = new Point(14, 8),
            });
            header.Controls.Add(new Label
            {
                Text      = _day.ToString("dddd, d MMMM yyyy"),
                Font      = new Font("Segoe UI", 10f),
                ForeColor = ClrMuted,
                AutoSize  = true,
                Location  = new Point(16, 34),
            });

            // ── KPI row ───────────────────────────────────────────────────────
            var kpiPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 82,
                BackColor = ClrBg,
                Padding   = new Padding(8, 8, 8, 4),
            };
            _lblOrders  = MakeKpiBox(kpiPanel, "ORDERS",     0);
            _lblRevenue = MakeKpiBox(kpiPanel, "REVENUE",    1);
            _lblGst     = MakeKpiBox(kpiPanel, "GST",        2);
            _lblAvg     = MakeKpiBox(kpiPanel, "AVG ORDER",  3);

            // ── Detail panels ─────────────────────────────────────────────────
            var detail = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = ClrBg,
                Padding     = new Padding(6),
            };
            detail.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            detail.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));

            _lvPayments = MakeList(new[] { "Payment Method", "Orders", "Revenue" },
                                   new[] { 140, 70, 110 });
            _lvItems    = MakeList(new[] { "Item",           "Qty",    "Revenue" },
                                   new[] { 210, 55, 100 });

            detail.Controls.Add(WrapInGroup("Payment Method Breakdown", _lvPayments), 0, 0);
            detail.Controls.Add(WrapInGroup("Top Items Today",          _lvItems),    1, 0);

            // ── Footer timestamp ──────────────────────────────────────────────
            var lblGenerated = new Label
            {
                Text      = $"Generated: {DateTime.Now:yyyy-MM-dd  HH:mm:ss}  —  Pizza Express NZ",
                Dock      = DockStyle.Bottom,
                Height    = 22,
                ForeColor = ClrMuted,
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ClrBg,
            };

            // ── Button bar ────────────────────────────────────────────────────
            var btnPrint = new Button { Text = "Print Report", Width = 120, Height = 34 };
            ApplyBtn(btnPrint, ClrBrand, Color.White);
            btnPrint.Click += (s, e) => PrintReport();

            var btnExport = new Button { Text = "Export CSV", Width = 110, Height = 34 };
            ApplyBtn(btnExport, ClrNeutral, ClrText);
            btnExport.Click += (s, e) => ExportCsv();

            var btnClose = new Button { Text = "Close", Width = 90, Height = 34 };
            ApplyBtn(btnClose, ClrNeutral, ClrText);
            btnClose.Click += (s, e) => Close();

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(6),
                BackColor     = ClrSurface,
            };
            btnPanel.Controls.Add(btnClose);
            btnPanel.Controls.Add(btnExport);
            btnPanel.Controls.Add(btnPrint);

            Controls.Add(detail);
            Controls.Add(kpiPanel);
            Controls.Add(header);
            Controls.Add(lblGenerated);
            Controls.Add(btnPanel);
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private void LoadData()
        {
            DateTime from = _day;
            DateTime to   = _day.AddDays(1).AddTicks(-1);

            _summary  = _repo.GetSummaryForPeriod(from, to);
            _payments = _repo.GetPaymentBreakdown(from, to);
            _topItems = _repo.GetTopItems(from, to, 15);

            // KPIs
            _lblOrders.Text  = _summary.TotalOrders.ToString();
            _lblRevenue.Text = _summary.TotalRevenue.ToString("C2", NZD);
            _lblGst.Text     = (_summary.TotalRevenue - _summary.TotalRevenue / 1.15m).ToString("C2", NZD);
            _lblAvg.Text     = _summary.AverageOrderValue.ToString("C2", NZD);

            // Payment breakdown
            foreach (var p in _payments)
            {
                var item = new ListViewItem(p.PaymentMethod ?? "Unknown");
                item.SubItems.Add(p.OrderCount.ToString());
                item.SubItems.Add(p.Revenue.ToString("C2", NZD));
                _lvPayments.Items.Add(item);
            }
            if (_payments.Count == 0)
                _lvPayments.Items.Add(new ListViewItem("No sales today"));

            // Top items
            foreach (var t in _topItems)
            {
                var item = new ListViewItem(t.Item.Trim());
                item.SubItems.Add(t.TotalQty > 0 ? t.TotalQty.ToString() : "1");
                item.SubItems.Add(t.TotalRevenue.ToString("C2", NZD));
                _lvItems.Items.Add(item);
            }
            if (_topItems.Count == 0)
                _lvItems.Items.Add(new ListViewItem("No items sold today"));

            // Build print text
            _reportText = BuildReportText();
        }

        // ── Print ─────────────────────────────────────────────────────────────

        private void PrintReport()
        {
            var pd = new PrintDocument();
            pd.DocumentName = $"Z-Report {_day:yyyy-MM-dd}";
            pd.PrintPage   += PrintPage;

            using (var preview = new PrintPreviewDialog
            {
                Document = pd,
                Width    = 700,
                Height   = 900,
                Text     = "Print Preview — End of Day Report",
            })
            {
                preview.ShowDialog(this);
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs ev)
        {
            var boldFont   = new Font("Courier New", 10f, FontStyle.Bold);
            var normalFont = new Font("Courier New",  9f);
            var smallFont  = new Font("Courier New",  8f);

            float x        = ev.MarginBounds.Left;
            float y        = ev.MarginBounds.Top;
            float pageH    = ev.MarginBounds.Bottom;
            float lineH    = normalFont.GetHeight(ev.Graphics) + 2f;
            float pageW    = ev.MarginBounds.Width;

            foreach (string line in _reportText.Split('\n'))
            {
                if (y + lineH > pageH) break;   // single page; truncate if needed

                Font   f = line.StartsWith("===") || line.StartsWith("---")
                         ? boldFont : normalFont;
                ev.Graphics.DrawString(line.TrimEnd('\r'), f, Brushes.Black, x, y);
                y += f.GetHeight(ev.Graphics) + 2f;
            }

            boldFont.Dispose();
            normalFont.Dispose();
            smallFont.Dispose();
            ev.HasMorePages = false;
        }

        // ── Export ────────────────────────────────────────────────────────────

        private void ExportCsv()
        {
            using (var dlg = new SaveFileDialog
            {
                Filter   = "CSV files (*.csv)|*.csv",
                FileName = $"ZReport_{_day:yyyyMMdd}.csv",
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine($"=== PIZZA EXPRESS — Z-REPORT ===");
                sb.AppendLine($"Date: {_day:yyyy-MM-dd}");
                sb.AppendLine($"Orders: {_lblOrders.Text},  Revenue: {_lblRevenue.Text},  GST: {_lblGst.Text},  Avg: {_lblAvg.Text}");
                sb.AppendLine();
                sb.AppendLine("PAYMENT METHOD,ORDERS,REVENUE");
                foreach (ListViewItem r in _lvPayments.Items)
                    if (r.SubItems.Count >= 3)
                        sb.AppendLine($"{r.Text},{r.SubItems[1].Text},{r.SubItems[2].Text}");
                sb.AppendLine();
                sb.AppendLine("ITEM,QTY,REVENUE");
                foreach (ListViewItem r in _lvItems.Items)
                    if (r.SubItems.Count >= 3)
                        sb.AppendLine($"\"{r.Text}\",{r.SubItems[1].Text},{r.SubItems[2].Text}");
                System.IO.File.WriteAllText(dlg.FileName, sb.ToString());
                MessageBox.Show("Z-Report exported.", "Exported",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ── Plain-text report builder (used for printing) ─────────────────────

        private string BuildReportText()
        {
            var sb = new StringBuilder();
            string sep = new string('=', 44);
            string line = new string('-', 44);

            sb.AppendLine(sep);
            sb.AppendLine(" PIZZA EXPRESS NZ — Z-REPORT");
            sb.AppendLine($" Date    : {_day:dddd d MMMM yyyy}");
            sb.AppendLine($" Printed : {DateTime.Now:yyyy-MM-dd  HH:mm:ss}");
            sb.AppendLine(sep);
            sb.AppendLine();
            sb.AppendLine($" {"Orders",-22} {_summary.TotalOrders,10}");
            sb.AppendLine($" {"Revenue (incl. GST)",-22} {_summary.TotalRevenue,10:C2}");

            decimal gst = _summary.TotalRevenue - _summary.TotalRevenue / 1.15m;
            sb.AppendLine($" {"GST (15%)",-22} {gst,10:C2}");
            sb.AppendLine($" {"Average Order",-22} {_summary.AverageOrderValue,10:C2}");
            sb.AppendLine();
            sb.AppendLine(line);
            sb.AppendLine(" PAYMENT BREAKDOWN");
            sb.AppendLine(line);
            foreach (var p in _payments)
                sb.AppendLine($" {(p.PaymentMethod ?? "Unknown"),-20} x{p.OrderCount,3}   {p.Revenue,8:C2}");
            if (_payments.Count == 0) sb.AppendLine(" (no sales)");

            sb.AppendLine();
            sb.AppendLine(line);
            sb.AppendLine(" TOP ITEMS");
            sb.AppendLine(line);
            foreach (var t in _topItems)
                sb.AppendLine($" {t.Item.Trim(),-28} x{(t.TotalQty > 0 ? t.TotalQty : 1),2}  {t.TotalRevenue,8:C2}");
            if (_topItems.Count == 0) sb.AppendLine(" (no items sold)");

            sb.AppendLine();
            sb.AppendLine(sep);
            return sb.ToString();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private Label MakeKpiBox(Panel parent, string caption, int index)
        {
            int w = 180;
            int x = 8 + index * (w + 8);
            var box = new Panel
            {
                Location  = new Point(x, 6),
                Size      = new Size(w, 64),
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
                Text      = "\u2014",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = ClrAmber,
                AutoSize  = true,
                Location  = new Point(10, 26),
            };
            box.Controls.Add(value);
            parent.Controls.Add(box);
            return value;
        }

        private ListView MakeList(string[] headers, int[] widths)
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
            for (int i = 0; i < headers.Length; i++)
                lv.Columns.Add(headers[i], widths[i]);
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

        private static void ApplyBtn(Button btn, Color back, Color fore)
        {
            btn.FlatStyle                 = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor                 = back;
            btn.ForeColor                 = fore;
            btn.Font                      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Cursor                    = Cursors.Hand;
            btn.UseVisualStyleBackColor   = false;
        }
    }
}
