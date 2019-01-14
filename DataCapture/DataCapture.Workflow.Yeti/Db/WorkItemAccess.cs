using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace DataCapture.Workflow.Yeti.Db
{
    public class WorkItemAccess
    {
        #region Constants
        public static readonly String TABLE = "workflow.work_item_access";
        public static readonly String INSERT = ""
                + "INSERT INTO "
                + TABLE
                + " ( "
                + "     item_id "
                + "     , user_id "
                + "     , is_allowed "
                + ") "
                + "VALUES ("
                + "     @item_id "
                + "     , @user_id "
                + "     , @is_allowed "
                + ") "
            ;

        public static readonly String SELECT_BY_ID = ""
                + "SELECT "
                + "     access_id "
                + "     , item_id "
                + "     , user_id "
                + "     , is_allowed "
                + "FROM "
                + TABLE + " "
                + "WHERE 0 = 0 "
                + "AND   access_id = @access_id "
                ; 

            public static readonly String SELECT_BY_ITEM_USER_ID = ""
         + "SELECT "
         + "     access_id "
         + "     , item_id "
         + "     , user_id "
         + "     , is_allowed "
         + "FROM "
         + TABLE + " "
         + "WHERE 0 = 0 "
            + "AND item_id = @item_id "
            + "AND user_id = @user_id"
         ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WorkItemId { get; set; }
        public bool IsAllowed { get; set; }
        #endregion

        #region Constructors
        public WorkItemAccess(int id
            , int workItemId
            , int userId
            , bool isAllowed
                        )
        {
            Id = id;
            WorkItemId = workItemId;
            IsAllowed = isAllowed;
            UserId = userId;
        }
        public WorkItemAccess(IDataReader reader)
        {
            Id = DbUtil.GetInt(reader, "access_id");
            WorkItemId = DbUtil.GetInt(reader, "item_id");
            IsAllowed = DbUtil.GetBool(reader, "is_allowed");
            UserId = DbUtil.GetInt(reader, "user_id");
        }
        #endregion

        #region CRUD: Insert
        public static WorkItemAccess Insert(IDbConnection dbConn
                                          , WorkItem workItem
                                          , User user
                                          , bool isAllowed
            )
                                          
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;

            DbUtil.AddParameter(command, "@item_id", workItem.Id);
            DbUtil.AddParameter(command, "@user_id", user.Id);
            DbUtil.AddParameter(command, "@is_allowed", isAllowed);

            int id = Convert.ToInt32(command.ExecuteScalar());
            return new WorkItemAccess(id
                                    , workItem.Id
                                    , user.Id
                                    , isAllowed
                                    );
                                                                     
        }
        #endregion

        #region CRUD: Select
        public static WorkItemAccess Select(IDbConnection dbConn, int accessId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_ID;
                DbUtil.AddParameter(command, "@access_id", accessId);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new WorkItemAccess(reader);
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }
        public static WorkItemAccess Select(IDbConnection dbConn, int workItemId, int userId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_ITEM_USER_ID;
                DbUtil.AddParameter(command, "@item_id", workItemId);
                DbUtil.AddParameter(command, "@user_id", userId);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new WorkItemAccess(reader);
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
            sb.Append(", workItem=");
            sb.Append(this.WorkItemId);
            sb.Append(", user=");
            sb.Append(this.UserId);
            sb.Append(" ");
            sb.Append(this.IsAllowed ? "allowed" : "denied");
            return sb.ToString();
        }
        #endregion
    }
}
