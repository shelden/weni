﻿using System;
using System.Collections.Generic;
using LM.DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace LM.DataCapture.Workflow.Yeti.Test
{
    public class CrudSessionTest
    {
        [Test()]
        public void CanInsert()
        {
            var dbConn = ConnectionFactory.Create();
            int before = TestUtil.SelectCount(dbConn, Session.TABLE);

            var user = TestUtil.MakeUser(dbConn);
            var session = Session.Insert(dbConn, user);

            int after = TestUtil.SelectCount(dbConn, Session.TABLE);
            Assert.AreEqual(before + 1, after);
            Assert.GreaterOrEqual(session.Id, 1);
            Assert.AreEqual(session.UserId, user.Id);
            Assert.AreEqual(session.Hostname, Environment.MachineName.ToLower());
        }

        [Test()]
        public void InsertAndSelectEqual()
        {
            DateTime start = TestUtil.FlooredNow();
            var dbConn = ConnectionFactory.Create();
            var user = TestUtil.MakeUser(dbConn);

            // first there should be no sessions for that user:
            var list = Session.SelectAll(dbConn, user);
            Assert.AreNotEqual(list, null);
            Assert.AreEqual(list.Count, 0);

            int count = TestUtil.RANDOM.Next(2, 5);

            for (int i = 0; i < count; i++)
            {
                var session = Session.Insert(dbConn, user);
            }

            // now there should be 2-5 sessions for that user.
            // they should have different ids; same hosts, and 
            // timestamps approximately the same as when this 
            // test started:
            list = Session.SelectAll(dbConn, user);
            Assert.AreNotEqual(list, null);
            Assert.AreEqual(list.Count, count);

            var ids = new HashSet<int>();
            for(int i = 0; i < count; i++)
            {
                Assert.AreEqual(list[i].Hostname, Environment.MachineName.ToLower());
                Assert.GreaterOrEqual(list[i].StartTime, start);
                Assert.GreaterOrEqual(list[i].Id, 1);
                Assert.That(!ids.Contains(list[i].Id));
                ids.Add(list[i].Id);
            }
        }
    }
}
