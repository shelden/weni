using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Db
{
    public class Step
    {
        #region Constants
        public static readonly String TABLE = "workflow.steps";
        public static readonly int NO_NEXT_STEP = 0;
        public static readonly String INSERT = ""
                + "insert into "
                + TABLE
                + " (map_id, queue_id, name, next_step_id, type) "
                + "values (@map_id, @queue_id, @name, @next_step_id, @type) "
            ;
        public static readonly String SELECT_BY_NAME = ""
                + "select step_id "
                + "       , name " 
                + "       , map_id "
                + "       , queue_id "
                + "       , next_step_id "
                + "       , type "
                + " from "
                + TABLE + " "
                + "WHERE 0 = 0 "
                + "AND   name = @name "
                ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public String Name { get; set; }
        public int MapId { get; set; }
        public int QueueId { get; set; }
        public int NextStepId { get; set; }
        public int Type { get; set; } // XXX: Int16?
        #endregion

        #region Constructors
        public Step(int id
                , String name
                , int mapId
                , int queueId
                , int nextStepId
                , int type
            )
        {
            Id = id;
            Name = name;
            MapId = mapId;
            QueueId = queueId;
            NextStepId = nextStepId;
            Type = type;
        }
        #endregion

        #region CRUD: Insert
        public static Step Insert(IDbConnection dbConn
                                  , String name
                                  , Map map
                                  , Queue queue
                                  , int type
                                  )

        {
            return Insert(dbConn, name, map, queue, null, type);
        }

        public static Step Insert(IDbConnection dbConn
                             , String name
                             , Map map
                             , Queue queue
                             , Step nextStep
                             , int type
                             )

        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;
            DbUtil.AddParameter(command, "@name", name);
            DbUtil.AddParameter(command, "@map_id", map.Id);
            DbUtil.AddParameter(command, "@queue_id", queue.Id);
            if (nextStep == null)
            {
                DbUtil.AddNullParameter(command, "@next_step_id");
            }
            else{
                DbUtil.AddParameter(command, "@next_step_id", nextStep.Id);
            }
            DbUtil.AddParameter(command, "@type", type);
            int id = Convert.ToInt32(command.ExecuteScalar());
            Step tmp = new Step(id, name, map.Id, queue.Id, nextStep == null ? NO_NEXT_STEP : nextStep.Id, type);
            return tmp;
        }
        #endregion

        #region CRUD: Select
        public static Step Select(IDbConnection dbConn, String name)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_NAME;
                DbUtil.AddParameter(command, "@name", name);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                var tmp = new Step(DbUtil.GetInt(reader, "step_id")
                    , DbUtil.GetString(reader, "name")
                    , DbUtil.GetInt(reader, "map_id")
                    , DbUtil.GetInt(reader, "queue_id")
                    , NO_NEXT_STEP
                    , DbUtil.GetInt(reader, "type")
                    );
                if (!DbUtil.IsNull(reader, "next_step_id"))
                {
                    tmp.NextStepId = DbUtil.GetInt(reader, "next_step_id");
                }
                return tmp;
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
            sb.Append(this.GetType().FullName);
            sb.Append(' ');
            sb.Append(this.Id);
            sb.Append(", name=");
            sb.Append(this.Name);
            sb.Append(", next=");
            sb.Append(this.NextStepId);
            return sb.ToString();
        }
        #endregion
    }
}
