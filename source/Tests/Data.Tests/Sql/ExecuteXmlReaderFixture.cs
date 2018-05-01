﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Transactions;
using System.Xml;
using EnterpriseLibrary.Data.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnterpriseLibrary.Data.Sql.Tests
{
    [TestClass]
    public class ExecuteXmlReaderFixture
    {
        static SqlDatabase sqlDatabase;

        [TestInitialize]
        public void SetUp()
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory(TestConfigurationSource.CreateConfigurationSource());
            sqlDatabase = (SqlDatabase)factory.CreateDefault();
        }

        [TestMethod]
        public void CanExecuteXmlQueryWithDataAndSchema()
        {
            string knownGoodOutput = "<Schema name=\"Schema1\" xmlns=\"urn:schemas-microsoft-com:xml-data\" " +
                                     "xmlns:dt=\"urn:schemas-microsoft-com:datatypes\"><ElementType name=\"Region\" content=\"empty\" " +
                                     "model=\"closed\"><AttributeType name=\"RegionID\" dt:type=\"i4\" />" +
                                     "<AttributeType name=\"RegionDescription\" dt:type=\"string\" /><attribute type=\"RegionID\" />" +
                                     "<attribute type=\"RegionDescription\" /></ElementType></Schema>" +
                                     "<Region xmlns=\"x-schema:#Schema1\" RegionID=\"1\" RegionDescription=\"Eastern                                           \" />" +
                                     "<Region xmlns=\"x-schema:#Schema1\" RegionID=\"2\" RegionDescription=\"Western                                           \" />" +
                                     "<Region xmlns=\"x-schema:#Schema1\" RegionID=\"3\" RegionDescription=\"Northern                                          \" />" +
                                     "<Region xmlns=\"x-schema:#Schema1\" RegionID=\"4\" RegionDescription=\"Southern                                          \" />";

            string queryString = "Select * from Region for xml auto, xmldata";
            SqlCommand sqlCommand = sqlDatabase.GetSqlStringCommand(queryString) as SqlCommand;

            string actualOutput = RetrieveXmlFromDatabase(sqlCommand);

            Assert.AreEqual(ConnectionState.Closed, sqlCommand.Connection.State);
            Assert.AreEqual(knownGoodOutput, actualOutput);
        }

        [TestMethod]
        public void CanExecuteXmlQueryRetrivingDataOnly()
        {
            string knownGoodOutput =
                "<Region RegionID=\"1\" RegionDescription=\"Eastern                                           \" />" +
                "<Region RegionID=\"2\" RegionDescription=\"Western                                           \" />" +
                "<Region RegionID=\"3\" RegionDescription=\"Northern                                          \" />" +
                "<Region RegionID=\"4\" RegionDescription=\"Southern                                          \" />";

            string queryString = "Select * from Region for xml auto";
            SqlCommand sqlCommand = sqlDatabase.GetSqlStringCommand(queryString) as SqlCommand;

            string actualOutput = RetrieveXmlFromDatabase(sqlCommand);

            Assert.AreEqual(ConnectionState.Closed, sqlCommand.Connection.State);
            Assert.AreEqual(knownGoodOutput, actualOutput);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void InvalidSqlStringThrowsWhenRetrievingXml()
        {
            string queryString = "Select * from Foo for xml auto";
            SqlCommand sqlCommand = sqlDatabase.GetSqlStringCommand(queryString) as SqlCommand;

            string actualOutput = RetrieveXmlFromDatabase(sqlCommand);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteXmlReaderWithWrongCommandTypeWillThrow()
        {
            sqlDatabase.ExecuteXmlReader(new OdbcCommand());
        }

        [TestMethod]
        public void CanExecuteXmlQueryThroughTransaction()
        {
            string knownGoodOutputAfterChange =
                "<Region RegionID=\"1\" RegionDescription=\"Eastern                                           \" />" +
                "<Region RegionID=\"2\" RegionDescription=\"Western                                           \" />" +
                "<Region RegionID=\"3\" RegionDescription=\"Northern                                          \" />" +
                "<Region RegionID=\"4\" RegionDescription=\"Southern                                          \" />" +
                "<Region RegionID=\"99\" RegionDescription=\"Midwest                                           \" />";

            string knownGoodOutputAfterRollback =
                "<Region RegionID=\"1\" RegionDescription=\"Eastern                                           \" />" +
                "<Region RegionID=\"2\" RegionDescription=\"Western                                           \" />" +
                "<Region RegionID=\"3\" RegionDescription=\"Northern                                          \" />" +
                "<Region RegionID=\"4\" RegionDescription=\"Southern                                          \" />";

            string insertString = "insert into region values (99, 'Midwest')";
            DbCommand insertCommand = sqlDatabase.GetSqlStringCommand(insertString);

            string queryString = "Select * from Region for xml auto";
            SqlCommand sqlCommand = sqlDatabase.GetSqlStringCommand(queryString) as SqlCommand;

            string actualOutput = "";

            using (DbConnection connection = sqlDatabase.CreateConnection())
            {
                connection.Open();
                using (RollbackTransactionWrapper transaction = new RollbackTransactionWrapper(connection.BeginTransaction()))
                {
                    sqlDatabase.ExecuteNonQuery(insertCommand, transaction.Transaction);

                    XmlReader results = sqlDatabase.ExecuteXmlReader(sqlCommand, transaction.Transaction);
                    results.MoveToContent();
                    for (string value = results.ReadOuterXml(); value != null && value.Length != 0; value = results.ReadOuterXml())
                    {
                        actualOutput += value;
                    }
                    results.Close();
                }
            }

            Assert.AreEqual(actualOutput, knownGoodOutputAfterChange);

            string confirmationString = "Select * from Region for xml auto";
            SqlCommand confirmationCommand = sqlDatabase.GetSqlStringCommand(confirmationString) as SqlCommand;

            string rollbackResults = RetrieveXmlFromDatabase(confirmationCommand);
            Assert.AreEqual(knownGoodOutputAfterRollback, rollbackResults);
        }

        string RetrieveXmlFromDatabase(SqlCommand sqlCommand)
        {
            string actualOutput = "";

            XmlReader reader = null;
            try
            {
                reader = sqlDatabase.ExecuteXmlReader(sqlCommand);
                reader.MoveToContent();
                for (string value = reader.ReadOuterXml(); value != null && value.Length != 0; value = reader.ReadOuterXml())
                {
                    actualOutput += value;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                sqlCommand.Connection.Close();
            }

            return actualOutput;
        }

        [TestMethod]
        public void ClosingAnXmlReaderClosesTheUnderlyingConnection_Bug2751()
        {
            var database = new SqlDatabase(String.Format(@"server={0};database=Northwind;Integrated Security=true", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]));

            var command = new SqlCommand { CommandText = "Select * from Region for xml auto", CommandType = CommandType.Text };

            using (var reader = database.ExecuteXmlReader(command))
            {
                Assert.AreNotEqual(ConnectionState.Closed, command.Connection.State);
            }
            Assert.AreEqual(ConnectionState.Closed, command.Connection.State);
        }

        [TestMethod]
        public void ClosingAnXmlReaderExecutedWithATrasactionDoesNotCloseTheUnderlyingConnection_Bug2751()
        {
            var database = new SqlDatabase(String.Format(@"server={0};database=Northwind;Integrated Security=true", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]));

            using (var connection = database.CreateConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                var command = new SqlCommand { CommandText = "Select * from Region for xml auto", CommandType = CommandType.Text };

                using (var reader = database.ExecuteXmlReader(command, transaction))
                {
                    Assert.AreNotEqual(ConnectionState.Closed, command.Connection.State);
                }
                Assert.AreNotEqual(ConnectionState.Closed, command.Connection.State);
            }
        }

        [TestMethod]
        public void ClosingAnXmlReaderExecutedWhileInATrasactionScopeDoesNotCloseTheUnderlyingConnection_Bug2751()
        {
            var database = new SqlDatabase(String.Format(@"server={0};database=Northwind;Integrated Security=true", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]));

            var command = new SqlCommand { CommandText = "Select * from Region for xml auto", CommandType = CommandType.Text };

            using (new TransactionScope())
            {
                using (var reader = database.ExecuteXmlReader(command))
                {
                    Assert.AreNotEqual(ConnectionState.Closed, command.Connection.State);
                }
                Assert.AreNotEqual(ConnectionState.Closed, command.Connection.State);
            }
            Assert.AreEqual(ConnectionState.Closed, command.Connection.State);
        }
    }
}
