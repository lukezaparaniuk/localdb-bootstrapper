using Dapper;
using LocalDBBootstrapper.Entities;
using LocalDBBootstrapper.Factories;
using LocalDBBootstrapper.Wrappers;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;

namespace LocalDBBootstrapper.Services
{
    public interface ILocalDbService
    {
        void Make(string instanceName);
        void AddFakeLinkedServer(string serverName);
        void BuildAndPublishProject(string projectPath, string databaseName);
    }

    /// <summary>
    /// Provides setup and tear down access to a localdb instance
    /// </summary>
    public class LocalDbService
    {
        // SqlLocalDb.exe CLI utility path
        public string ExecutablePath { get; private set; }
        // Where .mdf and .ldf SQL Server database files are created to
        public string InstancePath { get; private set; }
        public string InstanceName { get; private set; }

        private IFile _file = new FileWrapper();
        private IDirectory _directory = new DirectoryWrapper();
        private IDbConnectionFactory _connectionFactory = new DbConnectionFactory();
        private IProcessFactory _processFactory = new ProcessFactory();
        private IAppDomain _appDomain = new AppDomainWrapper();
        private int _deletionWaitMs = 1000;
        private bool _made = false;

        public LocalDbService(string executablePath, string instancePath)
        {
            ExecutablePath = executablePath;
            InstancePath = instancePath;
        }

        public LocalDbService(
            IFile file,
            IDirectory directory,
            IDbConnectionFactory connectionFactory,
            IProcessFactory processFactory,
            IAppDomain appDomain,
            string executablePath,
            string instancePath,
            int deletionWaitMs = default(int))
            : this(executablePath, instancePath)
        {
            _file = file;
            _directory = directory;
            _connectionFactory = connectionFactory;
            _processFactory = processFactory;
            _appDomain = appDomain;
            _deletionWaitMs = deletionWaitMs;
        }

        /// <summary>
        /// Creates a new localdb instance. If the instance already exists, it and all the databases it contains are
        /// deleted and the associated files on disk. To ensure this is successful, any running localdb processes are
        /// killed
        /// </summary>
        public void Make(string instanceName)
        {
            if (!_file.Exists(ExecutablePath))
            {
                throw new FileNotFoundException("LocalDB executable does not exist");
            }

            if (!_directory.Exists(InstancePath))
            {
                throw new DirectoryNotFoundException("LocalDB instance directory does not exist");
            }

            if (string.IsNullOrWhiteSpace(instanceName))
            {
                throw new ArgumentNullException("instanceName");
            }

            InstanceName = instanceName;

            if (CheckInstanceExists(InstanceName))
            {
                var databases = GetInstanceDatabases(InstanceName);

                if (databases != null && databases.Any())
                {
                    foreach (var database in databases)
                    {
                        DetachDatabase(InstanceName, database);
                    }
                }

                StopInstance(InstanceName);

                DeleteInstance(InstanceName);
            }

            if (CheckInstanceDirectoryExists(InstanceName))
            {
                KillProcesses();

                DeleteInstanceDirectory(InstanceName);
            }

            CreateInstance(InstanceName);

            _made = true;
        }

        /// <summary>
        /// Adds a faked linked server to the instance that doesn't do anything. This is useful if sprocs have
        /// dependencies on linked servers
        /// </summary>
        public void AddFakeLinkedServer(string serverName)
        {
            CheckIfMade();

            var connectionString = ConnectionString.New().Server(InstanceName).IntegratedSecurity().ToString();

            using (var connection = _connectionFactory.GetSqlConnection(@"Server=(localdb)\test;Integrated Security=true;"))
            {
                connection.Open();

                connection.Execute("EXEC master.dbo.sp_addlinkedserver @server = @Server, @srvproduct=N'', @provider=N'SQLNCLI'", new { Server = serverName });
            }
        }

        /// <summary>
        /// Builds and publishes a Visual Studio database project to the made localdb instance. A localdb log file is
        /// created in the instance directory with a file name of {databaseName}.msbuild.log. An exception is thrown if
        /// the build is not successful
        /// </summary>
        public void BuildAndPublishProject(string projectPath, string databaseName)
        {
            CheckIfMade();

            var connectionString = ConnectionString.New().Server(InstanceName).IntegratedSecurity().ToString();

            var profilePath = CreatePublishProfile(databaseName, connectionString);

            var projects = new ProjectCollection();

            var project = projects.LoadProject(projectPath);
            project.SetProperty("TargetConnectionString", connectionString);
            project.SetProperty("OutDir", _appDomain.CurrentDomainBaseDirectory);
            project.SetProperty("SqlPublishProfilePath", profilePath);

            var logger = new FileLogger();
            logger.Parameters = string.Format(@"logfile={0}.msbuild.log", databaseName);

            var result = project.Build(new[] { "Build", "Publish" }, new[] { logger });

            if (!result)
            {
                throw new InvalidOperationException("The project did not build successfully");
            }
        }

