using LocalDBBootstrapper.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LocalDBBootstrapper.UnitTests
{
    [TestClass]
    public class ConnectionStringTests
    {
        [TestMethod]
        public void Can_Initialise_Successfully_When_Has_No_Initial_Value()
        {
            // act
            var connectionString = ConnectionString.New();

            // assert
            Assert.IsNull(connectionString.ToString());
        }

        [TestMethod]
        public void Can_Initialise_Successfully_When_Has_Initial_Value()
        {
            // act
            var connectionString = ConnectionString.New("example");

            // assert
            Assert.AreEqual("example", connectionString.ToString());
        }

        [TestMethod]
        public void Can_Set_Server_Successfully()
        {
            // act
            var connectionString = ConnectionString.New().Server("server");

            // assert
            Assert.AreEqual(@"Server=(localdb)\server;", connectionString.ToString());
        }

        [TestMethod]
        public void Can_Set_Integrated_Security_Successfully_When_True()
        {
            // act
            var connectionString = ConnectionString.New().IntegratedSecurity();

            // assert
            Assert.AreEqual(@"Integrated Security=true;", connectionString.ToString());
        }

        [TestMethod]
        public void Can_Set_Integrated_Security_Successfully_When_False()
        {
            // act
            var connectionString = ConnectionString.New("example");

            // assert
            Assert.IsFalse(connectionString.ToString().Contains("Integrated Security=true;"));
        }

        [TestMethod]
        public void Can_Set_Timeout_Successfully()
        {
            // act
            var connectionString = ConnectionString.New().Timeout(1);

            // assert
            Assert.AreEqual(@"Connection Timeout=1;", connectionString.ToString());
        }

        [TestMethod]
        public void Can_Set_Database_Successfully()
        {
            // act
            var connectionString = ConnectionString.New().Database("database");

            // assert
            Assert.AreEqual(@"Database=database;", connectionString.ToString());
        }

        [TestMethod]
        public void Can_Set_Entire_Connection_String_Successfully()
        {
            // arrange
            var expectedConnectionString = @"Server=(localdb)\server;Integrated Security=true;Connection Timeout=1;Database=database;";

            // act
            var connectionString = ConnectionString.New().Server("server").IntegratedSecurity().Timeout(1).Database("database");

            // assert
            Assert.AreEqual(expectedConnectionString, connectionString.ToString());
        }
    }
}
