﻿using System;
using LM.DataCapture.Workflow.Yeti;
using NUnit.Framework;

namespace LM.DataCapture.Workflow.Yeti.Test
{
    public class ApiGetItemTest
    {
        [Test()]
        public void CanRetrieve()
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
            var item = wfConn.GetItem(names["queue"]);
            DateTime post = TestUtil.FlooredNow();
            TestUtil.AssertSame(item, itemName, pairs, start, priority);
            TestUtil.AssertRightPlaces(item, names["map"], names["startStep"]);
        }

        [Test()]
        public void CanRetrieveInPriorityOrder()
        {
            DateTime start = TestUtil.FlooredNow();
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


            // the negative priority-item should be retrieved first

            var negative = wfConn.GetItem(names["queue"]);

            TestUtil.AssertSame(negative, itemName + "negative", pairsNeg, start, -priority);
            TestUtil.AssertRightPlaces(negative, names["map"], names["startStep"]);

            // followed by the positive one:

            var positive = wfConn.GetItem(names["queue"]);
            TestUtil.AssertSame(positive, itemName + "positive", pairsPos, start, priority);
            TestUtil.AssertRightPlaces(positive, names["map"], names["startStep"]);

            var nomore = wfConn.GetItem(names["queue"]);
            Assert.IsNull(nomore);
        }

        [Test()]
        public void PriorityTiesResolvedByTime()
        {
            DateTime pretime = TestUtil.FlooredNow();
            String itemName0 = "early" + TestUtil.NextString();
            String itemName1 = "late" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(1, 100);
            var wfConn = TestUtil.CreateConnected();
            var names = TestUtil.CreateBasicMap();
            var pairs0 = TestUtil.CreatePairs();
            var pairs1 = TestUtil.CreatePairs();

            // first put in two items with same priority, but
            // different attributes
            wfConn.CreateItem(names["map"]
                , itemName0
                , names["startStep"]
                , pairs0
                , priority
                );
            wfConn.CreateItem(names["map"]
                , itemName1
                , names["startStep"]
                , pairs1
                , priority
            );

            // the earlier item should be retrieved first
            var item0 = wfConn.GetItem(names["queue"]);
            
            TestUtil.AssertSame(item0, itemName0, pairs0, pretime, priority);
            TestUtil.AssertRightPlaces(item0, names["map"], names["startStep"]);

            // followed by the later one:
            var item1 = wfConn.GetItem(names["queue"]);

            TestUtil.AssertSame(item1, itemName1, pairs1, pretime, priority);
            TestUtil.AssertRightPlaces(item1, names["map"], names["startStep"]);

            // followed by none:
            var nomore = wfConn.GetItem(names["queue"]);
            Assert.IsNull(nomore);
        }
    }
}
