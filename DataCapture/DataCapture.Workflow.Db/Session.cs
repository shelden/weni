using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace DataCapture.Workflow.Db
{
    public class Session
    {
        #region Constants
        public static readonly String TABLE = "workflow.sessions";
        public static readonly String INSERT = ""
            + "insert into "
            + TABLE
            + " (user_id, host_name, start_time) "
            + "values (@user_id, @host_name, @start_time) "
            ;
        public static readonly String SELECT_BY_USER_ID = ""
            + "select session_id "
            + "   , user_id "
            + "   , start_time "
            + "   , host_name "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   user_id = @user_id "
            ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public int UserId { get; set; }
        public String Hostname { get; set; }
        public DateTime StartTime { get; set; }
        #endregion

        #region Constructors
        public Session (int id
            , int userId
            , String hostname
            , DateTime startTime
            )
        {
            Id = id;
            UserId = userId;
            Hostname = hostname;
            StartTime = startTime;
        }
        public Session(IDataReader reader)
            : this(DbUtil.GetInt(reader, "session_id")
                  , DbUtil.GetInt(reader, "user_id")
                  , DbUtil.GetString(reader, "host_name")
                  , DbUtil.GetDateTime(reader, "start_time")
                  )
        { /* no code */ }
        #endregion

        #region CRUD: Insert
        public static Session Insert(IDbConnection dbConn
                  , User user
                  )

        {
            DateTime when = DateTime.UtcNow;
            String hostname = System.Environment.MachineName.ToLower(); // and maybe strip domains etc?
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;
            DbUtil.AddParameter(command, "@user_id", user.Id);
            DbUtil.AddParameter(command, "@host_name", hostname);
            DbUtil.AddParameter(command, "@start_time", when);
            int id = Convert.ToInt32(command.ExecuteScalar());
            return new Session(id, user.Id, hostname, when);
        }
        #endregion

        #region CRUD: Select
        public static IList<Session> SelectAll(IDbConnection dbConn
            , User user
            )
        {
            IDataReader reader = null;
            IList<Session> tmp = new List<Session>();
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_USER_ID;
                DbUtil.AddParameter(command, "@user_id", user.Id);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                while(reader.Read())
                {
                    tmp.Add(new Session(reader));
                }
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
            return tmp;
        }
        #endregion

        #region ToString()
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            sb.Append(' ');
            sb.Append(this.Id);
            sb.Append(", user_id=");
            sb.Append(this.Id);
            sb.Append(", from ");
            sb.Append(this.Hostname);
            sb.Append(", at ");
            sb.Append(this.StartTime.ToString(DbUtil.FORMAT));
            sb.Append(" UTC");
            return sb.ToString();
        }
        #endregion

    }
}
