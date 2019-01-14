using System;
using DataCapture.Workflow.Yeti;
using DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class CrudWorkItemTest
    {
        [Test()]
        public void CanInsert()
        {
            String workItemName = TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(1, 100);
            int state = TestUtil.RANDOM.Next(1, 100);

            var dbConn = ConnectionFactory.Create();
            int before = TestUtil.SelectCount(dbConn, WorkItem.TABLE);

            var step = TestUtil.MakeStep(dbConn);
            var session = TestUtil.MakeSession(dbConn);
            var item = WorkItem.Insert(dbConn, step, workItemName, priority, session);

            int after = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        [Test()]
        // if you don't close your reader correctly, you get an
        // error inserting in this order...
        public void CanSelectThenInsert()
        {
            DateTime startup = DateTime.UtcNow;
            String name = TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(1, 100);
            var dbConn = ConnectionFactory.Create();
            var item = WorkItem.Select(dbConn, name);
            Assert.AreEqual(item, null);
             
            var step = TestUtil.MakeStep(dbConn);
            var session = TestUtil.MakeSession(dbConn);
            var inserted = WorkItem.Insert(dbConn, step, name, priority, session);
            var selected = WorkItem.Select(dbConn, name);
            
            // first, is inserted rational:
            Assert.GreaterOrEqual(inserted.Id, 1);
            Assert.AreEqual(inserted.Name, name);
            Assert.AreEqual(inserted.StepId, step.Id);
            Assert.AreEqual(inserted.SessionId, session.Id);
            Assert.AreEqual(inserted.Priority, priority);
            Assert.AreEqual(inserted.ItemState, WorkItemState.Available);
            Assert.GreaterOrEqual(inserted.Created, startup);
            Assert.GreaterOrEqual(inserted.Entered, startup);
            Assert.AreEqual(inserted.Entered, inserted.Created);

            // next do they match when pulled out of the DB:
            Assert.AreEqual(inserted.Id, selected.Id);
            Assert.AreEqual(inserted.Name, selected.Name);
            Assert.AreEqual(inserted.StepId, selected.StepId);
            Assert.AreEqual(inserted.SessionId, selected.SessionId);
            Assert.AreEqual(inserted.Priority, selected.Priority);
            Assert.AreEqual(inserted.ItemState, selected.ItemState);

            TestUtil.AssertCloseEnough(inserted.Created, selected.Created);
            TestUtil.AssertCloseEnough(inserted.Entered, selected.Entered);
        }
    }
}
