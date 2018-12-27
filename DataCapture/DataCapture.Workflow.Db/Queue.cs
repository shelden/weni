using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Db
{
    public class Queue
    {
        #region Constants
        public static readonly String TABLE = "workflow.queues";
        public static readonly String INSERT = ""
            + "insert into "
            + TABLE
            + " (name, is_fail) "
            + "values (@name, @is_fail) "
            ;
        public static readonly String SELECT_BY_NAME = ""
            + "select queue_id "
            + "   , name "
            + "   , is_fail "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   name = @name "
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
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;
            DbUtil.AddParameter(command, "@name", name);
            DbUtil.AddParameter(command, "@is_fail", isFail);

            int id = Convert.ToInt32(command.ExecuteScalar());
            Queue tmp = new Queue(id, name, isFail);
            return tmp;
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
