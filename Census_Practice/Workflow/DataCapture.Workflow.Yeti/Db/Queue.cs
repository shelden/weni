using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Yeti.Db
{
    public class Queue
    {
        #region Constants
        public static readonly String TABLE = "workflow.queues";
        private static readonly String INSERT = ""
            + "INSERT INTO "
            + TABLE
            + " (name, is_fail) "
            + "VALUES (@name, @is_fail) "
            ;
        private static readonly String SELECT_BASE = ""
            + "SELECT queue_id "
            + "   , name "
            + "   , is_fail "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            ;
        private static readonly String SELECT_BY_NAME = SELECT_BASE
            + "AND   name = @name "
            ;
        private static readonly String SELECT_BY_ID = SELECT_BASE 
            + "AND   queue_id = @queue_id "
            ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public String Name { get; set; }
        /// <summary>
        /// Is this a normal queue?  Or a fail queue?
        /// </summary>
        public bool IsNormal { get; set; }
        /// <summary>
        /// Is this a fail queue?  Or a normal queue?
        /// </summary>
        public bool IsFail
        {
            get { return !IsNormal; }
        }
        #endregion

        #region Constructors
        public Queue (int id
            , String name
            , bool isFail = false
            )
        {
            Id = id;
            Name = name;
            IsNormal = !isFail;
        }
        public Queue(IDataReader reader)
        {
            Id = DbUtil.GetInt(reader, "queue_id");
            Name = DbUtil.GetString(reader, "name");
            IsNormal = !DbUtil.GetBool(reader, "is_fail");
        }
        #endregion

        #region CRUD: Insert
        public static Queue Insert(IDbConnection dbConn
            , String name
            , bool isFail = false
            )
        { 
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;
                DbUtil.AddParameter(command, "@name", name);
                DbUtil.AddParameter(command, "@is_fail", isFail);

                int id = Convert.ToInt32(command.ExecuteScalar());
                Queue tmp = new Queue(id, name, isFail);
                return tmp;
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.Append("Cannot insert queue [");
                msg.Append(name);
                msg.Append("]: ");
                msg.Append(ex.Message);
                throw new Exception(msg.ToString(), ex);
            }
        }
        #endregion

        #region CRUD: Select
        public static Queue Select(IDbConnection dbConn, String name)
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
                return new Queue(reader);
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }
        public static Queue Select(IDbConnection dbConn, int queueId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_ID;
                DbUtil.AddParameter(command, "@queue_id", queueId);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new Queue(reader);
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
            if (IsFail)
            {
                sb.Append(" fail");
            }
            return sb.ToString();
        }
        #endregion

    }
}
