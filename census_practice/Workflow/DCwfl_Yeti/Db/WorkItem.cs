﻿using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using LM.DataCapture.Workflow.Yeti;

namespace LM.DataCapture.Workflow.Yeti.Db
{
    public class WorkItem
    {
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
        private static readonly String SELECT_BASE = ""
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
            ;
        private static readonly String SELECT_BY_NAME = SELECT_BASE 
                + "AND   name = @name "
                ;
        private static readonly String SELECT_BY_ID = SELECT_BASE
            + "AND   item_id = @item_id "
            ;

        private static readonly String SELECT_BY_QUEUE_PRIORITY = ""
                + "SELECT "
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
                + "      , " + Queue.TABLE + " q "
                + "      , " + Step.TABLE + " s "
                + "WHERE 0 = 0 "
                + "AND   s.step_id = w.step_id " 
                + "AND   s.queue_id = q.queue_id "
                + "AND   q.queue_id = @queue_id "
                + "AND   w.state = @available " 
                + "ORDER BY w.priority ASC "
                // using w.item_id as a proxy for w.created.  A query on item_id 
                // should be faster since a) it's an int and b) it's already indexed
                // because it's the PK
                + "         , w.item_id ASC " 
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
        private static readonly String DELETE_BY_ID = ""
            + "DELETE FROM " + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND item_id = @item_id "
            ;

        #endregion

        #region Properties
        public int Id { get; private set; }
        public int StepId { get; set; }
        public int SessionId { get; set; }
        public int Priority { get; set; }
        public WorkItemState ItemState { get; set; }
        public String Name { get; set; }
        public DateTime Created { get; private set; }
        public DateTime Entered { get; set; }
        #endregion

        #region Constructors
        public WorkItem(int id
                        , int stepId
                        , String name
                        , WorkItemState state
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
                  , (WorkItemState)DbUtil.GetInt(reader, "state")
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
            DbUtil.AddParameter(command, "@state", (int)WorkItemState.Available);
            DbUtil.AddParameter(command, "@priority", priority);
            DbUtil.AddParameter(command, "@created", when);
            DbUtil.AddParameter(command, "@entered", when);
            DbUtil.AddParameter(command, "@session_id", session.Id);

            int id = Convert.ToInt32(command.ExecuteScalar());
            return new WorkItem(id
                                    , step.Id
                                    , name
                                    , WorkItemState.Available
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
        public static WorkItem Select(IDbConnection dbConn, int itemId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_ID;
                DbUtil.AddParameter(command, "@item_id", itemId);
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
                DbUtil.AddParameter(command, "@available", (int)WorkItemState.Available);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                while(reader.Read())
                {
                    tmp.Add(new WorkItem(reader));
                }
                return tmp;
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
            command.ExecuteNonQuery();

        }
        #endregion

        #region CRUD: Delete
        public void Delete(IDbConnection dbConn)
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = DELETE_BY_ID;
            DbUtil.AddParameter(command, "@item_id", this.Id);
            int rows = command.ExecuteNonQuery();
            // This assumes that the work_item_data rows are deleted in a cascading way
            // by the DB.  It's enforced by unit tests.
            switch (rows)
            {
                case 0:
                    // strange; not even there.  Maybe warn?
                    break;
                case 1:
                    // working correctly.
                    break;
                default:
                    var msg = new StringBuilder();
                    msg.Append("internal error: delete by ID returned multiple rows? ");
                    msg.Append(this);
                    throw new Exception(msg.ToString());
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

