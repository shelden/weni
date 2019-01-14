using System;
using System.Collections.Generic;
using DataCapture.Workflow.Yeti;
using DataCapture.Workflow.Yeti.Db;
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
            var wfConn = new DataCapture.Workflow.Yeti.Connection();

            int before = TestUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, false);
            Assert.GreaterOrEqual(before, 0);

            wfConn.Connect(user.Login);
            int afterConnect = TestUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, true);
            Assert.AreEqual(before + 1, afterConnect);

            wfConn.Disconnect();
            int afterDisconnect = TestUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, false);
            Assert.AreEqual(before, afterDisconnect);
        }

        [Test()]
        public void DisposeDisconnects()
        {
            var dbConn = ConnectionFactory.Create();
            int before = TestUtil.SelectCount(dbConn, Session.TABLE);

            using (var wfConn = new DataCapture.Workflow.Yeti.Connection())
            {
                Assert.IsNotNull(wfConn);
                Console.WriteLine(wfConn);
            }
            int after = TestUtil.SelectCount(dbConn, Session.TABLE);
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

            if (String.IsNullOrEmpty(msg))
            {
                Assert.Fail("connection limit violated but no exception thrown");
            }

        }
    }
}
