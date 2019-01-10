    using System;
    using System.Collections.Generic;
    using DataCapture.Workflow.Db;
    using NUnit.Framework;

    namespace DataCapture.Workflow.Test
    {
        public class ApiGetItemTest
        {
            [Test()]
            public void CanRetrieve()
            {
                DateTime start = DateTime.UtcNow;
                String itemName = "item" + TestUtil.NextString();
                int priority = TestUtil.RANDOM.Next(-100, 100);
                var wfConn = TestUtil.CreateConnected();
                var names = TestUtil.CreateBasicMap();
                wfConn.CreateItem(names["map"]
                    , itemName
                    , names["startStep"]
                    , null
                    , priority
                    );

                DateTime post = DateTime.UtcNow;


                var item = wfConn.GetItem(names["queue"]);
                Assert.IsNotNull(item);
                Assert.Greater(item.Id, 0);
                Assert.Less(start, item.Created);
                Assert.Less(start, item.Entered);
                Assert.Greater(post, item.Created);
                Assert.AreEqual(item.MapName, names["map"]);
                Assert.AreEqual(item.StepName, names["startStep"]);
                Assert.AreEqual(item.Name, itemName);
                Assert.AreEqual(item.Priority, priority);

                var item2 = wfConn.GetItem(names["queue"]);
                Assert.IsNull(item2);

            }

            [Test()]
            public void CanRetrieveInPriorityOrder()
            {
                DateTime start = DateTime.UtcNow;
                String itemName = "item" + TestUtil.NextString();
                int priority = TestUtil.RANDOM.Next(1, 100);
                var wfConn = TestUtil.CreateConnected();
                var names = TestUtil.CreateBasicMap();


                // first put in item with a positive priority
                wfConn.CreateItem(names["map"]
                    , itemName + "positive"
                    , names["startStep"]
                    , null
                    , priority
                    );

                // then put in item with a negative priority:
                wfConn.CreateItem(names["map"]
                    , itemName + "negative"
                    , names["startStep"]
                    , null
                    , -priority
                );

                DateTime post = DateTime.UtcNow;

                // the negative priority-item should be retrieved first

                var negative = wfConn.GetItem(names["queue"]);
                Assert.IsNotNull(negative);
                Assert.Greater(negative.Id, 0);
                Assert.Less(start, negative.Created);
                Assert.Less(start, negative.Entered);
                Assert.Greater(post, negative.Created);
                Assert.AreEqual(negative.MapName, names["map"]);
                Assert.AreEqual(negative.StepName, names["startStep"]);
                Assert.AreEqual(negative.Name, itemName + "negative");
                Assert.AreEqual(negative.Priority, -priority);

                // followed by the positive one:

                var positive = wfConn.GetItem(names["queue"]);
                Assert.IsNotNull(positive);
                Assert.Greater(positive.Id, 0);
                Assert.Less(start, positive.Created);
                Assert.Less(start, positive.Entered);
                Assert.Greater(post, positive.Created);
                Assert.AreEqual(positive.MapName, names["map"]);
                Assert.AreEqual(positive.StepName, names["startStep"]);
                Assert.AreEqual(positive.Name, itemName + "positive");
                Assert.AreEqual(positive.Priority, priority);

                var nomore = wfConn.GetItem(names["queue"]);
                Assert.IsNull(nomore);
            }
        }
    }
