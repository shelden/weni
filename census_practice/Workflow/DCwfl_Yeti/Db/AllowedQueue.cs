using System;
using System.Data;
using System.Text;

namespace LM.DataCapture.Workflow.Yeti.Db
{
    public class AllowedQueue
    {
        #region Constants
        public static readonly String TABLE = "workflow.allowed_queues";
        public static readonly String INSERT = ""
            + "insert into "
            + TABLE
            + " (user_id, queue_id) "
            + "values (@user_id, @queue_id) "
            ;
        public static readonly String SELECT_BY_USER_QUEUE_ID = ""
            + "select allowed_id "
            + "   , user_id "
            + "   , queue_id "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   user_id = @user_id "
            + "AND   queue_id = @queue_id "
            ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public int UserId { get; set; }
        public int QueueId { get; set; }
        #endregion

        #region Constructors
        public AllowedQueue(int id
            , int userId
            , int queueId
            )
        {
            Id = id;
            UserId = userId;
            QueueId = queueId;
        }
        public AllowedQueue(IDataReader reader)
            : this(DbUtil.GetInt(reader, "allowed_id")
                  , DbUtil.GetInt(reader, "user_id")
                  , DbUtil.GetInt(reader, "queue_id")
                  )
        { /* no code */ }
        #endregion

        #region CRUD: Insert
        public static AllowedQueue Insert(IDbConnection dbConn
            , User user
            , Queue queue
            )
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;
            DbUtil.AddParameter(command, "@queue_id", queue.Id);
            DbUtil.AddParameter(command, "@user_id", user.Id);

            int id = Convert.ToInt32(command.ExecuteScalar());
            AllowedQueue tmp = new AllowedQueue(id, user.Id, queue.Id);
            return tmp;
        }
        #endregion

        #region CRUD: Select
        public static AllowedQueue Select(IDbConnection dbConn, User user, Queue queue)
        {
            IDataReader reader = null;

            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_USER_QUEUE_ID;
                DbUtil.AddParameter(command, "@queue_id", queue.Id);
                DbUtil.AddParameter(command, "@user_id", user.Id);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new AllowedQueue(reader);
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }

        }
        #endregion

        #region ToString()
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Id);
            sb.Append(", queue=");
            sb.Append(this.QueueId);
            sb.Append(", user=");
            sb.Append(this.UserId);
            return sb.ToString();
        }
        #endregion

    }
}
