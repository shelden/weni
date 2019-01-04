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

            Map.Insert(dbConn, name, Map.VERSION);
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
        public void InsertingDuplicateIncreasesVersion()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Map.TABLE);
            var insertedv1 = Map.InsertWithMaxVersion(dbConn, name);
            int afterv1 = DbUtil.SelectCount(dbConn, Map.TABLE);
            Assert.AreEqual(before + 1, afterv1);
            Assert.AreEqual(insertedv1.Version, Map.VERSION);

            // now try and insert again.  When doing so, the version
            // should increase.
            var insertedv2 = Map.InsertWithMaxVersion(dbConn, name);
            int afterv2 = DbUtil.SelectCount(dbConn, Map.TABLE);


            Assert.AreEqual(afterv2, before + 2);
            Assert.AreEqual(insertedv2.Version, insertedv1.Version + 1);
        }

        [Test()]
        public void CanSelect()
        {
            String name = TestUtil.NextString();

            var dbConn = ConnectionFactory.Create();
            var inserted = Map.Insert(dbConn, name, Map.VERSION);

            var found = Map.Select(dbConn, name);

            Assert.AreNotEqual(found, null);
            //Assert.AreEqual(found.
            //Assert.AreEqual(found.LoginLimit, limit);
            //Assert.AreNotEqual(found.Id, 0);
        }

        
    }
}
