using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Db
{
    public class Map
    {
        #region Constants
        public static readonly String TABLE = "workflow.maps";
	    public static readonly int VERSION = 1;
        public static readonly String INSERT = ""
            + "insert into "
            + TABLE
            + " (name, version) "
            + "values (@name, @version) "
            ;
        public static readonly String SELECT_BY_NAME = ""
            + "select map_id "
            + "   , name "
            + "   , version "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   name = @name "
            ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public String Name { get; set; }
        public int Version { get; set; } // XXX: Int16?
        #endregion

        #region Constructors
        public Map (int id
            , String name
            , int version = 1
            )
        {
            Id = id;
            Name = name;
            Version = version;
        }
        #endregion

        #region CRUD: Insert
        public static Map Insert(IDbConnection dbConn
                  , String name
            , int version = 1
                  )

        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;
            DbUtil.AddParameter(command, "@name", name);
            DbUtil.AddParameter(command, "@version", version);

            int id = Convert.ToInt32(command.ExecuteScalar());
            Map tmp = new Map(id, name, version);
            return tmp;
        }
        #endregion

        #region CRUD: Select
        public static Map Select(IDbConnection dbConn, String name)
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
                return new Map(reader.GetInt32(0) // XXX bad practice to use indexes.  Use names.
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
            sb.Append(", v");
            sb.Append(this.Version);
            return sb.ToString();
        }
        #endregion

    }
}
