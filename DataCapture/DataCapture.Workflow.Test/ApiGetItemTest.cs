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

            var item = wfConn.GetItem(names["queue"]);
            Assert.IsNotNull(item);
            Assert.Greater(item.Id, 0);
            //TestUtil.AssertCloseEnough(start, item.Created);
            //TestUtil.AssertCloseEnough(start, item.Entered);
            //Assert.AreEqual(item.MapName, names["map"]);
            //Assert.AreEqual(item.StepName, names["startStep"]);
            Console.WriteLine("Name; " + item.Name + " vs " + itemName);
            Assert.AreEqual(item.Name, itemName);
            Assert.AreEqual(item.Priority, priority);
        }

    }
}
