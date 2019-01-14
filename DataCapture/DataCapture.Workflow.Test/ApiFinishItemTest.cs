using System;
using System.Text;
using System.Collections.Generic;
using DataCapture.Workflow.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class ApiFinishItemTest
    {
        [Test()]
        public void FinishThenFinishGoesAway()
        {
            DateTime start = DateTime.UtcNow;
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
            DateTime post = DateTime.UtcNow;
            TestUtil.AssertSame(item0, itemName, pairs, start, post, priority);
            TestUtil.AssertRightPlaces(item0, names["map"], names["startStep"]);

            Assert.IsNotNull(item0);

            // now finish the item.  It should move to the
            // end step in our simple map setup:
            // XXX; ideally we'd change some values and they'd move thru
            wfConn.FinishItem(item0);
            var item1 = wfConn.GetItem(names["queue"]);
            post = DateTime.UtcNow;
            TestUtil.AssertSame(item1, itemName, pairs, start, post, priority);
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


        [Test()]
        public void CanModifyWorkItem()
        {
            DateTime before = DateTime.UtcNow;
            var itemName = TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(-100, 100);
            var wfConn = TestUtil.CreateConnected();
            var names = TestUtil.CreateBasicMap();
            var startPairs = new Dictionary<String, String>()
            {
                { "foo", "1"}
                , { "bar", "2"}
            };
            var endPairs = new Dictionary<String, String>()
            {
                { "New York", "Giants"}
                , { "Washington", "Redskins"}
                , { "London", "Monarchs"}
            };
            wfConn.CreateItem(names["map"]
                , itemName
                , names["startStep"]
                , startPairs
                , priority
                );

            var modifyMe = wfConn.GetItem(names["queue"]);
            DateTime after = DateTime.UtcNow;
            TestUtil.AssertSame(modifyMe, itemName, startPairs, before, after, priority);
            TestUtil.AssertRightPlaces(modifyMe, names["map"], names["startStep"]);

            // now let's change things in the workitem:
            modifyMe.Remove("foo");
            modifyMe.Remove("bar");
            modifyMe.Add("New York", endPairs["New York"]);
            modifyMe.Add("London", endPairs["London"]);
            modifyMe.Add("Washington", endPairs["Washington"]);
            modifyMe.Priority = 7;
            modifyMe.Name = "changed" + itemName;

            // and finish it.  The new values should be updated:
            wfConn.FinishItem(modifyMe);

            var hopefullyModified = wfConn.GetItem(names["queue"]);

            after = DateTime.UtcNow;

            TestUtil.AssertSame(hopefullyModified, "changed" + itemName, endPairs, before, after, 7);
            TestUtil.AssertRightPlaces(hopefullyModified, names["map"], names["endStep"]);
        }
    }
}

