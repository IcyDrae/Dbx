
namespace Dbx.Database
{
    public interface IDatabaseProvider
    {
        void Connect();
        List<string> ListTables();
        string RunQuery(string sql);
    }
}

