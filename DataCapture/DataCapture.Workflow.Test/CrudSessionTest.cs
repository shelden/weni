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

            Session.Insert(dbConn, user);

            int after = DbUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        /*
        [Test()]
        public void CanSelect()
        {
            String login = TestUtil.NextString();
            int limit = TestUtil.RANDOM.Next(2, 100);


            var dbConn = ConnectionFactory.Create();
            User.Insert(dbConn, login, limit);


            User found = User.Select(dbConn, login);

            Assert.AreNotEqual(found, null);
            Assert.AreEqual(found.Login, login);
            Assert.AreEqual(found.LoginLimit, limit);
            Assert.AreNotEqual(found.Id, 0);
        }
        */

        
    }
}
