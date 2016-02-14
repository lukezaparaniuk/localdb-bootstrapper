namespace LocalDBBootstrapper.Entities
{
    public class ConnectionString
    {
        public string Value { get; set; }

        public ConnectionString()
        {

        }

        public ConnectionString(string value)
        {
            Value = Value;
        }

        public override string ToString()
        {
            return Value;
        }

        public void Append(string value)
        {
            Value = Value + value;
        }

        public static ConnectionString Make
        {
            get
            {
                return new ConnectionString();
            }
        }
    }

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
