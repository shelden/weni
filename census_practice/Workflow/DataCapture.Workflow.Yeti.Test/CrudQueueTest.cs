using System;
using System.Collections.Generic;
using DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Yeti.Test
{
    public class CrudQueueTest
    {
        [Test()]
        public void CanInsert()
        {
            String regular = TestUtil.NextString();
            String fail = TestUtil.NextString();
            String unspec = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();

            int before = TestUtil.SelectCount(dbConn, Queue.TABLE);
            Queue.Insert(dbConn, regular, true);
            int after = TestUtil.SelectCount(dbConn, Queue.TABLE);
            Assert.AreEqual(before + 1, after);

            Queue.Insert(dbConn, fail, false);
            after = TestUtil.SelectCount(dbConn, Queue.TABLE);
            Assert.AreEqual(before + 2, after);

            Queue.Insert(dbConn, unspec);
            after = TestUtil.SelectCount(dbConn, Queue.TABLE);
            Assert.AreEqual(before + 3, after);
        }

        [Test()]
        public void CanSelectThenInsert()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            var queue = Queue.Select(dbConn, name);
            Assert.AreEqual(queue, null, "randomly named queue" + name + " found in DB which is pretty unlikely"); 

            Queue.Insert(dbConn, name);
            queue = Queue.Select(dbConn, name);

            Assert.AreEqual(queue.Name, name);
            Assert.AreEqual(queue.IsFail, false);
            Assert.AreEqual(queue.IsNormal, true);
            Assert.GreaterOrEqual(queue.Id, 1);
        }

        [Test()]
        public void InsertAndSelectEqual()
        {
            String regular = TestUtil.NextString();
            String fail = TestUtil.NextString();
            String unspec = TestUtil.NextString();
            var expected = new Dictionary<String, bool>
            {
                { "reg" + regular, false }
                , { "fail" + fail, true }
                , { "unspec" + unspec, false }
            };

            var dbConn = ConnectionFactory.Create();

            foreach (var item in expected)
            {
                var before = Queue.Select(dbConn, item.Key);
                Assert.AreEqual(before, null);

                Queue inserted = null;
                if (item.Key == unspec)
                {
                    inserted = Queue.Insert(dbConn, item.Key);
                }
                else
                {
                    inserted = Queue.Insert(dbConn, item.Key, item.Value);
                }
                Assert.AreNotEqual(inserted, null);

                var selected = Queue.Select(dbConn, item.Key);
                Assert.AreNotEqual(selected, null);

                // now make sure they have the same attributes.  
                Assert.AreEqual(inserted.Id, selected.Id);
                Assert.AreEqual(inserted.Name, selected.Name);
                Assert.AreEqual(inserted.IsNormal, selected.IsNormal);
                Assert.AreEqual(inserted.IsFail, selected.IsFail);
                Assert.AreEqual(selected.Name, item.Key);
                Assert.AreEqual(selected.IsNormal, !item.Value);
                Assert.AreEqual(selected.IsFail, item.Value);
            }
        }

        [Test()]
        public void CannotInsertDuplicate()
        {
            String name = "QueueDup" + TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = TestUtil.SelectCount(dbConn, Queue.TABLE);
            Queue.Insert(dbConn, name);
            int after0 = TestUtil.SelectCount(dbConn, Queue.TABLE);
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

            Console.WriteLine("expected exception: " + msg);
            Assert.AreNotEqual(msg, "", "expected exception not thrown");
            Assert.That(msg.Contains("[" + name + "]"), "exception message should contain bad queue");
            int after2 = TestUtil.SelectCount(dbConn, Queue.TABLE);
            Assert.AreEqual(after2, before + 1);
        }

        [Test()]
        public void CanSelect()
        {
            String regular = TestUtil.NextString();
            String fail = TestUtil.NextString();
            String unspecifed = TestUtil.NextString();
            var expected = new Dictionary<String, bool>
            {
                { regular, false }
                , { fail, true }
                , { unspecifed, false }
            };

            var dbConn = ConnectionFactory.Create();
            Queue.Insert(dbConn, regular, false);
            Queue.Insert(dbConn, fail, true);
            Queue.Insert(dbConn, unspecifed);


            foreach (var item in expected)
            {
                Queue found = Queue.Select(dbConn, item.Key);

                Assert.AreNotEqual(found, null);
                Assert.AreEqual(found.Name, item.Key);
                Assert.AreEqual(found.IsNormal, !item.Value);
                Assert.AreEqual(found.IsFail, item.Value);
                Assert.GreaterOrEqual(found.Id, 1);
            }
        }

    }
}
