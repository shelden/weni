using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Yeti.Db
{
    public class User
    {
        #region Constants
        public static readonly String TABLE = "workflow.users";
        private static readonly String INSERT = ""
            + "insert into "
            + TABLE
            + " (login, login_limit) "
            + "values (@login, @limit) "
            ;
        private static readonly String SELECT_BY_LOGIN = ""
            + "select user_id "
            + "   , login "
            + "   , login_limit "
            + " from "
            + TABLE + " "
            + "WHERE 0 = 0 "
            + "AND   login = @login "
            ;
        private static readonly String UPDATE = ""
                + "UPDATE " + TABLE + " SET "
                + "    login = @login "
                + "    , login_limit = @login_limit "
                + " "
                + "WHERE user_id = @user_id"
                ;
        #endregion

        #region Properties
        public String Login { get; set; }
        public int LoginLimit { get; set; }
        public int Id { get; set; }
        #endregion

        #region Constructors
        public User(int id, String login, int loginLimit)
        {
            Id = id;
            LoginLimit = loginLimit;
            Login = login;
        }
        public User(IDataReader reader)
            : this(DbUtil.GetInt(reader, "user_id")
                    , DbUtil.GetString(reader, "login")
                    , DbUtil.GetInt(reader, "login_limit")
                  )
        { /* no code */ }
        #endregion

        #region CRUD: Insert
        public static User Insert(IDbConnection dbConn
            , String login
            , int login_limit
            )
        {
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = INSERT + ";" + DbUtil.GET_KEY;
                DbUtil.AddParameter(command, "@login", login);
                DbUtil.AddParameter(command, "@limit", login_limit);
                int id = Convert.ToInt32(command.ExecuteScalar());
                return new User(id, login, login_limit);
            }
            catch(Exception ex)
            {
                var msg = new StringBuilder();
                msg.Append("Cannot insert user [");
                msg.Append(login);
                msg.Append("]: ");
                msg.Append(ex.Message);
                throw new Exception(msg.ToString(), ex);
            }
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
                DbUtil.AddParameter(command, "@login", login);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;
                return new User(reader); 
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }
        #endregion

        #region CRUD: Update
        public void Update(IDbConnection dbConn)
        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = UPDATE;
            DbUtil.AddParameter(command, "@login", this.Login);
            DbUtil.AddParameter(command, "@login_limit", this.LoginLimit);
            DbUtil.AddParameter(command, "@user_id", this.Id);
            int rows = command.ExecuteNonQuery();

            var msg = new StringBuilder();
            switch (rows)
            {
                case 1:
                    return;
                case 0:
                    msg.Append("cannot update user [");
                    msg.Append(this.ToString());
                    msg.Append("], not found in database");
                    throw new Exception(msg.ToString());
                default:
                    msg.Append("internal error.  Multiple [");
                    msg.Append(rows);
                    msg.Append("] users with id #");
                    msg.Append(this.Id);
                    msg.Append(".  Should be unique in database.  [");
                    msg.Append(this.ToString());
                    msg.Append(']');
                    throw new Exception(msg.ToString());
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
