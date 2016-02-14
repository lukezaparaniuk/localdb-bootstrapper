using Dapper;
using LocalDBBootstrapper.Entities;
using LocalDBBootstrapper.Wrappers;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LocalDBBootstrapper
{
    public interface ILocalDbService
    {

    }

    public class LocalDBService
    {
        private readonly IFile _file;
        private readonly IDirectory _directory;

        private bool _made = false;

        public string ExecutablePath { get; private set; }
        public string InstancePath { get; private set; }
        public string Instance { get; private set; }

        private LocalDBService()
        {
            _file = FileWrapper.Instance;
            _directory = DirectoryWrapper.Instance;
        }

        public LocalDBService(string executablePath, string instancePath) : this()
        {
            if (!_file.Exists(executablePath))
            {
                throw new FileNotFoundException("LocalDB executable does not exist");
            }

            ExecutablePath = executablePath;

            if (!_directory.Exists(instancePath))
            {
                throw new DirectoryNotFoundException("LocalDB instance directory does not exist");
            }

            InstancePath = instancePath;
        }

        public void Make(string instanceName)
        {
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                throw new ArgumentNullException("instanceName");
            }

            Instance = instanceName;

            if (CheckInstanceExists(Instance))
            {
                var databases = GetInstanceDatabases(Instance);

                if (databases != null && databases.Any())
                {
                    foreach (var database in databases)
                    {
                        DetachDatabase(Instance, database);
                    }
                }

                StopInstance(Instance);

                DeleteInstance(Instance);
            }

            if (CheckInstanceDirectoryExists(Instance))
            {
                DeleteInstanceDirectory(Instance);
            }

            CreateInstance(Instance);

            _made = true;
        }

        public void AddFakeLinkedServer(string serverName)
        {
            if (!_made)
            {
                throw new InvalidOperationException("The instance has not been made");
            }

            var connectionString = ConnectionString.Make.Server(Instance).IntegratedSecurity().ToString();

            using (var connection = new SqlConnection(@"Server=(localdb)\test;Integrated Security=true;"))
            {
                connection.Open();

                connection.Execute("EXEC master.dbo.sp_addlinkedserver @server = @Server, @srvproduct=N'', @provider=N'SQLNCLI'", new { Server = serverName });
            }
        }

        protected bool CheckInstanceExists(string name)
        {
            var connectionString = ConnectionString.Make.Server(name).IntegratedSecurity().Timeout(1).ToString();

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        protected IEnumerable<string> GetInstanceDatabases(string name)
        {
            var connectionString = ConnectionString.Make.Server(name).IntegratedSecurity().ToString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return connection.Query<string>("SELECT name FROM sys.databases WHERE database_id > 4;");
            }
        }

        protected bool CheckInstanceDirectoryExists(string name)
        {
            return Directory.Exists(Path.Combine(InstancePath, name));
        }

        protected bool CheckDatabaseExists(string instanceName, string databaseName)
        {
            var connectionString = ConnectionString.Make.Server(instanceName).IntegratedSecurity().ToString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return connection.Query<bool>("IF (DB_ID( @Database ) IS NOT NULL) SELECT 1 ELSE SELECT 0;", new { Database = databaseName }).FirstOrDefault();
            }
        }

        protected void DetachDatabase(string instanceName, string databaseName)
        {
            var connectionString = ConnectionString.Make.Server(instanceName).IntegratedSecurity().ToString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                connection.Execute(string.Format("ALTER DATABASE {0} SET OFFLINE WITH ROLLBACK IMMEDIATE;", databaseName));

                connection.Execute("EXECUTE sp_detach_db @Database ;", new { Database = databaseName });
            }
        }

        protected void DeleteInstanceDirectory(string name)
        {
            Directory.Delete(Path.Combine(InstancePath, name), true);
        }

        protected void CreateInstance(string name)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = ExecutablePath;
            startInfo.Arguments = string.Format("create {0} -s", name);

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Failed to create LocalDB instance");
            }
        }

        protected void StopInstance(string name)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = ExecutablePath;
            startInfo.Arguments = string.Format("stop {0} -k", name);

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Failed to stop LocalDB instance");
            }
        }

        protected void DeleteInstance(string name)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = ExecutablePath;
            startInfo.Arguments = string.Format("delete {0}", name);

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Failed to delete LocalDB instance");
            }
        }

        public void BuildAndPublishProject(string projectPath, string databaseName)
        {
            var connectionString = ConnectionString.Make.Server(Instance).IntegratedSecurity().ToString();

            var profilePath = CreatePublishProfile(databaseName, connectionString);

            var projects = new ProjectCollection();

            var project = projects.LoadProject(projectPath);
            project.SetProperty("TargetConnectionString", connectionString);
            project.SetProperty("OutDir", AppDomain.CurrentDomain.BaseDirectory);
            project.SetProperty("SqlPublishProfilePath", profilePath);

            var logger = new FileLogger();
            logger.Parameters = string.Format(@"logfile={0}.msbuild.log", databaseName);

            var result = project.Build(new[] { "Build", "Publish" }, new[] { logger });
        }

        public string CreatePublishProfile(string databaseName, string connectionString)
        {
            var profileTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "publish.xml");

            if (!File.Exists(profileTemplatePath))
            {
                throw new FileNotFoundException("The publish profile template does not exist");
            }

            var text = File.ReadAllText(profileTemplatePath)
                           .Replace("{databaseName}", databaseName)
                           .Replace("{scriptName}", databaseName + ".sql")
                           .Replace("{connectionString}", connectionString);

            var profilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseName + ".publish.xml");

            File.WriteAllText(profilePath, text);

            return profilePath;
        }
    }
}