using System;
using System.Collections.Generic;
using DataCapture.Workflow.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class ApiAddItemTest
    {


        [Test()]
        public void CanCreateItem()
        {
            var dbConn = ConnectionFactory.Create();
            String itemName = "Item" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(-100, 100);
            int beforeItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int beforeNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            var wfConn = TestUtil.CreateConnected();

            var where = TestUtil.CreateBasicMap();
            var inboundPairs = TestUtil.CreatePairs();
            wfConn.CreateItem(where["map"]
                , itemName
                , where["startStep"]
                , inboundPairs
                , priority
                );

            int afterItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int afterNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            Assert.AreEqual(beforeItems + 1, afterItems);
            Assert.Greater(inboundPairs.Count, 0);
            Assert.AreEqual(beforeNvps + inboundPairs.Count, afterNvps);

            var item = WorkItem.Select(dbConn, itemName);
            Assert.IsNotNull(item);
            Assert.AreEqual(item.Priority, priority);
            Assert.AreEqual(item.Name, itemName);
            Assert.AreEqual(item.ItemState, WorkItemState.Available);
            Assert.Greater(item.Id, 0);

            var selectedPairs = WorkItemData.SelectAll(dbConn, item.Id);
            Assert.IsNotNull(selectedPairs);
            Assert.GreaterOrEqual(selectedPairs.Count, 1);
            Assert.AreEqual(selectedPairs.Count, inboundPairs.Count);
            foreach (var row in selectedPairs)
            {
                String key = row.VariableName;
                String value = row.VariableValue;
                Assert.AreEqual(inboundPairs[key], value);
            }
        }


        [Test()]
        public void CanCreateEmptyItem()
        {
            var dbConn = ConnectionFactory.Create();
            String itemName = "Item" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(-100, 100);
            int beforeItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int beforeNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            var where = TestUtil.CreateBasicMap();
            var inboundPairs = TestUtil.CreatePairs();
            var wfConn = TestUtil.CreateConnected();
            wfConn.CreateItem(where["map"]
                , itemName
                , where["startStep"]
                , new Dictionary<String, String>()
                , priority
                );

            int afterItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int afterNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            // since we created the item with a blank set of NVPs,
            //afterNvps should not increase:
            Assert.AreEqual(beforeItems + 1, afterItems);
            Assert.AreEqual(beforeNvps + 0, afterNvps);

            var item = WorkItem.Select(dbConn, itemName);
            Assert.IsNotNull(item);
            Assert.AreEqual(item.Priority, priority);
            Assert.Greater(item.Id, 0);
            Assert.AreEqual(item.ItemState, WorkItemState.Available);

            var selectedPairs = WorkItemData.SelectAll(dbConn, item.Id);
            Assert.IsNotNull(selectedPairs);
            Assert.AreEqual(selectedPairs.Count, 0);
        }

        [Test()]
        public void CanCreateItemWithoutPairs()
        {
            var dbConn = ConnectionFactory.Create();
            String itemName = "Item" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(-100, 100);
            int beforeItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int beforeNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            var where = TestUtil.CreateBasicMap();
            var inboundPairs = TestUtil.CreatePairs();
            var wfConn = TestUtil.CreateConnected();
            wfConn.CreateItem(where["map"]
                , itemName
                , where["startStep"]
                , null
                , priority
                );

            int afterItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int afterNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            // since we created the item with a blank set of NVPs,
            //afterNvps should not increase:
            Assert.AreEqual(beforeItems + 1, afterItems);
            Assert.AreEqual(beforeNvps + 0, afterNvps);

            var item = WorkItem.Select(dbConn, itemName);
            Assert.IsNotNull(item);
            Assert.AreEqual(item.Priority, priority);
            Assert.Greater(item.Id, 0);
            Assert.AreEqual(item.ItemState, WorkItemState.Available);

            var selectedPairs = WorkItemData.SelectAll(dbConn, item.Id);
            Assert.IsNotNull(selectedPairs);
            Assert.AreEqual(selectedPairs.Count, 0);
        }

        [Test()]
        public void ItemInMissingStepThrows()
        {
            String itemName = "Item" + TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int beforeItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int beforeNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            var where = TestUtil.CreateBasicMap();
            var wfConn = TestUtil.CreateConnected();

            String bogus = "Bogus." + TestUtil.NextString();
            String msg = "";
            try
            {
                wfConn.CreateItem(where["map"]
                    , itemName
                    , bogus
                    , TestUtil.CreatePairs()
                    );
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            Assert.That(!String.IsNullOrEmpty(msg));
            Assert.That(msg.Contains("step [" + bogus + "]"));

            int afterItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int afterNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            Assert.AreEqual(beforeItems, afterItems);
            Assert.AreEqual(beforeNvps, afterNvps);

        }

        [Test()]
        public void ItemInEndStepThrows()
        {
            String itemName = "Item" + TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int beforeItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int beforeNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            var where = TestUtil.CreateBasicMap();
            var wfConn = TestUtil.CreateConnected();

            String msg = "";
            try
            {
                wfConn.CreateItem(where["map"]
                    , itemName
                    , where["endStep"]
                    , TestUtil.CreatePairs()
                    );
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            Assert.That(!String.IsNullOrEmpty(msg));
            Assert.That(msg.Contains("on start steps"));
            Assert.That(msg.Contains("[" + where["endStep"] + "]"));

            int afterItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int afterNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            Assert.AreEqual(beforeItems, afterItems);
            Assert.AreEqual(beforeNvps, afterNvps);
        }

        [Test()]
        public void ItemInMissingMapThrows()
        {
            String itemName = "Item" + TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int beforeItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int beforeNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            var where = TestUtil.CreateBasicMap();
            var wfConn = TestUtil.CreateConnected();
            String bogus = "Bogus" + TestUtil.NextString();

            String msg = "";
            try
            {
                wfConn.CreateItem(bogus
                    , itemName
                    , where["startStep"]
                    , TestUtil.CreatePairs()
                    );
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            Assert.That(!String.IsNullOrEmpty(msg));
            Assert.That(msg.Contains("No such map"));
            Assert.That(msg.Contains("[" + bogus + "]"));

            int afterItems = TestUtil.SelectCount(dbConn, WorkItem.TABLE);
            int afterNvps = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            Assert.AreEqual(beforeItems, afterItems);
            Assert.AreEqual(beforeNvps, afterNvps);
        }
    }
}
