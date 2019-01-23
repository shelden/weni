using System;
using System.Data;
using System.Text;

namespace LM.DataCapture.Workflow.Yeti.Db
{
    public static class DbUtil
    {
        #region constants
        // this is the mysql way to get the autoincrement of last insert;
        // therefore using this is not portable :-/ XXX
        public static readonly String GET_KEY = "SELECT LAST_INSERT_ID()";
        public static readonly String FORMAT = "yyyy-MM-dd HH:mm:ss.fff";
        #endregion

        #region AddParameter 
        // these utility functions allows us to add a named parameter,
        // generically, to a IDbCommand.  There should be no mysql / oracle
        // SqlServer / MariaDB code in these functions
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
            var copy = new DateTime(value.Year
                , value.Month
                , value.Day
                , value.Hour
                , value.Minute
                , value.Second
                , value.Millisecond // rounds down to Millisecond precision; which is all we have in the DB
                , value.Kind
                );

            var param = command.CreateParameter();
            param.DbType = System.Data.DbType.DateTime;
            param.ParameterName = name;
            param.Value = copy;
            command.Parameters.Add(param);
        }
        public static void AddParameter(IDbCommand command, String name, bool value)
        {
            var param = command.CreateParameter();
            param.DbType = System.Data.DbType.Int16;
            param.ParameterName = name;
            param.Value = value ? 1 : 0;
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
        public static bool GetBool(IDataReader reader, String name)
        {
            int index = reader.GetOrdinal(name);
            int tmp = reader.GetInt32(index);
            return (tmp != 0);
        }
        public static DateTime GetDateTime(IDataReader reader, String name)
        {
            int index = reader.GetOrdinal(name);
            return reader.GetDateTime(index);
        }
        public static bool IsNull(IDataReader reader, String name)
        {
            int index = reader.GetOrdinal(name);
            return reader.IsDBNull(index);
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
        public static void ReallyClose(IDbConnection dbConn)
        {
            if (dbConn == null) return;
            try
            {
                dbConn.Close();
            }
            catch
            {
                ; // no code; per contract
            }
        }
        public static void ReallyClose(IDbTransaction transaction)
        {
            DbUtil.ReallyBackout(transaction);
        }
        public static void ReallyBackout(IDbTransaction transaction)
        {
            if (transaction == null) return;
            try
            {
                transaction.Rollback();
            }
            catch
            {
                ; // no code; per contract
            }
        }
        #endregion

    }
}
