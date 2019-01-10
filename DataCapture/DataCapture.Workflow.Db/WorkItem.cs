using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

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
        private static readonly String INSERT = ""
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
                
        private static readonly String SELECT_BY_NAME = ""
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

        private static readonly String SELECT_BY_QUEUE_PRIORITY = ""
                + "select "
                + "     w.item_id "
                + "     , w.step_id "
                + "     , w.name "
                + "     , w.state "
                + "     , w.priority "
                + "     , w.created "
                + "     , w.entered "
                + "     , w.session_id "
                + "FROM "
                + TABLE + " w "
                + ", " + Queue.TABLE + " q "
                + ", " + Step.TABLE + " s "
                + "WHERE 0 = 0 "
                + "AND s.step_id = w.step_id " // XXX: use 21st century join syntax :-)
                + "AND s.queue_id = q.queue_id "
                + "AND q.queue_id = @queue_id "
                + "AND w.state = @available " 
                + "ORDER BY w.priority "
                + "         , w.created "
            ;
        private static readonly String UPDATE = ""
            + "UPDATE " + TABLE + " set "
            + "    step_id = @step_id "
            + "    , name = @name "
            + "    , state = @state "
            + "    , priority = @priority "
            + "    , created = @created "
            + "    , entered = @entered "
            + "    , session_id = @session_id "
            + "WHERE 0 = 0 "
            + "AND   item_id = @item_id "
            ;
                



        #endregion

        #region Properties
        public int Id { get; private set; }
        public int StepId { get; private set; }
        public int SessionId { get; set; }
        public int Priority { get; set; }
        public WorkItem.State ItemState { get; set; }
        public String Name { get; set; }
        public DateTime Created { get; private set; }
        public DateTime Entered { get; set; }
        #endregion

        #region Constructors
        public WorkItem(int id
                        , int stepId
                        , String name
                        , WorkItem.State state
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
        public WorkItem(IDataReader reader)
            : this(DbUtil.GetInt(reader, "item_id")
                  , DbUtil.GetInt(reader, "step_id")
                  , DbUtil.GetString(reader, "name")
                  , (WorkItem.State)DbUtil.GetInt(reader, "state")
                  , DbUtil.GetInt(reader, "priority")
                  , DbUtil.GetDateTime(reader, "created")
                  , DbUtil.GetDateTime(reader, "entered")
                  , DbUtil.GetInt(reader, "session_id")
                  )
        { /* no code */ }

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
                return new WorkItem(reader);
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }

        // TODO: spec overloads this method with ranges.
        //       which is a good idea as opposed to the quick-
        //       and-dirty "slurp" method written below.
        public static IList<WorkItem> SelectByPriority(IDbConnection dbConn, Queue queue)
        {
            IDataReader reader = null;
            List<WorkItem> tmp = new List<WorkItem>();
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_QUEUE_PRIORITY;
                DbUtil.AddParameter(command, "@queue_id", queue.Id);
                DbUtil.AddParameter(command, "@available", (int)WorkItem.State.Available);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                while(reader.Read())
                {
                    tmp.Add(new WorkItem(reader));
                }
                return tmp;
            }
            catch(Exception ex)
            {
                Console.WriteLine("SBP: " + ex.Message);
                Console.WriteLine("SBP: " + SELECT_BY_QUEUE_PRIORITY);
                throw;
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }
        #endregion

        #region CRUD: Update
        public void Update(IDbConnection dbConn)
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = UPDATE;
            DbUtil.AddParameter(command, "@step_id", this.StepId);
            DbUtil.AddParameter(command, "@name", this.Name);
            DbUtil.AddParameter(command, "@state", (int)this.ItemState);
            DbUtil.AddParameter(command, "@priority", this.Priority);
            DbUtil.AddParameter(command, "@created", this.Created);
            DbUtil.AddParameter(command, "@entered", this.Entered);
            DbUtil.AddParameter(command, "@session_id", this.SessionId);
            DbUtil.AddParameter(command, "@item_id", this.Id);
            try
            {
                              command.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine(UPDATE);
                Console.WriteLine(ex.Message);
                throw;
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

