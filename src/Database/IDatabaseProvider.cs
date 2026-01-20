using Dbx.Output;

namespace Dbx.Database
{
    public interface IDatabaseProvider
    {
        void Connect();
        List<string> ListTables();
        string RunQuery(string sql);
        List<TableColumn> DescribeTable(string Name);
        List<string> ListRows(string TableName, int Page, int PageSize, string? WhereClause);
        List<string> Query(string Query);
    }
}

