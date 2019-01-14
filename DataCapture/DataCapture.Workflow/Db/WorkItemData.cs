using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace DataCapture.Workflow.Db
{
    public class WorkItemData
    {
        #region Constants
        public static readonly String TABLE = "workflow.work_item_data";
        private static readonly String INSERT = ""
                + "INSERT INTO "
                + TABLE
                + " ( "
                + "     item_id "
                + "     , variable_name "
                + "     , variable_value "
                + ") "
                + "VALUES ("
                + "     @item_id "
                + "     , @variable_name "
                + "     , @variable_value "
                + ") "
            ;
                
        private static readonly String SELECT_BY_DATA_ID = ""
                + "SELECT "
                + "     data_id "
                + "     , item_id "
                + "     , variable_name "
                + "     , variable_value "
                + "FROM "
                + TABLE + " "
                + "WHERE 0 = 0 "
                + "AND   data_id = @data_id "
                ;
        private static readonly String SELECT_BY_WORK_ITEM_ID = ""
                + "SELECT "
                + "     data_id "
                + "     , item_id "
                + "     , variable_name "
                + "     , variable_value "
                + "FROM "
                + TABLE + " "
                + "WHERE 0 = 0 "
                + "AND   item_id = @item_id "
                ;

        private static readonly String DELETE_BY_ITEM_ID = ""
            + "DELETE from "
            + TABLE
            + " WHERE 0 = 0"
            + " AND item_id = @item_id"
    ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public int WorkItemId { get; set; }
        public String VariableName { get; set; }
        public String VariableValue { get; set; }
        #endregion

        #region Constructors
        public WorkItemData(int id
                            , int workItemId
                            , String variableName
                            , String variableValue
                        )
        {
            Id = id;
            WorkItemId = workItemId;
            VariableName = variableName;
            VariableValue = variableValue;
        }
        public WorkItemData(IDataReader reader)
            : this(DbUtil.GetInt(reader, "data_id")
                  , DbUtil.GetInt(reader, "item_id")
                  , DbUtil.GetString(reader, "variable_name")
                  , DbUtil.GetString(reader, "variable_value")
                  )
        { /* no code */ }
        #endregion

        #region CRUD: Insert
        public static WorkItemData Insert(IDbConnection dbConn
                                          , WorkItem workItem
                                          , String key
                                          , String value
            )
                                          
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;

            DbUtil.AddParameter(command, "@item_id", workItem.Id);
            DbUtil.AddParameter(command, "@variable_name", key);
            DbUtil.AddParameter(command, "@variable_value", value);

            int id = Convert.ToInt32(command.ExecuteScalar());
            return new WorkItemData(id
                                    , workItem.Id
                                    , key
                                    , value
                                    );
                                                                     
        }
        public static IList<WorkItemData> Insert(IDbConnection dbConn
                    , WorkItem item
                    , IDictionary<String, String> pairs
                    )
        {
            var tmp = new List<WorkItemData>();
            if (pairs == null) return tmp;
            foreach (var key in pairs.Keys)
            {
                tmp.Add(WorkItemData.Insert(dbConn, item, key, pairs[key]));
            }
            return tmp;
        }
        #endregion

        #region CRUD: Select
        public static WorkItemData Select(IDbConnection dbConn, int dataId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_DATA_ID;
                DbUtil.AddParameter(command, "@data_id", dataId);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new WorkItemData(reader);
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }

        public static IList<WorkItemData> SelectAll(IDbConnection dbConn, int workItemId)
        {
            IDataReader reader = null;
            var tmp = new List<WorkItemData>();
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_WORK_ITEM_ID;
                DbUtil.AddParameter(command, "@item_id", workItemId);
                reader = command.ExecuteReader();

                if (reader == null) return tmp;
                while(reader.Read())
                {
                    tmp.Add(new WorkItemData(reader));
                }
                return tmp;
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }
        #endregion

        #region CRUD: Delete
        public static void DeleteAll(IDbConnection dbConn, int workItemId)
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = DELETE_BY_ITEM_ID;
            DbUtil.AddParameter(command, "@item_id", workItemId);
            command.ExecuteScalar();
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
            sb.Append(", ");
            sb.Append(this.VariableName);
            sb.Append("=");
            sb.Append(this.VariableValue);
            return sb.ToString();
        }
        #endregion
    }
}

