using System.Data.Common;
using System.Data.SqlClient;

namespace LocalDBBootstrapper.Factories
{
    public interface IDbConnectionFactory
    {
        DbConnection GetSqlConnection(string connectionString);
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        public DbConnection GetSqlConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
