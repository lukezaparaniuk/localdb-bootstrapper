using System;

namespace LocalDBBootstrapper.Entities
{
    /// <summary>
    /// Represents a connection string. It wraps the value held internally because we need an entity to add extension
    /// methods to to give us fluent connection string creation
    /// </summary>
    public class ConnectionString
    {
        private string _value;

        public ConnectionString()
        {

        }

        public ConnectionString(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }

        public void Append(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException("value");
            }

            _value = _value + value;
        }

        public static ConnectionString New()
        {
            return new ConnectionString();
        }

        public static ConnectionString New(string value)
        {
            return new ConnectionString(value);
        }
    }

    // Extensions methods that allow us to affect the underlying connection string using fluent syntax
    public static class ConnectionStringExtensions
    {
        public static ConnectionString Server(this ConnectionString connectionString, string instanceName)
        {
            connectionString.Append(string.Format(@"Server=(localdb)\{0};", instanceName));

            return connectionString;
        }

        public static ConnectionString IntegratedSecurity(this ConnectionString connectionString)
        {
            connectionString.Append("Integrated Security=true;");

            return connectionString;
        }

        public static ConnectionString Timeout(this ConnectionString connectionString, int seconds)
        {
            connectionString.Append(string.Format("Connection Timeout={0};", seconds));

            return connectionString;
        }

        public static ConnectionString Database(this ConnectionString connectionString, string databaseName)
        {
            connectionString.Append(string.Format("Database={0};", databaseName));

            return connectionString;
        }
    }
}
