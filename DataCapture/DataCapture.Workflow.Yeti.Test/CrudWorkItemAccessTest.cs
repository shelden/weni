using System;
using DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Yeti.Test
{
    public class CrudWorkItemAccessTest
    {
        [Test()]
        public void CanInsert()
        {
            var dbConn = ConnectionFactory.Create();
            var item = TestUtil.MakeWorkItem(dbConn);
            var good = TestUtil.MakeUser(dbConn);
            var bad = TestUtil.MakeUser(dbConn);
            int before = TestUtil.SelectCount(dbConn, WorkItemAccess.TABLE);

            WorkItemAccess.Insert(dbConn, item, good, true);
            WorkItemAccess.Insert(dbConn, item, bad, false);

            int after = TestUtil.SelectCount(dbConn, WorkItemAccess.TABLE);
            Assert.AreEqual(before + 2, after);
        }

        [Test()]
        public void CannotInsertDuplicate()
        {
            var dbConn = ConnectionFactory.Create();
            var item = TestUtil.MakeWorkItem(dbConn);
            var user = TestUtil.MakeUser(dbConn);
            int before = TestUtil.SelectCount(dbConn, WorkItemAccess.TABLE);

            var access = WorkItemAccess.Insert(dbConn, item, user, true);
            int after0 = TestUtil.SelectCount(dbConn, WorkItemAccess.TABLE);
            Assert.AreNotEqual(access, null);
            Assert.AreEqual(before + 1, after0);

            // now an access exists.  If we try to insert the same user,
            // different, value, it should throw:

            String msg = "";
            try
            {
                WorkItemAccess.Insert(dbConn, item, user, false);
            }
            catch(Exception ex)
            {
                msg = ex.Message;
            }
            Console.WriteLine("expected exception: " + msg);
            Assert.AreNotEqual(msg, "", "expected exception not thrown");

            int after1 = TestUtil.SelectCount(dbConn, WorkItemAccess.TABLE);
            Assert.AreEqual(before + 1, after0);

        }

        [Test()]
        public void InsertThenSelectMatch()
        {
            var dbConn = ConnectionFactory.Create();
            var item = TestUtil.MakeWorkItem(dbConn);
            var user = TestUtil.MakeUser(dbConn);
            bool value = false;  //TestUtil.RANDOM.Next(0, 1) == 0;
            Assert.AreNotEqual(item, null);

            var inserted = WorkItemAccess.Insert(dbConn, item, user, value);
            Assert.AreNotEqual(inserted, null);

            var selected = WorkItemAccess.Select(dbConn, item.Id, user.Id);
            Assert.AreNotEqual(selected, null);
            Assert.GreaterOrEqual(inserted.Id, 1);

            Assert.AreNotEqual(selected, null);
            Assert.GreaterOrEqual(inserted.Id, 1);
            Assert.AreEqual(inserted.Id, selected.Id);
            Console.WriteLine(inserted.ToString() + " vs " + selected.ToString());
            Assert.AreEqual(inserted.UserId, selected.UserId);
            Assert.AreEqual(inserted.IsAllowed, selected.IsAllowed);
            Assert.AreEqual(inserted.IsAllowed, value);
        }
    }
}
