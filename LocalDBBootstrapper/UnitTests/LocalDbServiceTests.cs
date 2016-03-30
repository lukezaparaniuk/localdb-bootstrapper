using LocalDBBootstrapper.Factories;
using LocalDBBootstrapper.Services;
using LocalDBBootstrapper.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data.Common;
using System.IO;

namespace LocalDBBootstrapper.UnitTests
{
    [TestClass]
    public class LocalDbServiceTests
    {
        Mock<IFile> _mockFile;
        Mock<IDirectory> _mockDirectory;
        Mock<IDbConnectionFactory> _mockConnectionFactory;
        Mock<IProcessFactory> _mockProcessFactory;
        Mock<IAppDomain> _mockAppDomain;
        LocalDbServiceWrapper _service;

        const string EXECUTABLE_PATH = "EXECUTABLE_PATH";
        const string INSTANCE_PATH = "INSTANCE_PATH";
        const string INSTANCE_NAME = "INSTANCE_NAME";

        [TestInitialize]
        public void TestInitialize()
        {
            _mockFile = new Mock<IFile>();
            _mockDirectory = new Mock<IDirectory>();
            _mockConnectionFactory = new Mock<IDbConnectionFactory>();
            _mockProcessFactory = new Mock<IProcessFactory>();
            _mockAppDomain = new Mock<IAppDomain>();

            _service = new LocalDbServiceWrapper(
                _mockFile.Object,
                _mockDirectory.Object,
                _mockConnectionFactory.Object,
                _mockProcessFactory.Object,
                _mockAppDomain.Object,
                EXECUTABLE_PATH,
                INSTANCE_PATH
            );
        }

        [TestMethod]
        public void Make_Fails_When_Executable_Path_Does_Not_Exist()
        {
            // arrange
            _mockFile.Setup(x => x.Exists(EXECUTABLE_PATH)).Returns(false);

            // act
            try
            {
                _service.Make(INSTANCE_NAME);

                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(FileNotFoundException));
            }
        }

