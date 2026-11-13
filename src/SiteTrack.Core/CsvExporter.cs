using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SiteTrack.Core
{
    public static class CsvExporter
    {
        public static string FormatCsv(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
        {
            if (headers == null) throw new ArgumentNullException("headers");
            if (rows == null) throw new ArgumentNullException("rows");

            StringBuilder sb = new StringBuilder();

            // 1. Headers
            bool firstHeader = true;
            foreach (string h in headers)
            {
                if (!firstHeader) sb.Append(",");
                sb.Append(EscapeCell(h));
                firstHeader = false;
            }
            sb.AppendLine();

            // 2. Rows
            foreach (IEnumerable<string> row in rows)
            {
                bool firstCell = true;
                foreach (string cell in row)
                {
                    if (!firstCell) sb.Append(",");
                    sb.Append(EscapeCell(cell));
                    firstCell = false;
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static void SaveToFile(string filePath, string csvContent)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if (csvContent == null) csvContent = string.Empty;

            // Write with UTF-8 BOM so Microsoft Excel cleanly preserves accented characters & symbols
            Encoding utf8WithBom = new UTF8Encoding(true);
            File.WriteAllText(filePath, csvContent, utf8WithBom);
        }

        public static string EscapeCell(string cell)
        {
            if (cell == null) return string.Empty;

            bool containsSpecial = cell.Contains(",") ||
                                  cell.Contains("\"") ||
                                  cell.Contains("\r") ||
                                  cell.Contains("\n");

            if (containsSpecial)
            {
                string escaped = cell.Replace("\"", "\"\"");
                return string.Format("\"{0}\"", escaped);
            }

            return cell;
        }
    }
}

