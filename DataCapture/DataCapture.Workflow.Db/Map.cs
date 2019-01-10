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
        private static readonly String INSERT = ""
            + "insert into "
            + TABLE
            + " (name, version) "
            + "values (@name, @version) "
            ;
        private static readonly String SELECT_BY_NAME = ""
            + "select map_id "
            + "   , name "
            + "   , version "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   name = @name "
            + "ORDER BY version DESC "
            ;
        private static readonly String SELECT_BY_ID = ""
             + "select map_id "
             + "   , name "
             + "   , version "
              + " from "
         + TABLE + " "
         + "WHERE 0 = 0 "
          + "AND   map_id = @map_id "
          ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public String Name { get; set; }
        public int Version { get; set; } 
        #endregion

        #region Constructors
        public Map (int id
            , String name
            , int version
            )
        {
            Id = id;
            Name = name;
            Version = version;
        }
        public Map(IDataReader reader)
        {
            Id = DbUtil.GetInt(reader, "map_id");
            Name = DbUtil.GetString(reader, "name");
            Version = DbUtil.GetInt(reader, "version");
        }
        #endregion

        #region CRUD: Insert
        /// <summary>
        /// Insert the specified dbConn, name and version.
        /// Version doesn't default.  If you want to insert
        /// while creating a new version if it already exists,
        /// ise InsertWithMaxVersion()
        /// </summary>
        /// <returns>The insert.</returns>
        /// <param name="dbConn">Db conn.</param>
        /// <param name="name">Name.</param>
        /// <param name="version">Version.</param>
        public static Map Insert(IDbConnection dbConn
            , String name
            , int version
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
        /// <summary>
        /// inserts a new map.  Increments version if there is already
        /// a map with that name.
        /// </summary>
        /// <returns>The resultantly added map</returns>
        /// <param name="dbConn">Db conn.</param>
        /// <param name="name">Name.</param>
        public static Map InsertWithMaxVersion(IDbConnection dbConn
            , String name
            )
        {
            var max = Map.Select(dbConn, name);
            int nextVersion = max == null ? Map.VERSION : max.Version + 1;
            return Map.Insert(dbConn, name, nextVersion);
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
                return new Map(reader);
                // Note: this assumes that we only every want the one with the
                // max version.  If that stops being true, we will need an
                // overload that specifies the version.
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }
        public static Map Select(IDbConnection dbConn, int mapId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_ID;
                DbUtil.AddParameter(command, "@map_id", mapId);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new Map(reader);
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
            sb.Append(", v");
            sb.Append(this.Version);
            return sb.ToString();
        }
        #endregion

    }
}
