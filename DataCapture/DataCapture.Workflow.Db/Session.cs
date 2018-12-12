using System;
using System.Data;
using System.Text;

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
        public static readonly String SELECT_BY_ID = ""
            + "select session_id "
            + "   , user_id "
            + "   , start_time "
            + "   , host_name "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   session_id = @session_id "
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
        #endregion

        #region CRUD: Insert
        public static Session Insert(IDbConnection dbConn
                  , User user
                  )

        {
            DateTime when = DateTime.UtcNow;
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;
            DbUtil.AddParameter(command, "@user_id", user.Id);
            DbUtil.AddParameter(command, "@host_name", System.Environment.MachineName);
            DbUtil.AddParameter(command, "@start_time", when);
            int id = Convert.ToInt32(command.ExecuteScalar());
            return new Session(id, user.Id, System.Environment.MachineName, when);
        }
        #endregion

        #region CRUD: Select

        #endregion

        #region ToString()
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Id);
            sb.Append(", user_id=");
            sb.Append(this.Id);
            sb.Append(", from ");
            sb.Append(this.Hostname);
            sb.Append(", at ");
            sb.Append(this.StartTime);
            return sb.ToString();
        }
        #endregion

    }
}
