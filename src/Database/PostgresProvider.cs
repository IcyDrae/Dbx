using Dbx.Data;
using Dbx.Output;
using Npgsql;

namespace Dbx.Database
{
    public class PostgresProvider : IDatabaseProvider
    {
        private readonly DbConnection DbConnection;
        private NpgsqlConnection? PostgresConnection;

        public PostgresProvider(DbConnection DbConnection)
        {
            this.DbConnection = DbConnection;
        }

        public void Connect()
        {
            string connString =
                $"Host={DbConnection.Host};" +
                $"Port={DbConnection.Port};" +
                $"Database={DbConnection.Database};" +
                $"Username={DbConnection.Username};" +
                $"Password={DbConnection.Password}";

            PostgresConnection = new NpgsqlConnection(connString);
            PostgresConnection.Open();
        }

        public List<string> ListTables()
        {
            if (this.PostgresConnection == null || this.PostgresConnection.State != System.Data.ConnectionState.Open)
            {
                this.Connect();
            }

            List<string> Tables = new List<string>();
            using var Command = new NpgsqlCommand(
                "SELECT tablename FROM pg_tables WHERE schemaname='public';",
                this.PostgresConnection
            );
            using var Reader = Command.ExecuteReader();
            
            while (Reader.Read())
            {
                Tables.Add(Reader.GetString(0));
            }

            return Tables;
        }

        public string RunQuery(string Sql)
        {
            if (this.PostgresConnection == null || this.PostgresConnection.State != System.Data.ConnectionState.Open)
            {
                Connect();
            }

            using var Command = new NpgsqlCommand(Sql, this.PostgresConnection);
            var Result = Command.ExecuteScalar();

            return Result?.ToString() ?? "No result.";
        }

        public List<TableColumn> DescribeTable(string Name)
        {
            this.Connect();
            List<TableColumn> TableColumn = new List<TableColumn>();

            string Sql = @"
SELECT
    c.column_name,
    c.data_type,
    c.is_nullable,
    CASE
        WHEN tc.constraint_type = 'PRIMARY KEY' THEN 'PRI'
        WHEN tc.constraint_type = 'FOREIGN KEY' THEN 'FK'
        WHEN tc.constraint_type = 'UNIQUE' THEN 'UNI'
        ELSE ''
    END AS key_column,
    c.column_default
FROM information_schema.columns AS c
LEFT JOIN information_schema.key_column_usage AS kcu
       ON c.table_name = kcu.table_name
      AND c.column_name = kcu.column_name
LEFT JOIN information_schema.table_constraints AS tc
       ON kcu.constraint_name = tc.constraint_name
      AND kcu.table_name = tc.table_name
WHERE c.table_name = @Name
ORDER BY c.ordinal_position;
    ";

            NpgsqlCommand command = new NpgsqlCommand(Sql, this.PostgresConnection);
            command.Parameters.AddWithValue("@Name", Name);
            using var Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                TableColumn.Add(new TableColumn
                {
                    Name = Reader["column_name"]?.ToString() ?? "",
                    Type = Reader["data_type"]?.ToString() ?? "",
                    Nullable = Reader["is_nullable"]?.ToString() ?? "",
                    Key = Reader["key_column"].ToString() ?? "",
                    Default = Reader["column_default"]?.ToString() ?? "",
                });
            }

            return TableColumn;
        }

        public List<string> ListRows(string TableName, int Page = 1, int PageSize = 10, string? WhereClause = null)
        {
            if (this.PostgresConnection == null || this.PostgresConnection.State != System.Data.ConnectionState.Open)
            {
                this.Connect();
            }

            List<string> Rows = new List<string>();

            int Offset = (Page - 1) * PageSize;

            string Sql = $"SELECT * FROM {TableName}";

            if (!string.IsNullOrWhiteSpace(WhereClause))
            {
                Sql += " WHERE " + WhereClause;
            }

            Sql += " LIMIT @Limit OFFSET @Offset;";

            using NpgsqlCommand Command = new NpgsqlCommand(Sql, this.PostgresConnection);
            Command.Parameters.AddWithValue("@Limit", PageSize);
            Command.Parameters.AddWithValue("@Offset", Offset);

            using NpgsqlDataReader Reader = Command.ExecuteReader();
            
            while (Reader.Read())
            {
                List<string> RowValues = new List<string>();

                for (int i = 0; i < Reader.FieldCount; i++)
                {
                    RowValues.Add(Reader[i]?.ToString() ?? "");
                }

                Rows.Add(string.Join("  |  ", RowValues));
            }

            return Rows;
        }
    }
}

