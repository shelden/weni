using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using DataCapture.Workflow;
using DataCapture.Workflow.Db; // for DbUtil.SelectCount
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class ApiAddItemTest
    {
        #region util
        static Dictionary<String, String> CreateBasicMap()
        {
            var dbConn = ConnectionFactory.Create();

            var map = TestUtil.MakeMap(dbConn);
            Console.WriteLine(map);

            var queue = TestUtil.MakeQueue(dbConn);
            Console.WriteLine(queue);

            var end = Step.Insert(dbConn
                , "End" + TestUtil.NextString()
                , map
                , queue
                , Step.StepType.Terminating
             );

            var start = Step.Insert(dbConn
                , "Start" + TestUtil.NextString()
                , map
                , queue
                , end
                , Step.StepType.Start
                );
                
            Console.WriteLine(start);
            Console.WriteLine(end);


            return new Dictionary<String, String>() {
                {  "queue", queue.Name }
                , { "map", map.Name }
                , {  "startStep", start.Name }
                , {  "endStep", end.Name }
            };
        }

        static Dictionary<String, String> CreatePairs()
        {
            Dictionary<String, String> tmp = new Dictionary<String, String>();
            int count = TestUtil.RANDOM.Next(1, 5);
            for (int i = 0; i < count; i++)
            {
                tmp.Add("key" + i + "of" + count, TestUtil.NextString());
            }
            return tmp;
        }
        #endregion

        [Test()]
        public void CanCreateItem()
        {
            var dbConn = ConnectionFactory.Create();
            String itemName = "Item" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(-100, 100);
            int beforeItems = DbUtil.SelectCount(dbConn, WorkItem.TABLE);
            int beforeNvps = DbUtil.SelectCount(dbConn, WorkItemData.TABLE);

            var user = TestUtil.MakeUser(ConnectionFactory.Create());
            var wfConn = new Connection();
            wfConn.Connect(user.Login);
            var where = CreateBasicMap();
            var inboundPairs = CreatePairs();
            wfConn.CreateItem(where["map"]
                , itemName
                , where["startStep"]
                , inboundPairs
                , priority
                );

            int afterItems = DbUtil.SelectCount(dbConn, WorkItem.TABLE);
            int afterNvps = DbUtil.SelectCount(dbConn, WorkItemData.TABLE);

            Assert.AreEqual(beforeItems + 1, afterItems);
            Assert.Greater(inboundPairs.Count, 0);
            Assert.AreEqual(beforeNvps + inboundPairs.Count, afterNvps);

            var item = WorkItem.Select(dbConn, itemName);
            Assert.IsNotNull(item);
            Assert.AreEqual(item.Priority, priority);
            Assert.Greater(item.Id, 0);

            var selectedPairs = WorkItemData.SelectAll(dbConn, item.Id);
            Assert.IsNotNull(selectedPairs);
            Assert.GreaterOrEqual(selectedPairs.Count, 1);
            Assert.AreEqual(selectedPairs.Count, inboundPairs.Count);
            foreach(var row in selectedPairs)
            {
                String key = row.VariableName;
                String value = row.VariableValue;
                Console.WriteLine(key + ") " + inboundPairs[key] + " vs " + value);
                Assert.AreEqual(inboundPairs[key], value);
            }
        }
    }
}
