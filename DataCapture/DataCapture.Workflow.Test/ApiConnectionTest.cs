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
            var wfConn = new DataCapture.Workflow.Connection();

            int before = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, false);
            Assert.GreaterOrEqual(before, 0);

            wfConn.Connect();
            int afterConnect = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, true);
            Assert.AreEqual(before + 1, afterConnect);

            wfConn.Disconnect();
            int afterDisconnect = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(wfConn.IsConnected, false);
            Assert.AreEqual(before, afterDisconnect);
        }
    }
}
