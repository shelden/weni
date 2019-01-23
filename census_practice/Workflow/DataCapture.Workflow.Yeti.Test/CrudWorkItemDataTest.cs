using System;
using DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Yeti.Test
{
    public class CrudWorkItemDataTest
    {
        [Test()]
        public void CanInsert()
        {
            var dbConn = ConnectionFactory.Create();
            var item = TestUtil.MakeWorkItem(dbConn);
            int before = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);

            WorkItemData.Insert(dbConn, item, "variable0", TestUtil.NextString());

            int after = TestUtil.SelectCount(dbConn, WorkItemData.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        [Test()]
        public void CanSelectThenInsert()
        {
            String key = "test";
            String value = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();

            var item = TestUtil.MakeWorkItem(dbConn);
            Assert.AreNotEqual(item, null);

            // there should be no data (aka key value pairs) now:
            var list = WorkItemData.SelectAll(dbConn, item.Id);
            Assert.AreNotEqual(list, null); // ensure SelectAll returns empty list, not null
            Assert.AreEqual(list.Count, 0);

            // now put one key/value pair in:
            var inserted = WorkItemData.Insert(dbConn, item, key, value);

            var selected = WorkItemData.SelectAll(dbConn, item.Id);

            Assert.AreNotEqual(selected, null); // ensure SelectAll returns empty list, not null
            Assert.AreEqual(selected.Count, 1);
            Assert.GreaterOrEqual(inserted.Id, 1);
            Assert.AreEqual(inserted.Id, selected[0].Id);
            Assert.AreEqual(inserted.WorkItemId, selected[0].WorkItemId);
            Assert.AreEqual(inserted.VariableName, selected[0].VariableName);
            Assert.AreEqual(inserted.VariableName, key);
            Assert.AreEqual(inserted.VariableValue, selected[0].VariableValue);
        }
    }
}
