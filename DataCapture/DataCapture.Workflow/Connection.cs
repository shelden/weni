﻿using System;
using System.Data;
using System.Text;
using DataCapture.Workflow.Db;

namespace DataCapture.Workflow
{
    public class Connection : IDisposable
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
            Connect(Environment.UserName);
        }

        // this should be protected so we can test connecting as other people.
        // We don't want this exposed for the general public XXX
        public void Connect(String asUser)
        {
            try
            {
                dbConn_ = ConnectionFactory.Create(); /// XXX transactions?
                user_ = User.Select(dbConn_, asUser);
                if (user_ == null)
                {
                    var msg = new StringBuilder();
                    msg.Append("No such workflow user [");
                    msg.Append(asUser);
                    msg.Append(']');
                    throw new Exception(msg.ToString());
                }
                var sessions = Session.SelectAll(dbConn_, user_);
                if (sessions.Count >= user_.LoginLimit)
                {
                    var msg = new StringBuilder();
                    msg.Append("[");
                    msg.Append(user_.Login);
                    msg.Append("] is limited to ");
                    msg.Append(user_.LoginLimit);
                    msg.Append(" sessions, and there are currently ");
                    msg.Append(sessions.Count);
                    throw new Exception(msg.ToString());
                }
                session_ = Session.Insert(dbConn_, user_);
            }
            catch
            {
                DbUtil.ReallyClose(dbConn_);
                session_ = null;
                user_ = null;
                throw;
            }
        }
        // note, Disconnect is called from Dispose, so it cannot throw
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

        #region Dispose
        public void Dispose()
        {
            this.Disconnect();
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            if (IsConnected)
            {
                sb.Append(" as ");
                sb.Append(user_.Login);
                sb.Append(" from ");
                sb.Append(session_.Hostname);
                sb.Append(" max ");
                sb.Append(user_.LoginLimit);
            }
            else sb.Append(" not connected");
            return sb.ToString();
        }
        #endregion
    }
}