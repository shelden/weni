﻿using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Db
{
    public static class DbUtil

    {
        #region constants
        // this is the mysql way to get the autoincrement of last insert;
        // therefore using this is not portable
        public static readonly String GET_KEY = "SELECT LAST_INSERT_ID()";
        #endregion

        #region AddParameter 
        // these utility functions allows us to add a named parameter,
        // generically, to a IDbCommand.  There should be no mysql / oracle
        // SqlServer code in these functions
        public static void AddParameter(IDbCommand command, String name, String value)
        {
            var param = command.CreateParameter();
            param.DbType = System.Data.DbType.String;
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }
        public static void AddParameter(IDbCommand command, String name, int value)
        {
            var param = command.CreateParameter();
            param.DbType = System.Data.DbType.Int32;
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }
        public static void AddParameter(IDbCommand command, String name, DateTime value)
        {
            var param = command.CreateParameter();
            param.DbType = System.Data.DbType.DateTime;
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }
        public static void AddNullParameter(IDbCommand command, String name)
        {
            var param = command.CreateParameter();
            param.DbType = System.Data.DbType.Int32;
            param.ParameterName = name;
            param.Value = null;
            command.Parameters.Add(param);
        }
        #endregion

        #region Get Named Parameter
        public static String GetString(IDataReader reader, String name)
        {
            int index = reader.GetOrdinal(name);
            return reader.GetString(index);
        }
        public static int GetInt(IDataReader reader, String name)
        {
            int index = reader.GetOrdinal(name);
            return reader.GetInt32(index);
        }
        public static bool IsNull(IDataReader reader, String name)
        {
            int index = reader.GetOrdinal(name);
            return reader.IsDBNull(index);
        }
        #endregion

        #region SelectCount
        // Utility function (mostly for unit tests) to do a select count * from
        // table.  If you're writing a GUI, you should use parameters hanging
        // off your DataReader.
        public static int SelectCount(IDbConnection dbConn, string table)
        {
 
            var command = dbConn.CreateCommand();
            var sql = new StringBuilder();
            sql.Append("SELECT COUNT(1) FROM ");
            sql.Append(table);
            command.CommandText = sql.ToString();
            return Convert.ToInt32(command.ExecuteScalar());
        }
        #endregion

        #region Really
        // Less verbose utility methods to really close objects --
        // even if null -- which often happens in catch / finally blocks.

        public static void ReallyClose(IDataReader reader)
        {
            if (reader == null) return;
            try
            {
                reader.Close();
            }
            catch
            {
                ; // no code; per contract
            }
        }
        #endregion

    }
}
