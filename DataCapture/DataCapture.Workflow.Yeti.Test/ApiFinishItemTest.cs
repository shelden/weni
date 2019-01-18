using System;
using System.Collections.Generic;
using DataCapture.Workflow.Yeti;
using DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Yeti.Test
{
    public class ApiFinishItemTest
    {
        [Test()]
        public void FinishThenFinishGoesAway()
        {
            DateTime start = TestUtil.FlooredNow();
            String itemName = "item" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(-100, 100);
            var wfConn = TestUtil.CreateConnected();
            var names = TestUtil.CreateBasicMap();
            var pairs = TestUtil.CreatePairs();
            wfConn.CreateItem(names["map"]
                , itemName
                , names["startStep"]
                , pairs
                , priority
                );
            var item0 = wfConn.GetItem(names["queue"]);
            TestUtil.AssertSame(item0, itemName, pairs, start, priority);
            TestUtil.AssertRightPlaces(item0, names["map"], names["startStep"]);

            Assert.IsNotNull(item0);

            // now finish the item.  It should move to the
            // end step in our simple map setup:
            wfConn.FinishItem(item0);
            var item1 = wfConn.GetItem(names["queue"]);
            TestUtil.AssertSame(item1, itemName, pairs, start, priority);
            TestUtil.AssertRightPlaces(item1, names["map"], names["endStep"]);

            // now, since we're in an end step, finishing the item should make
            // it go away:
            wfConn.FinishItem(item1);

            var item2 = wfConn.GetItem(names["queue"]);
            Assert.IsNull(item2);

            // make sure everything is gone from the DB, now that the item
            // is complete:

            var dbConn = ConnectionFactory.Create();
            Assert.IsNull(WorkItem.Select(dbConn, item0.Id));
            Assert.IsNull(WorkItem.Select(dbConn, item1.Id));
            Assert.AreEqual(0, WorkItemData.SelectAll(dbConn, item0.Id).Count);
            Assert.AreEqual(0, WorkItemData.SelectAll(dbConn, item1.Id).Count);
        }

    }
}