        [TestMethod]
        public void Make_Fails_When_Instance_Path_Does_Not_Exist()
        {
            // arrange
            _mockFile.Setup(x => x.Exists(EXECUTABLE_PATH)).Returns(true);
            _mockDirectory.Setup(x => x.Exists(INSTANCE_PATH)).Returns(false);

            // act
            try
            {
                _service.Make(INSTANCE_NAME);

                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(DirectoryNotFoundException));
            }
        }

        [TestMethod]
        public void Make_Fails_When_Instance_Name_Is_Null()
        {
            // arrange
            _mockFile.Setup(x => x.Exists(EXECUTABLE_PATH)).Returns(true);
            _mockDirectory.Setup(x => x.Exists(INSTANCE_PATH)).Returns(true);

            // act
            try
            {
                _service.Make(null);

                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
            }
        }

        [TestMethod]
        public void Can_Check_Instance_Exists_Successfully_When_Instance_Exists()
        {
            // arrange
            var mockConnection = new Mock<DbConnection>();
            //mockConnection.Setup(x => x.Open()).Throws(new )
            _mockConnectionFactory.Setup(x => x.GetSqlConnection(It.IsAny<string>())).Returns(mockConnection.Object);

            // act
            var result = _service.CheckInstanceExists(INSTANCE_NAME);

            // assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Can_Check_Instance_Exists_Successfully_When_Instance_Does_Not_Exist()
        {
            // arrange
            var mockConnection = new Mock<DbConnection>();
            var mockDbException = new Mock<DbException>();
            mockConnection.Setup(x => x.Open()).Throws(mockDbException.Object);
            _mockConnectionFactory.Setup(x => x.GetSqlConnection(It.IsAny<string>())).Returns(mockConnection.Object);

            // act
            var result = _service.CheckInstanceExists(INSTANCE_NAME);

            // assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Can_Check_Instance_Directory_Exists_Successfully_When_Directory_Exists()
        {
            // arrange
            var expectedPath = INSTANCE_PATH + "\\" + INSTANCE_NAME;
            _mockDirectory.Setup(x => x.Exists(expectedPath)).Returns(true);

            // act
            var result = _service.CheckInstanceDirectoryExists(INSTANCE_NAME);

            // assert
            _mockDirectory.VerifyAll();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Can_Delete_Instance_Directory_Successfully_When_Not_In_Use()
        {
            // arrange
            var expectedPath = INSTANCE_PATH + "\\" + INSTANCE_NAME;
            _mockDirectory.Setup(x => x.Delete(expectedPath, It.IsAny<bool>()));

            // act
            _service.DeleteInstanceDirectory(INSTANCE_NAME);

            // assert
            _mockDirectory.VerifyAll();
        }

        [TestMethod]
        public void Can_Delete_Instance_Directory_Fails_When_In_Use()
        {
            // arrange
            var expectedPath = INSTANCE_PATH + "\\" + INSTANCE_NAME;
            _mockDirectory.Setup(x => x.Delete(expectedPath, It.IsAny<bool>())).Throws(new IOException());

            // act & assert
            try
            {
                _service.DeleteInstanceDirectory(INSTANCE_NAME);

                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                _mockDirectory.VerifyAll();
                Assert.IsInstanceOfType(ex, typeof(TimeoutException));
            }
        }

        [TestMethod]
        public void Can_Create_Instance_Successfully()
        {
            // arrange
            var mockProcess = new Mock<IProcess>();
            _mockProcessFactory.Setup(x => x.GetProcess()).Returns(mockProcess.Object);

            // act
            _service.CreateInstance(INSTANCE_NAME);

            // assert
            mockProcess.VerifySet(x => x.StartInfo);
            mockProcess.Verify(x => x.Start(), Times.Once);
            mockProcess.Verify(x => x.WaitForExit(), Times.Once);
        }

        [TestMethod]
        public void Can_Create_Instance_Fails()
        {
            // arrange
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(x => x.ExitCode).Returns(1);
            _mockProcessFactory.Setup(x => x.GetProcess()).Returns(mockProcess.Object);

            // act & assert
            try
            {
                _service.CreateInstance(INSTANCE_NAME);

                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.AreEqual("Failed to create LocalDB instance", ex.Message);
            }

            // assert
            mockProcess.VerifySet(x => x.StartInfo);
            mockProcess.Verify(x => x.Start(), Times.Once);
            mockProcess.Verify(x => x.WaitForExit(), Times.Once);
        }

        [TestMethod]
        public void Can_Stop_Instance_Successfully()
        {
            // arrange
            var mockProcess = new Mock<IProcess>();
            _mockProcessFactory.Setup(x => x.GetProcess()).Returns(mockProcess.Object);

            // act
            _service.StopInstance(INSTANCE_NAME);

            // assert
            mockProcess.VerifySet(x => x.StartInfo);
            mockProcess.Verify(x => x.Start(), Times.Once);
            mockProcess.Verify(x => x.WaitForExit(), Times.Once);
        }

        [TestMethod]
        public void Can_Stop_Instance_Fails()
        {
            // arrange
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(x => x.ExitCode).Returns(1);
            _mockProcessFactory.Setup(x => x.GetProcess()).Returns(mockProcess.Object);

            // act & assert
            try
            {
                _service.StopInstance(INSTANCE_NAME);

                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.AreEqual("Failed to stop LocalDB instance", ex.Message);
            }

            // assert
            mockProcess.VerifySet(x => x.StartInfo);
            mockProcess.Verify(x => x.Start(), Times.Once);
            mockProcess.Verify(x => x.WaitForExit(), Times.Once);
        }

        [TestMethod]
        public void Can_Delete_Instance_Successfully()
        {
            // arrange
            var mockProcess = new Mock<IProcess>();
            _mockProcessFactory.Setup(x => x.GetProcess()).Returns(mockProcess.Object);

            // act
            _service.DeleteInstance(INSTANCE_NAME);

            // assert
            mockProcess.VerifySet(x => x.StartInfo);
            mockProcess.Verify(x => x.Start(), Times.Once);
            mockProcess.Verify(x => x.WaitForExit(), Times.Once);
        }

        [TestMethod]
        public void Can_Delete_Instance_Fails()
        {
            // arrange
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(x => x.ExitCode).Returns(1);
            _mockProcessFactory.Setup(x => x.GetProcess()).Returns(mockProcess.Object);

            // act & assert
            try
            {
                _service.DeleteInstance(INSTANCE_NAME);

                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.AreEqual("Failed to delete LocalDB instance", ex.Message);
            }

            // assert
            mockProcess.VerifySet(x => x.StartInfo);
            mockProcess.Verify(x => x.Start(), Times.Once);
            mockProcess.Verify(x => x.WaitForExit(), Times.Once);
        }

        [TestMethod]
        public void Can_Create_Publish_Profile_Fails_When_Publish_Profile_Does_Not_Exist()
        {
            // arrange
            var databaseName = "foo";
            var connectionString = "bar";
            _mockAppDomain.Setup(x => x.CurrentDomainBaseDirectory).Returns("C:\\");

            _mockFile.Setup(x => x.Exists("C:\\publish.xml")).Returns(false);

            // act
            try
            {
                _service.CreatePublishProfile(databaseName, connectionString);

                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(FileNotFoundException));
                _mockFile.VerifyAll();
            }
        }

        [TestMethod]
        public void Can_Create_Publish_Profile_Successfully()
        {
            // arrange
            var databaseName = "foo";
            var connectionString = "bar";
            _mockAppDomain.Setup(x => x.CurrentDomainBaseDirectory).Returns("C:\\");

            _mockFile.Setup(x => x.Exists("C:\\publish.xml")).Returns(true);
            _mockFile.Setup(x => x.ReadAllText("C:\\publish.xml")).Returns(@"
                <?xml version='1.0' encoding='utf-8'?>
                <Project ToolsVersion='14.0' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
                    <PropertyGroup>
                        <IncludeCompositeObjects>True</IncludeCompositeObjects>
                        <TargetDatabaseName>{databaseName}</TargetDatabaseName>
                        <DeployScriptFileName>{scriptName}</DeployScriptFileName>
                        <TargetConnectionString>{connectionString}</TargetConnectionString>
                        <ProfileVersionNumber>1</ProfileVersionNumber>
                    </PropertyGroup>
                </Project>");

            // act
            var result = _service.CreatePublishProfile(databaseName, connectionString);

            // assert
            Assert.AreEqual("C:\\foo.publish.xml", result);
            _mockFile.VerifyAll();
            _mockFile.Verify(x => x.WriteAllText("C:\\foo.publish.xml", @"
                <?xml version='1.0' encoding='utf-8'?>
                <Project ToolsVersion='14.0' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
                    <PropertyGroup>
                        <IncludeCompositeObjects>True</IncludeCompositeObjects>
                        <TargetDatabaseName>foo</TargetDatabaseName>
                        <DeployScriptFileName>foo.sql</DeployScriptFileName>
                        <TargetConnectionString>bar</TargetConnectionString>
                        <ProfileVersionNumber>1</ProfileVersionNumber>
                    </PropertyGroup>
                </Project>"));
        }

        [TestMethod]
        public void Can_Build_And_Publish_Profile_Fails_When_Instance_Has_Not_Been_Made()
        {
            // act & assert
            try
            {
                _service.BuildAndPublishProject(null, null);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.AreEqual("The instance has not been made", ex.Message);
            }
        }

        [TestMethod]
        public void Can_Add_Fake_Linked_Server_Fails_When_Instance_Has_Not_Been_Made()
        {
            // act & assert
            try
            {
                _service.AddFakeLinkedServer(null);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
                Assert.AreEqual("The instance has not been made", ex.Message);
            }
        }
    }

    public class LocalDbServiceWrapper : LocalDbService
    {
        public LocalDbServiceWrapper(
            IFile file,
            IDirectory directory,
            IDbConnectionFactory connectionFactory,
            IProcessFactory processFactory,
            IAppDomain appDomain,
            string executablePath,
            string instancePath
        ) : base(file, directory, connectionFactory, processFactory, appDomain, executablePath, instancePath)
        {
        }

        public new bool CheckInstanceExists(string name)
        {
            return base.CheckInstanceExists(name);
        }

        public new bool CheckInstanceDirectoryExists(string name)
        {
            return base.CheckInstanceDirectoryExists(name);
        }

        public new void DeleteInstanceDirectory(string name)
        {
            base.DeleteInstanceDirectory(name);
        }

        public new void CreateInstance(string name)
        {
            base.CreateInstance(name);
        }

        public new void StopInstance(string name)
        {
            base.StopInstance(name);
        }

        public new void DeleteInstance(string name)
        {
            base.DeleteInstance(name);
        }

        public new string CreatePublishProfile(string databaseName, string connectionString)
        {
            return base.CreatePublishProfile(databaseName, connectionString);
        }
    }
}
