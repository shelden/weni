using System;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace DataCapture.Workflow.Db
{
    /// <summary>
    /// This class creates a DB connection to the supported databases.  In here
    /// should be the only DB-specific code.
    /// 
    /// 1st pass uses mySQL only.
    /// </summary>
    public static class ConnectionFactory
    {
        #region Utility
        private static void Append(StringBuilder sb, String key, String value)
        {
            sb.Append(key);
            sb.Append('=');
            sb.Append(value);
            sb.Append(';');
        }
        #endregion

        #region Create()
        public static IDbConnection Create()
        {
            StringBuilder sb = new StringBuilder();
            Append(sb, "SERVER", "localhost");
            Append(sb, "DATABASE", "workflow");
            Append(sb, "UID", "workflow");
            Append(sb, "PASSWORD", "");
            var tmp = new MySqlConnection(sb.ToString());
            tmp.Open();
            return tmp;
        }
        #endregion
    }
}


#error error to be caught in jenkins
