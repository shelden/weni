using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Db
{
    public static class DbUtil
    {
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
        public static int SelectCount(IDbConnection dbConn, string table)
        {
 
            var command = dbConn.CreateCommand();
            var sql = new StringBuilder();
            sql.Append("SELECT COUNT(1) FROM ");
            sql.Append(table);
            command.CommandText = sql.ToString();
            var o = command.ExecuteScalar();

            // RTFM how to get this scalar as int more cleanly
            String s = o.ToString();
            return int.Parse(s);
        }
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

    }
}
