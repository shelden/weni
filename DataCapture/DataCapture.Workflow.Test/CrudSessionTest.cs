using System;
using System.Data;
using DataCapture.Workflow.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class CrudSessionTest
    {
        [Test()]
        public void CanInsert()
        {
            String login = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Session.TABLE);

            User.Insert(dbConn, login, 7);
            var user = User.Select(dbConn, login);

            var session = Session.Insert(dbConn, user);

            int after = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(before + 1, after);
            Assert.GreaterOrEqual(session.Id, 1);
            Assert.AreEqual(session.UserId, user.Id);
            Assert.AreEqual(session.Hostname, Environment.MachineName);
        }

    }
}
