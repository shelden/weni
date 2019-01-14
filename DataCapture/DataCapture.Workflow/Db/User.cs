using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Db
{
    public class User
    {
        #region Constants
        public static readonly String TABLE = "workflow.users";
        public static readonly String INSERT = ""
            + "insert into "
            + TABLE
            + " (login, login_limit) "
            + "values (@login, @limit) "
            ;
        public static readonly String SELECT_BY_LOGIN = ""
            + "select user_id "
            + "   , login "
            + "   , login_limit "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   login = @login "
            ;
        #endregion

        #region Properties
        public String Login { get; set; }
        public int LoginLimit { get; set; }
        public int Id { get; set; }
        //public enum Status { get; set; }
        #endregion

        #region Constructors
        public User(int id, String login, int loginLimit)
        {
            Id = id;
            LoginLimit = loginLimit;
            Login = login;
        }
        #endregion

        #region CRUD: Insert
        public static User Insert(IDbConnection dbConn
            , String login
            , int login_limit
            )
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + ";" + DbUtil.GET_KEY;
            DbUtil.AddParameter(command, "@login", login);
            DbUtil.AddParameter(command, "@limit", login_limit);
            int id = Convert.ToInt32(command.ExecuteScalar());
            return new User(id, login, login_limit);
        }
        #endregion

        #region CRUD: Select
        public static User Select(IDbConnection dbConn, String login)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_LOGIN;
                DbUtil.AddParameter(command, "@login", login);reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new User(DbUtil.GetInt(reader, "user_id")
                    , DbUtil.GetString(reader, "login")
                    , DbUtil.GetInt(reader, "login_limit")
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
            sb.Append(this.GetType().FullName);
            sb.Append(' ');
            sb.Append(this.Login);
            sb.Append(", id=");
            sb.Append(this.Id);
            return sb.ToString();
        }
        #endregion

    }
}
