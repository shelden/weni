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
            + " (name, type) "
            + "values (@name, type) "
            ;
        public static readonly String SELECT_BY_NAME = ""
            + "select queue_id "
            + "   , name "
            + "   , type "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   name = @name "
            ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public int Type { get; set; } // XXX these are supposed to be enums
        public String Name { get; set; }
        #endregion

        #region Constructors
        public Queue (int id
            , String name
            , int type = 1
            )
        {
            Id = id;
            Name = name;
            Type = type;
        }
        #endregion

        #region CRUD: Insert
        public static Queue Insert(IDbConnection dbConn
                  , String name
                  )

        {
            int type = 1; // XXX should be enum
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;
            DbUtil.AddParameter(command, "@name", name);
            DbUtil.AddParameter(command, "@type", type);

            int id = Convert.ToInt32(command.ExecuteScalar());
            Queue tmp = new Queue(id, name, type);
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
                return new Queue(reader.GetInt32(0) // XXX bad practice to use indexes.  Use names.
                    , reader.GetString(1)
                    , reader.GetInt32(2)
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
            sb.Append(this.Id);
            sb.Append(", name=");
            sb.Append(this.Name);
            return sb.ToString();
        }
        #endregion

    }
}
