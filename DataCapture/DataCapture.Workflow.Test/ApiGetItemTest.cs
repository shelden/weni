using System;
using System.Text;
using System.Collections.Generic;
using DataCapture.Workflow;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class ApiGetItemTest
    {
        #region util
        public static void AssertSame(IDictionary<String, String> left
            , IDictionary<String, String> right
            )
        {
            if (left == null && right == null) return;
            if (left == null && right != null) 
            {
                Assert.Fail("left is null but right has data");
            }
            if (left != null && right == null)
            {
                Assert.Fail("left had data but right is null");
            }

            var msg = new StringBuilder();
            foreach (var key in left.Keys)
            {
                if (!right.ContainsKey(key))
                {
                    msg.Append(", right is missing key [");
                    msg.Append(key);
                    msg.Append("]");
                }
                else if (!left[key].Equals(right[key]))
                {
                    msg.Append(", mismatch in key [");
                    msg.Append(key);
                    msg.Append("]: ");
                    msg.Append(left[key]);
                    msg.Append(" vs ");
                    msg.Append(right[key]);
                }
            }


            foreach (var key in right.Keys)
            {
                if (!left.ContainsKey(key))
                {
                    msg.Append(", left is missing key [");
                    msg.Append(key);
                    msg.Append("]");
                }
                else if (!right[key].Equals(left[key]))
                {
                    msg.Append(", mismatch in key [");
                    msg.Append(key);
                    msg.Append("]: ");
                    msg.Append(left[key]);
                    msg.Append(" vs ");
                    msg.Append(right[key]);
                }
            }

            if (msg.Length >= 2)
            {
                msg.Remove(0, 2); // make pretty by removing the leading ", "
                Console.WriteLine(msg);

                Assert.Fail(msg.ToString());
            }

        }

        #endregion

        [Test()]
        public void CanRetrieve()
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

            AssertSame(pairs, item);

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
            var pairsNeg = TestUtil.CreatePairs();
            var pairsPos = TestUtil.CreatePairs();


            // first put in item with a positive priority
            wfConn.CreateItem(names["map"]
                , itemName + "positive"
                , names["startStep"]
                , pairsPos
                , priority
                );

            // then put in item with a negative priority:
            wfConn.CreateItem(names["map"]
                , itemName + "negative"
                , names["startStep"]
                , pairsNeg
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
            AssertSame(pairsNeg, negative);

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
            AssertSame(pairsPos, positive);

            var nomore = wfConn.GetItem(names["queue"]);
            Assert.IsNull(nomore);
        }
    }
}
