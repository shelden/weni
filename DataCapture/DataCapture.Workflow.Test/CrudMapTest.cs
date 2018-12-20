using System;
using DataCapture.Workflow.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class CrudMapTest
    {
        [Test()]
        public void CanInsert()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Map.TABLE);
            Map.Insert(dbConn, name, 1);
            int after = DbUtil.SelectCount(dbConn, Map.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        [Test()]
        // if you don't close your reader correctly, you get an
        // error inserting in this order...
        public void CanSelectThenInsert()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            var map = Map.Select(dbConn, name);
            Assert.AreEqual(map, null); // because login is a random string

            Map.Insert(dbConn, name);
            map = Map.Select(dbConn, name);

            Assert.AreEqual(map.Name, name);
            Assert.AreEqual(map.Version, Map.VERSION);
            Assert.GreaterOrEqual(map.Id, 1);
        }

        [Test()]
        public void InsertAndSelectEqual()
        {
            String name = TestUtil.NextString();
            int version = TestUtil.RANDOM.Next(10, 20);
            var dbConn = ConnectionFactory.Create();
            var user = Map.Select(dbConn, name);
            Assert.AreEqual(user, null); // because login is a random string

            var inserted = Map.Insert(dbConn, name, version);
            Assert.AreNotEqual(inserted, null);

            var selected = Map.Select(dbConn, name);
            Assert.AreNotEqual(selected, inserted);

            // now make sure they have the same attribites.  Prebably
            // better if we overloaded Equals?
            Assert.AreEqual(inserted.Id, selected.Id);
            Assert.AreEqual(inserted.Name, selected.Name);
            Assert.AreEqual(inserted.Version, selected.Version);
            Assert.AreEqual(selected.Name, name);
            Assert.AreEqual(selected.Version, version);
        }


        [Test()]
        public void CannotInsertDuplicate()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Map.TABLE);
            Map.Insert(dbConn, name, 1);
            int after0 = DbUtil.SelectCount(dbConn, Map.TABLE);
            Assert.AreEqual(before + 1, after0);

            // now try and insert again, it should fail:
            string msg = "";
            try
            {
                Map.Insert(dbConn, name, 1);
                int after1 = DbUtil.SelectCount(dbConn, Map.TABLE);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            Assert.AreNotEqual("", msg);
            Console.WriteLine("expected exception: " + msg);
            int after2 = DbUtil.SelectCount(dbConn, Map.TABLE);
            Assert.AreEqual(after2, before + 1);
        }

        [Test()]
        public void CanSelect()
        {
            String login = TestUtil.NextString();
            int limit = TestUtil.RANDOM.Next(2, 100);


            var dbConn = ConnectionFactory.Create();
            User.Insert(dbConn, login, limit);


            User found = User.Select(dbConn, login);

            Assert.AreNotEqual(found, null);
            Assert.AreEqual(found.Login, login);
            Assert.AreEqual(found.LoginLimit, limit);
            Assert.AreNotEqual(found.Id, 0);
        }

        
    }
}
