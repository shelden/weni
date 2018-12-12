using System;
using System.Data;
using DataCapture.Workflow.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class CrudQueueTest
    {
        [Test()]
        public void CanInsert()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Queue.TABLE);
            Queue.Insert(dbConn, name);
            int after = DbUtil.SelectCount(dbConn, Queue.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        [Test()]
        public void CanSelectThenInsert()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            var queue = Queue.Select(dbConn, name);
            Assert.AreEqual(queue, null); // because login is a random string

            Queue.Insert(dbConn, name);
            queue = Queue.Select(dbConn, name);

            Assert.AreEqual(queue.Name, name);
            Assert.AreEqual(queue.Type, 1);
            Assert.GreaterOrEqual(queue.Id, 1);
        }

        [Test()]
        public void InsertAndSelectEqual()
        {
            String name = TestUtil.NextString();
            int type = 1;
            var dbConn = ConnectionFactory.Create();
            var before = Queue.Select(dbConn, name);
            Assert.AreEqual(before, null);

            Queue inserted = Queue.Insert(dbConn, name);

            Assert.AreNotEqual(inserted, null);

            var selected = Queue.Select(dbConn, name);
            Assert.AreNotEqual(selected, null);

            // now make sure they have the same attributes.  Prebably
            // better if we overloaded Equals?
            Assert.AreEqual(inserted.Id, selected.Id);
            Assert.AreEqual(inserted.Name, selected.Name);
            Assert.AreEqual(inserted.Type, selected.Type);
            Assert.AreEqual(selected.Name, name);
            Assert.AreEqual(selected.Type, type);
        }

        [Test()]
        public void CannotInsertDuplicate()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Queue.TABLE);
            Queue.Insert(dbConn, name);
            int after0 = DbUtil.SelectCount(dbConn, Queue.TABLE);
            Assert.AreEqual(before + 1, after0);

            // now try and insert again, it should fail:
            string msg = "";
            try
            {
                Queue.Insert(dbConn, name);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            Assert.AreNotEqual("", msg);
            Console.WriteLine("expected exception: " + msg);
            int after2 = DbUtil.SelectCount(dbConn, User.TABLE);
            Assert.AreEqual(after2, before + 1);
        }

        [Test()]
        public void CanSelect()
        {
            String name = TestUtil.NextString();

            var dbConn = ConnectionFactory.Create();
            Queue.Insert(dbConn, name);

            Queue found = Queue.Select(dbConn, name);

            Assert.AreNotEqual(found, null);
            Assert.AreEqual(found.Name, name);
            Assert.GreaterOrEqual(found.Id, 1);
        }

    }
}
