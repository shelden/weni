using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Db
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
        public static readonly String SELECT_BY_ID = ""
            + "select allowed_id "
            + "   , user_id "
            + "   , queue_id "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   allowed_id = @id "
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
        #endregion

        #region CRUD: Insert
        public static AllowedQueue Insert(IDbConnection dbConn
            , User user
            , Queue queue
            )
        {
            int type = 1; // XXX should be enum
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
        public static AllowedQueue Select(IDbConnection dbConn, String name)
        {
            IDataReader reader = null;


            try
            {
                return null;
                /*
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_NAME;
                DbUtil.AddParameter(command, "@name", name); 
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new Queue(reader.GetInt32(0) // XXX bad practice to use indexes.  Use names.
                    , reader.GetString(1)
                    , reader.GetInt32(2)
                    );
                    */
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
