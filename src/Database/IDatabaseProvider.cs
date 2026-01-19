using Dbx.Output;

namespace Dbx.Database
{
    public interface IDatabaseProvider
    {
        void Connect();
        List<string> ListTables();
        string RunQuery(string sql);
        List<TableColumn> DescribeTable(string Name);
    }
}

