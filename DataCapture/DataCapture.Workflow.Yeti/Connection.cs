using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using DataCapture.Workflow.Yeti.Db;

namespace DataCapture.Workflow.Yeti
{
    using KeyValuePairs = Dictionary<String, String>;
    public class Connection : IDisposable
    {
        #region constants
        #endregion

        #region members
        private User user_ = null;
        private Session session_ = null;
        private IDbConnection dbConn_ = null; 
        #endregion

        #region properties
        public bool IsConnected
        {
            get
            {
                if (dbConn_ == null) return false;
                switch (dbConn_.State)
                {
                    case ConnectionState.Open:
                    case ConnectionState.Executing:
                    case ConnectionState.Fetching:
                        return true;
                    default:
                        // all other cases (closed, broken, added-in-future)
                        // should return false.
                        break;
                }
                return false;
            }
        }
        #endregion

        #region constructors
        public Connection()
        { /* no code */ }
        #endregion

        #region Connection
        
        public void Connect()
        {
            Connect(Environment.UserName);
        }

        // this should be protected so we can test connecting as other people.
        // We don't want this exposed for the general public XXX
        public void Connect(String asUser)
        {
            try
            {
                dbConn_ = ConnectionFactory.Create(); /// XXX transactions?
                user_ = User.Select(dbConn_, asUser);
                if (user_ == null)
                {
                    var msg = new StringBuilder();
                    msg.Append("No such workflow user [");
                    msg.Append(asUser);
                    msg.Append(']');
                    throw new Exception(msg.ToString());
                }
                var sessions = Session.SelectAll(dbConn_, user_);
                if (sessions.Count >= user_.LoginLimit)
                {
                    var msg = new StringBuilder();
                    msg.Append("[");
                    msg.Append(user_.Login);
                    msg.Append("] is limited to ");
                    msg.Append(user_.LoginLimit);
                    msg.Append(" sessions, and there are currently ");
                    msg.Append(sessions.Count);
                    throw new Exception(msg.ToString());
                }
                session_ = Session.Insert(dbConn_, user_);
            }
            catch
            {
                DbUtil.ReallyClose(dbConn_);
                session_ = null;
                user_ = null;
                throw;
            }
        }
        // note, Disconnect is called from Dispose, so it cannot throw
        public void Disconnect()
        {
            try 
            {
                if (IsConnected && session_ != null)
                {
                    session_.Delete(dbConn_);
                }
            }
            finally 
            {
                DbUtil.ReallyClose(dbConn_);
                session_ = null;
                dbConn_ = null;
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            this.Disconnect();
        }
        #endregion

        #region Add/Get Items
        public void CreateItem(String mapName
            , String itemName
            , String stepName
            , KeyValuePairs data
            , int priority = 0
            )
        {
            IDbTransaction transaction = null;
            try
            {
                transaction = dbConn_.BeginTransaction();

                var map = Map.Select(dbConn_, mapName);
                if (map == null)
                {
                    var msg = new StringBuilder();
                    msg.Append("No such map [");
                    msg.Append(mapName);
                    msg.Append("]");
                    throw new Exception(msg.ToString());
                }

                var step = Step.Select(dbConn_, stepName, map.Id);

                if (step == null)
                {
                    var msg = new StringBuilder();
                    msg.Append("No such step [");
                    msg.Append(stepName);
                    msg.Append("] in map [");
                    msg.Append(map.Name);
                    msg.Append("]");
                    throw new Exception(msg.ToString());
                }

                if (step.Type != Step.StepType.Start)
                {
                    var msg = new StringBuilder();
                    msg.Append("One may only CreateItem() on start steps.  ");
                    msg.Append("[");
                    msg.Append(step.Name);
                    msg.Append("] is type [");
                    msg.Append(step.Type);
                    msg.Append("]");
                    throw new Exception(msg.ToString());
                }

                if (map.Id != step.MapId)
                {
                    var msg = new StringBuilder();
                    msg.Append("Step [");
                    msg.Append(stepName);
                    msg.Append("] is in map #");
                    msg.Append(step.MapId);
                    msg.Append(", not #");
                    msg.Append(map.Id); // TODO: look up correct map name and report it here
                    throw new Exception(msg.ToString());
                }

                var item = WorkItem.Insert(dbConn_
                    , step
                    , itemName
                    , priority
                    , session_
                    );

                if (data != null)
                {
                    foreach (String key in data.Keys)
                    {
                        var kvp = WorkItemData.Insert(dbConn_, item, key, data[key]);
                    }
                }
                transaction.Commit();
                transaction = null;
            }
            finally
            {
                DbUtil.ReallyClose(transaction);
            }
        }

        public WorkItemInfo GetItem(String queueName)
        {
            IDbTransaction transaction = null;
            try
            {
                transaction = dbConn_.BeginTransaction();

                // first get the queue object, so we can query things in
                // all steps hanging off that queue:

                var queue = Queue.Select(dbConn_, queueName);
                if (queue == null) throw new Exception("unknown queue [" + queueName + "]");

                // TODO check queue perms here: 
                //var allowed = AllowedQueue.Select(dbConn_, this.user_, queue);

                var items = WorkItem.SelectByPriority(dbConn_, queue);
                if (items == null || items.Count == 0) return null;

                // WorkItemInfos have information about the item's
                // step, and map; and the queue under which we queried
                // for it.  Therefore we need all 4 things to build
                // a WorkItemInfo:  

                var item = items[0];
                var step = Step.Select(dbConn_, item.StepId);
                var map = Map.Select(dbConn_, step.MapId);

                // Now update the work item, otherwise it will always be the
                // one returned:
                item.ItemState = WorkItemState.InProgress;
                item.Entered = DateTime.UtcNow;
                item.SessionId = session_.Id;
                item.Update(dbConn_);

                transaction.Commit();
                transaction = null;
                return new WorkItemInfo(item
                    , map
                    , step
                    , queue
                    , WorkItemData.SelectAll(dbConn_, item.Id)
                    );
            }
            finally
            {
                DbUtil.ReallyBackout(transaction);
            }
        }
        #endregion

        #region Finish / Return Item
        public void FinishItem(WorkItemInfo toBeFinished)
        {
            IDbTransaction transaction = null;
            try
            {
                transaction = dbConn_.BeginTransaction();

                var item = WorkItem.Select(dbConn_, toBeFinished.Id);
                if (item == null)
                {
                    var msg = new StringBuilder();
                    msg.Append("cannot finish item [");
                    msg.Append(item.Name);
                    msg.Append("], #");
                    msg.Append(item.Id);
                    msg.Append("; not found in database");
                    throw new Exception(msg.ToString());
                }

                var currentStep = Step.Select(dbConn_, item.StepId);
                if (currentStep == null) throw new Exception("internal error, no such current step on " + item.ToString());


                // update the step and state:
                if (currentStep.Type == Step.StepType.Terminating)
                {
                    item.Delete(dbConn_);
                }
                else if (currentStep.NextStepId == Step.NO_NEXT_STEP)
                {
                    throw new Exception("internal error: no next step, but not a terminating step: " + currentStep);
                }
                else
                {
                    // XXX: this assumes that no rules apply.
                    // XXX: it also assumes that we always want to 
                    //      finish by moving it along.
                    item.StepId = currentStep.NextStepId;
                    item.ItemState = WorkItemState.Available;
                    item.Entered = DateTime.UtcNow;
                    item.SessionId = session_.Id;
                    item.Priority = toBeFinished.Priority;
                    item.Name = toBeFinished.Name;
                    item.Update(dbConn_);

                    // probably more efficient to update the matching
                    // ones, then to delete them all and recreate.  But
                    // also harder to code correctly.  Can be updated 
                    // later if efficiency becomes a problem
                    WorkItemData.DeleteAll(dbConn_, item.Id);
                    WorkItemData.Insert(dbConn_, item, toBeFinished);
                }


                transaction.Commit();
                transaction = null;
            }
            finally
            {
                DbUtil.ReallyBackout(transaction);
            }
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            if (IsConnected)
            {
                sb.Append(" as ");
                sb.Append(user_.Login);
                sb.Append(" from ");
                sb.Append(session_.Hostname);
                sb.Append(" max ");
                sb.Append(user_.LoginLimit);
            }
            else sb.Append(" not connected");
            return sb.ToString();
        }
        #endregion
    }
}
