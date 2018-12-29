using System;
using System.Data;
using System.Text;
using DataCapture.Workflow.Db;

namespace DataCapture.Workflow
{
    public class Connection
    {
        #region constants
        #endregion

        #region members
        private User user_ = null;
        private Session session_ = null;
        private IDbConnection dbConn_ = null; // XXX destructor should close
        #endregion

        #region properties
        public bool IsConnected
        {
            get
            {
                if (dbConn_ == null) return false;
                switch (dbConn_.State)
                {
                    case ConnectionState.Open:
                    case ConnectionState.Executing:
                    case ConnectionState.Fetching:
                        return true;
                    default:
                        // all other cases (closed, broken, added-in-future)
                        // should return false.
                        break;
                }
                return false;
            }
        }
        #endregion

        #region constructors
        public Connection()
        { /* no code */ }
        #endregion

        #region Connection
        public void Connect()
        {
            dbConn_ = ConnectionFactory.Create(); /// XXX transactions?
            user_ = User.Select(dbConn_, Environment.UserName); // XXX is this secure?  Or just envariable?
            if (user_ == null)
            {
                user_ = User.Insert(dbConn_, Environment.UserName, 1);
            }
            // XXX: check number of outstanding sessions
            session_ = Session.Insert(dbConn_, user_);
        }
        public void Disconnect()
        {
            try 
            {
                if (IsConnected && session_ != null)
                {
                    session_.Delete(dbConn_);
                }
            }
            finally 
            {
                DbUtil.ReallyClose(dbConn_);
                session_ = null;
                dbConn_ = null;
            }
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            if (IsConnected) {
                sb.Append(" as ");
                sb.Append(user_.Login);
                sb.Append(" from ");
                sb.Append(session_.Hostname);
            }
            else sb.Append(" not connected");
            return sb.ToString();
}
        #endregion
    }
}
