using System;
using System.Collections.Generic;
using DataCapture.Workflow;
using DataCapture.Workflow.Db; // for DbUtil.SelectCount
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class ApiConnectionTest
    {
        [Test()]
        public void CanConnect()
        {
            var dbConn = ConnectionFactory.Create();
            var user = TestUtil.MakeUser(dbConn);
            var wfConn = new DataCapture.Workflow.Connection();

            int before = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, false);
            Assert.GreaterOrEqual(before, 0);

            wfConn.Connect(user.Login);
            int afterConnect = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, true);
            Assert.AreEqual(before + 1, afterConnect);

            wfConn.Disconnect();
            int afterDisconnect = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, false);
            Assert.AreEqual(before, afterDisconnect);
        }

        [Test()]
        public void DisposeDisconnects()
        {
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Session.TABLE);

            using (var wfConn = new DataCapture.Workflow.Connection())
            {
                Assert.IsNotNull(wfConn);
                Console.WriteLine(wfConn);
            }
            int after = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(before, after);
        }

        [Test()]
        public void SessionLimitEnforced()
        {
            var dbConn = ConnectionFactory.Create();
            var user = TestUtil.MakeUser(dbConn);
            var list = new List<Connection>();

            for(int i = 0; i < user.LoginLimit; i++)
            {
                var conn = new Connection();
                conn.Connect(user.Login);
                list.Add(conn);
                Assert.IsNotNull(conn);
            }
            Assert.AreEqual(user.LoginLimit, list.Count);

            String msg = "";
            try
            {
                var conn2many = new Connection();
                conn2many.Connect(user.Login);
            }
            catch(Exception ex)
            {
                msg = ex.Message;
            }

            Console.WriteLine("expected api exception: " + msg); 
            if (String.IsNullOrEmpty(msg))
            {
                Assert.Fail("connection limit violated but no exception thrown");
            }

        }
    }
}