        /// <summary>
        /// Checks a localdb instance exists by trying to open a connection to it with a timeout of one second. This is
        /// enough to establish its existence
        /// </summary>
        protected bool CheckInstanceExists(string name)
        {
            var connectionString = ConnectionString.New().Server(name).IntegratedSecurity().Timeout(1).ToString();

            using (var connection = _connectionFactory.GetSqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    return true;
                }
                catch (DbException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a collection of database that exist on the localdb instance
        /// </summary>
        protected IEnumerable<string> GetInstanceDatabases(string name)
        {
            var connectionString = ConnectionString.New().Server(name).IntegratedSecurity().ToString();

            using (var connection = _connectionFactory.GetSqlConnection(connectionString))
            {
                connection.Open();

                return connection.Query<string>("SELECT name FROM sys.databases WHERE database_id > 4;");
            }
        }

        protected bool CheckInstanceDirectoryExists(string name)
        {
            var instanceDirectory = Path.Combine(InstancePath, name);

            return _directory.Exists(instanceDirectory);
        }

        /// <summary>
        /// Kills any running LocalDB sqlservr instances by querying the Win32_Process database. It checks that it is a
        /// LocalDB process rather than a regular SQL Server process by testing for the existence of '\LocalDB\' within
        /// the executable path.
        ///
        /// There is a Module property on the Process object which exposes the CommandLine, which can be used to get the
        /// path of the executable, but this blows up depending on the CPU architecture used, so we need to defend
        /// against this by querying WMI instead
        /// </summary>
        protected void KillProcesses()
        {
            var query = "SELECT ExecutablePath, ProcessID FROM Win32_Process WHERE Name = 'sqlservr.exe'";

            var searcher = new ManagementObjectSearcher(query);

            foreach (var item in searcher.Get())
            {
                var processId = item["ProcessID"];
                var processPath = item["ExecutablePath"];

                if (processPath != null && processPath.ToString().ToLower().Contains(@"\localdb\"))
                {
                    var process = Process.GetProcessById(int.Parse(processId.ToString()));

                    if (process != null)
                    {
                        process.Kill();
                    }
                }
            }
        }

        /// <summary>
        /// Checks the database exists on the instnace
        /// </summary>
        protected bool CheckDatabaseExists(string instanceName, string databaseName)
        {
            var connectionString = ConnectionString.New().Server(instanceName).IntegratedSecurity().ToString();

            using (var connection = _connectionFactory.GetSqlConnection(connectionString))
            {
                connection.Open();

                return connection.Query<bool>("IF (DB_ID( @Database ) IS NOT NULL) SELECT 1 ELSE SELECT 0;", new { Database = databaseName }).FirstOrDefault();
            }
        }

        /// <summary>
        /// Detaches the database on the instance. This takes it offline and prevents any issues with it being in use
        /// elsewhere
        /// </summary>
        protected void DetachDatabase(string instanceName, string databaseName)
        {
            var connectionString = ConnectionString.New().Server(instanceName).IntegratedSecurity().ToString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                connection.Execute(string.Format("ALTER DATABASE {0} SET OFFLINE WITH ROLLBACK IMMEDIATE;", databaseName));

                connection.Execute("EXECUTE sp_detach_db @Database ;", new { Database = databaseName });
            }
        }

        /// <summary>
        /// Deletes the localdb instance directory. If the directory is in use, it will try again ten times with a
        /// second delay between each iteration before throwing an exception
        /// </summary>
        protected void DeleteInstanceDirectory(string name)
        {
            var failureCount = 0;

            while (true)
            {
                try
                {
                    var instanceDirectory = Path.Combine(InstancePath, name);

                    _directory.Delete(instanceDirectory, true);

                    return;
                }
                catch (IOException)
                {
                    failureCount++;

                    if (failureCount > 10)
                    {
                        throw new TimeoutException();
                    }

                    Thread.Sleep(_deletionWaitMs);
                }
            }
        }

        /// <summary>
        /// Creates a new localdb instance using the SqlLocalDb.exe CLI utility. It creates the instance with the -s
        /// flag which specifies that the instance should be started after it has been created. An exception is thrown
        /// if the creation was not successful
        /// </summary>
        protected void CreateInstance(string name)
        {
            var startInfo = new ProcessStartInfoWrapper();
            startInfo.FileName = ExecutablePath;
            startInfo.Arguments = string.Format("create {0} -s", name);

            var process = _processFactory.GetProcess();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Failed to create LocalDB instance");
            }
        }

        /// <summary>
        /// Stops a localdb instance using the SqlLocalDb.exe CLI utility. The -k flag is used which kills the instance
        /// at the same time. An exception is thrown if the instance was not stopped
        /// </summary>
        protected void StopInstance(string name)
        {
            var startInfo = new ProcessStartInfoWrapper();
            startInfo.FileName = ExecutablePath;
            startInfo.Arguments = string.Format("stop {0} -k", name);

            var process = _processFactory.GetProcess();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Failed to stop LocalDB instance");
            }
        }

        /// <summary>
        /// Deletes a localdb instance using the SqlLocalDb.exe CLI utility. An exception is thrown if the instance was
        /// not deleted
        /// </summary>
        protected void DeleteInstance(string name)
        {
            var startInfo = new ProcessStartInfoWrapper();
            startInfo.FileName = ExecutablePath;
            startInfo.Arguments = string.Format("delete {0}", name);

            var process = _processFactory.GetProcess();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Failed to delete LocalDB instance");
            }
        }

        /// <summary>
        /// Creates a new database-specific publish profile from a template. The path of the newly created publish
        /// profile is return. An exception is thrown if the template does not exist
        /// </summary>
        protected string CreatePublishProfile(string databaseName, string connectionString)
        {
            var profileTemplatePath = Path.Combine(_appDomain.CurrentDomainBaseDirectory, "publish.xml");

            if (!_file.Exists(profileTemplatePath))
            {
                throw new FileNotFoundException("The publish profile template does not exist");
            }

            var text = _file.ReadAllText(profileTemplatePath)
                            .Replace("{databaseName}", databaseName)
                            .Replace("{scriptName}", databaseName + ".sql")
                            .Replace("{connectionString}", connectionString);

            var profilePath = Path.Combine(_appDomain.CurrentDomainBaseDirectory, databaseName + ".publish.xml");

            _file.WriteAllText(profilePath, text);

            return profilePath;
        }

        private void CheckIfMade()
        {
            if (!_made)
            {
                throw new InvalidOperationException("The instance has not been made");
            }
        }
    }
}