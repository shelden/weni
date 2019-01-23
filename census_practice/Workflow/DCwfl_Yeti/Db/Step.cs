using System;
using System.Data;
using System.Text;

namespace LM.DataCapture.Workflow.Yeti.Db
{
    public class Step
    {
        #region enums
        public enum StepType
        {
            Standard = 1
            , Start = 2
            , Terminating = 4
            , Failure = 8
        }
        #endregion

        #region Constants
        public static readonly String TABLE = "workflow.steps";
        public static readonly int NO_NEXT_STEP = -1;
        private static readonly String INSERT = ""
            + "INSERT INTO "
            + TABLE
            + " ("
            + "   map_id "
            + "   , queue_id "
            + "   , name "
            + "   , next_step_id "
            + "   , type "
            + ") VALUES ("
            + "    @map_id "
            + "    , @queue_id "
            + "    , @name "
            + "    , @next_step_id "
            + "    , @type "
            + ") "
            ;
        private static readonly String SELECT_BASE = ""
                + "select step_id "
                + "       , name " 
                + "       , map_id "
                + "       , queue_id "
                + "       , next_step_id "
                + "       , type "
                + "FROM "
                + TABLE + " "
                + "WHERE 0 = 0 "
                ;
        private static readonly String SELECT_BY_NAME = SELECT_BASE
            + "AND name = @name "
            ;
        private static readonly String SELECT_BY_ID = SELECT_BASE
            + "AND step_id = @step_id "
            ;
        private static readonly String SELECT_BY_NAME_MAP = SELECT_BY_NAME
            + "AND map_id = @map_id "
            ;
        private static readonly String UPDATE = ""
                + "UPDATE " + TABLE + " set "
                + "    map_id = @map_id "
                + "    queue_id = @queue_id "
                + "    next_step_id = @next_step_id "
                + "    type = @type "
                + "    name = @name "
                + " "
                + "WHERE step_id = @step_id"
                ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public String Name { get; set; }
        public int MapId { get; set; }
        public int QueueId { get; set; }
        public int NextStepId { get; set; }
        public StepType Type { get; set; }
        #endregion

        #region Constructors
        public Step(int id
                , String name
                , int mapId
                , int queueId
                , int nextStepId
                , StepType type
            )
        {
            Id = id;
            Name = name;
            MapId = mapId;
            QueueId = queueId;
            NextStepId = nextStepId;
            Type = type;
        }
        public Step(IDataReader reader)
            : this(DbUtil.GetInt(reader, "step_id")
                  , DbUtil.GetString(reader, "name")
                  , DbUtil.GetInt(reader, "map_id")
                  , DbUtil.GetInt(reader, "queue_id")
                  , NO_NEXT_STEP
                  , (Step.StepType)(DbUtil.GetInt(reader, "type"))
                  )
        {
            if (!DbUtil.IsNull(reader, "next_step_id"))
            {
                this.NextStepId = DbUtil.GetInt(reader, "next_step_id");
            }
        }

        #endregion

        #region CRUD: Insert
        public static Step Insert(IDbConnection dbConn
                                  , String name
                                  , Map map
                                  , Queue queue
                                  , StepType type
                                  )

        {
            return Insert(dbConn, name, map, queue, null, type);
        }

        public static Step Insert(IDbConnection dbConn
                             , String name
                             , Map map
                             , Queue queue
                             , Step nextStep
                             , StepType type
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
            else
            {
                DbUtil.AddParameter(command, "@next_step_id", nextStep.Id);
            }
            DbUtil.AddParameter(command, "@type", (int)type);
            int id = Convert.ToInt32(command.ExecuteScalar());
            Step tmp = new Step(id
                    , name
                    , map.Id
                    , queue.Id
                    , nextStep == null ? NO_NEXT_STEP : nextStep.Id
                    , type
                    );
            return tmp;
        }
        #endregion

        #region CRUD: Update
        public void Update(IDbConnection dbConn)
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = UPDATE;
            DbUtil.AddParameter(command, "@name", this.Name);
            DbUtil.AddParameter(command, "@map_id", this.MapId);
            DbUtil.AddParameter(command, "@queue_id", this.QueueId);
            DbUtil.AddParameter(command, "@type", (int)this.Type);
            DbUtil.AddParameter(command, "@step_id", this.Id);
            DbUtil.AddParameter(command, "@next_step_id", this.NextStepId);
            command.ExecuteNonQuery();
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
                return new Step(reader);
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }
        public static Step Select(IDbConnection dbConn, String name, int mapId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_NAME_MAP;
                DbUtil.AddParameter(command, "@name", name);
                DbUtil.AddParameter(command, "@map_id", mapId);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new Step(reader);
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }

        public static Step Select(IDbConnection dbConn, int stepId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_ID;
                DbUtil.AddParameter(command, "@step_id", stepId);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new Step(reader);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
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
            sb.Append(", map=");
            sb.Append(this.MapId);
            sb.Append(", q=");
            sb.Append(this.QueueId);
            sb.Append(", next=");
            sb.Append(this.NextStepId);
            sb.Append(", type=");
            sb.Append(this.Type);

            return sb.ToString();
        }
        #endregion
    }
}
