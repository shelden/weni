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
        public static int SelectCount(IDbConnection dbConn, string table)
        {
 
            var command = dbConn.CreateCommand();
            var sql = new StringBuilder();
            sql.Append("SELECT COUNT(1) FROM ");
            sql.Append(table);
            command.CommandText = sql.ToString();
            var o = command.ExecuteScalar();

            // this is crap XXX RTFM how to get this scalar as int
            String s = o.ToString();
            return int.Parse(s);
        }

    }
}


#error error to be caught in jenkins
