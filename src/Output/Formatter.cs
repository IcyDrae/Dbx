using System.Text;

namespace Dbx.Output
{
    public class Formatter
    {
        public string FormatTable(List<TableColumn> columns, string TableName)
        {
            if (columns.Count == 0)
                return $"Table '{TableName}' has no columns.";

            string[] headers = { "Column", "Type", "Nullable", "Key", "Default" };
            var maxWidths = new Dictionary<string, int>
            {
                ["Column"] = headers[0].Length,
                ["Type"] = headers[1].Length,
                ["Nullable"] = headers[2].Length,
                ["Key"] = headers[3].Length,
                ["Default"] = headers[4].Length
                };

            // Compute max width per column
            foreach (var col in columns)
            {
                maxWidths["Column"] = Math.Max(maxWidths["Column"], col.Name.Length);
                maxWidths["Type"] = Math.Max(maxWidths["Type"], col.Type.Length);
                maxWidths["Nullable"] = Math.Max(maxWidths["Nullable"], col.Nullable.Length);
                maxWidths["Key"] = Math.Max(maxWidths["Key"], col.Key.Length);
                maxWidths["Default"] = Math.Max(maxWidths["Default"], col.Default.Length);
            }

            string BuildSeparator()
            {
                return "+" +
                    new string('-', maxWidths["Column"] + 2) + "+" +
                    new string('-', maxWidths["Type"] + 2) + "+" +
                    new string('-', maxWidths["Nullable"] + 2) + "+" +
                    new string('-', maxWidths["Key"] + 2) + "+" +
                    new string('-', maxWidths["Default"] + 2) + "+";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Table: {TableName}");
            sb.AppendLine(BuildSeparator());

            // Header row
            sb.AppendLine($"| {headers[0].PadRight(maxWidths["Column"])} | " +
                          $"{headers[1].PadRight(maxWidths["Type"])} | " +
                          $"{headers[2].PadRight(maxWidths["Nullable"])} | " +
                          $"{headers[3].PadRight(maxWidths["Key"])} | " +
                          $"{headers[4].PadRight(maxWidths["Default"])} |");
            sb.AppendLine(BuildSeparator());

            // Data rows
            foreach (var col in columns)
            {
                sb.AppendLine($"| {col.Name.PadRight(maxWidths["Column"])} | " +
                              $"{col.Type.PadRight(maxWidths["Type"])} | " +
                              $"{col.Nullable.PadRight(maxWidths["Nullable"])} | " +
                              $"{col.Key.PadRight(maxWidths["Key"])} | " +
                              $"{col.Default.PadRight(maxWidths["Default"])} |");
            }

            sb.AppendLine(BuildSeparator());
            return sb.ToString();
        }
    }
}

