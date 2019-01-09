using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Db
{
    public class WorkItem
    {
        #region Enums
        public enum State
        {
            Available = 1
                , Locked = 2
                , InProgress = 3
                , Suspended = 4
                , Terminated = 9
        };
        #endregion

        #region Constants
        public static readonly String TABLE = "workflow.work_items";
        public static readonly String INSERT = ""
                + "insert into "
                + TABLE
                + " ( "
                + "     step_id "
                + "     , name "
                + "     , state "
                + "     , priority "
                + "     , created "
                + "     , entered "
                + "     , session_id "
                + ") "
                + "values ("
                + "     @step_id "
                + "     , @name "
                + "     , @state "
                + "     , @priority "
                + "     , @created "
                + "     , @entered "
                + "     , @session_id "
                + ") "
            ;
                
        public static readonly String SELECT_BY_NAME = ""
                + "select "
                + "     item_id "
                + "     , step_id "
                + "     , name "
                + "     , state "
                + "     , priority "
                + "     , created "
                + "     , entered "
                + "     , session_id "
                + "FROM "
                + TABLE + " "
                + "WHERE 0 = 0 "
                + "AND   name = @name "
                ;

        #endregion
                    
        #region Properties
        public int Id { get; set; }
        public int StepId { get; set; }
        public int SessionId { get; set; }
        public int Priority { get; set; }
        public WorkItem.State ItemState { get; set; }
        public String Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Entered { get; set; }
        #endregion

        #region Constructors
        public WorkItem(int id
                        , int stepId
                        , String name
                        , WorkItem.State state // XXX enum
                        , int priority
                        , DateTime created
                        , DateTime entered
                        , int sessionId
                        )
        {
            Id = id;
            StepId = stepId;
            Name = name;
            Created = created;
            Entered = entered;
            SessionId = sessionId;
            Priority = priority;
            ItemState = state;
        }
        #endregion

        #region CRUD: Insert
        public static WorkItem Insert(IDbConnection dbConn
                                      , Step step
                                      , String name
                                      , int priority
                                      , Session session

                )
        {
            DateTime when = DateTime.UtcNow;
            
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;

            DbUtil.AddParameter(command, "@step_id", step.Id);
            DbUtil.AddParameter(command, "@name", name);
            DbUtil.AddParameter(command, "@state", (int)WorkItem.State.Available);
            DbUtil.AddParameter(command, "@priority", priority);
            DbUtil.AddParameter(command, "@created", when);
            DbUtil.AddParameter(command, "@entered", when);
            DbUtil.AddParameter(command, "@session_id", session.Id);

            int id = Convert.ToInt32(command.ExecuteScalar());
            return new WorkItem(id
                                    , step.Id
                                    , name
                                    , WorkItem.State.Available
                                    , priority
                                    , when
                                    , when
                                    , session.Id
                                    );
        }
        #endregion

        #region CRUD: Select
        public static WorkItem Select(IDbConnection dbConn, String name)
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
                return new WorkItem(DbUtil.GetInt(reader, "item_id")
                    , DbUtil.GetInt(reader, "step_id")
                    , DbUtil.GetString(reader, "name")
                    , (WorkItem.State)DbUtil.GetInt(reader, "state")
                    , DbUtil.GetInt(reader, "priority")
                    , DbUtil.GetDateTime(reader, "created")
                    , DbUtil.GetDateTime(reader, "entered")
                    , DbUtil.GetInt(reader, "session_id")
                    );
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
            sb.Append(", step=");
            sb.Append(this.StepId);
            sb.Append(", state=");
            sb.Append(this.ItemState);
            sb.Append(", prio=");
            sb.Append(this.Priority);
            return sb.ToString();
        }
        #endregion
    }
}

