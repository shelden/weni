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
        public static void Insert(IDbConnection dbConn
            , String login
            , int login_limit
            )
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT;
            DbUtil.AddParameter(command, "@login", login);
            DbUtil.AddParameter(command, "@limit", login_limit);
            command.ExecuteNonQuery();
        }
        #endregion

        #region CRUD: Select
        public static User Select(IDbConnection dbConn, String login)
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = SELECT_BY_LOGIN;
            DbUtil.AddParameter(command, "@login", login);
            var reader = command.ExecuteReader();

            if (reader == null) return null;
            if (!reader.Read()) return null;
            return new User(reader.GetInt32(0) // XXX bad practice to use indexes.  Use names.
                , reader.GetString(1)
                , reader.GetInt32(2)
                );
        }
        #endregion

        #region ToString()
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Login);
            sb.Append(", id=");
            sb.Append(this.Id);
            return sb.ToString();
        }
        #endregion

    }
}


#error error to be caught in jenkins
