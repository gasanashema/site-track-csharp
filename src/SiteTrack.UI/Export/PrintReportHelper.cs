using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace SiteTrack.UI.Export
{
    public class PrintReportHelper
    {
        private readonly string _reportTitle;
        private readonly string _projectName;
        private readonly string _dateRange;
        private readonly List<string> _headers;
        private readonly List<List<string>> _rows;
        private readonly List<int> _columnWidths;

        private int _currentRowIndex = 0;
        private int _pageNumber = 1;

        public PrintReportHelper(
            string reportTitle,
            string projectName,
            string dateRange,
            List<string> headers,
            List<List<string>> rows,
            List<int> columnWidths = null)
        {
            this._reportTitle = reportTitle ?? "SiteTrack Management Report";
            this._projectName = projectName ?? "All Active Projects";
            this._dateRange = dateRange ?? DateTime.Today.ToString("yyyy-MM-dd");
            this._headers = headers ?? new List<string>();
            this._rows = rows ?? new List<List<string>>();
            this._columnWidths = columnWidths;
        }

        public void ShowPrintPreview(IWin32Window owner)
        {
            _currentRowIndex = 0;
            _pageNumber = 1;

            PrintDocument doc = new PrintDocument();
            doc.DefaultPageSettings.Landscape = true;
            doc.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);
            doc.PrintPage += Doc_PrintPage;

            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = doc;
                preview.Width = 1000;
                preview.Height = 700;
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.ShowDialog(owner);
            }
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle bounds = e.MarginBounds;

            Font titleFont = new Font("Segoe UI", 14f, FontStyle.Bold);
            Font subFont = new Font("Segoe UI", 9f);
            Font headerFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            Font cellFont = new Font("Segoe UI", 8.5f);
            Font footerFont = new Font("Segoe UI", 8f, FontStyle.Italic);

            Brush textBrush = new SolidBrush(Color.FromArgb(15, 23, 42)); // Slate 900
            Brush subBrush = new SolidBrush(Color.FromArgb(100, 116, 139)); // Slate 500
            Brush headerBgBrush = new SolidBrush(Color.FromArgb(241, 245, 249)); // Slate 100
            Pen borderPen = new Pen(Color.FromArgb(226, 232, 240), 1);

            int y = bounds.Top;

            // 1. Report Header
            g.DrawString("SiteTrack Construction Manager", titleFont, textBrush, bounds.Left, y);
            y += 24;

            string subText = string.Format("Report: {0}  |  Project: {1}  |  Period: {2}  |  Generated: {3:yyyy-MM-dd HH:mm}",
                _reportTitle, _projectName, _dateRange, DateTime.Now);
            g.DrawString(subText, subFont, subBrush, bounds.Left, y);
            y += 24;

            g.DrawLine(new Pen(Color.FromArgb(148, 163, 184), 1.5f), bounds.Left, y, bounds.Right, y);
            y += 12;

            // Compute Column Widths if not provided
            int availableWidth = bounds.Width;
            int colCount = Math.Max(1, _headers.Count);
            int[] widths = new int[colCount];

            if (_columnWidths != null && _columnWidths.Count == colCount)
            {
                for (int i = 0; i < colCount; i++) widths[i] = _columnWidths[i];
            }
            else
            {
                int avgWidth = availableWidth / colCount;
                for (int i = 0; i < colCount; i++) widths[i] = avgWidth;
            }

            // 2. Table Column Headers
            int headerHeight = 26;
            g.FillRectangle(headerBgBrush, bounds.Left, y, bounds.Width, headerHeight);
            g.DrawRectangle(borderPen, bounds.Left, y, bounds.Width, headerHeight);

            int curX = bounds.Left;
            for (int i = 0; i < colCount; i++)
            {
                Rectangle headerRect = new Rectangle(curX + 4, y + 4, widths[i] - 8, headerHeight - 8);
                StringFormat sf = new StringFormat();
                sf.Trimming = StringTrimming.EllipsisCharacter;
                g.DrawString(_headers[i], headerFont, textBrush, headerRect, sf);
                curX += widths[i];
            }
            y += headerHeight;

            // 3. Rows
            int rowHeight = 22;
            while (_currentRowIndex < _rows.Count)
            {
                if (y + rowHeight > bounds.Bottom - 30)
                {
                    // Need next page
                    e.HasMorePages = true;
                    DrawFooter(g, bounds, footerFont, subBrush);
                    _pageNumber++;
                    return;
                }

                List<string> row = _rows[_currentRowIndex];

                if (_currentRowIndex % 2 == 1)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(248, 250, 252)), bounds.Left, y, bounds.Width, rowHeight);
                }
                g.DrawLine(borderPen, bounds.Left, y + rowHeight, bounds.Right, y + rowHeight);

                curX = bounds.Left;
                for (int i = 0; i < colCount; i++)
                {
                    string cellVal = (i < row.Count) ? row[i] : "";
                    Rectangle cellRect = new Rectangle(curX + 4, y + 3, widths[i] - 8, rowHeight - 6);
                    StringFormat sf = new StringFormat();
                    sf.Trimming = StringTrimming.EllipsisCharacter;
                    g.DrawString(cellVal, cellFont, textBrush, cellRect, sf);
                    curX += widths[i];
                }

                y += rowHeight;
                _currentRowIndex++;
            }

            // End of document
            e.HasMorePages = false;
            DrawFooter(g, bounds, footerFont, subBrush);
        }

        private void DrawFooter(Graphics g, Rectangle bounds, Font font, Brush brush)
        {
            string footerText = string.Format("Page {0}  —  Confidential AUCA DotNet Lab 3 Evaluation Record", _pageNumber);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            g.DrawString(footerText, font, brush, new Rectangle(bounds.Left, bounds.Bottom - 20, bounds.Width, 20), sf);
        }
    }
}

