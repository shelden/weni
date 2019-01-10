using System;
using System.Data;
using DataCapture.Workflow.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class CrudUserTest
    {
        [Test()]
        public void CanInsert()
        {
            String login = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = TestUtil.SelectCount(dbConn, User.TABLE);
            User.Insert(dbConn, login, 1);
            int after = TestUtil.SelectCount(dbConn, User.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        [Test()]
        // if you don't close your reader correctly, you get an
        // error inserting in this order...
        public void CanSelectThenInsert()
        {
            String login = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            var user = User.Select(dbConn, login);
            Assert.AreEqual(user, null); // because login is a random string

            User.Insert(dbConn, login, 99);
            user = User.Select(dbConn, login);

            Assert.AreEqual(user.Login, login);
            Assert.AreEqual(user.LoginLimit, 99);
            Assert.GreaterOrEqual(user.Id, 1);
        }

        [Test()]
        // Now that Insert returns User, not void, let's check some things:
        public void InsertAndSelectEqual()
        {
            String login = TestUtil.NextString();
            int limit = TestUtil.RANDOM.Next(10, 20);
            var dbConn = ConnectionFactory.Create();
            var user = User.Select(dbConn, login);
            Assert.AreEqual(user, null); // because login is a random string

            var inserted = User.Insert(dbConn, login, limit);
            Assert.AreNotEqual(inserted, null);

            var selected = User.Select(dbConn, login);
            Assert.AreNotEqual(selected, inserted);

            // now make sure they have the same attribites.  Prebably
            // better if we overloaded Equals?
            Assert.AreEqual(inserted.Id, selected.Id);
            Assert.AreEqual(inserted.Login, selected.Login);
            Assert.AreEqual(inserted.LoginLimit, selected.LoginLimit);
            Assert.AreEqual(selected.Login, login);
            Assert.AreEqual(selected.LoginLimit, limit);
        }

        [Test()]
        public void CannotInsertDuplicate()
        {
            String login = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = TestUtil.SelectCount(dbConn, User.TABLE);
            User.Insert(dbConn, login, 1);
            int after0 = TestUtil.SelectCount(dbConn, User.TABLE);
            Assert.AreEqual(before + 1, after0);

            // now try and insert again, it should fail:
            string msg = "";
            try
            {
                User.Insert(dbConn, login, 1);
                int after1 = TestUtil.SelectCount(dbConn, User.TABLE);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            Console.WriteLine("expected exception: " + msg);
            Assert.AreNotEqual(msg, "", "expected exception not thrown");
            int after2 = TestUtil.SelectCount(dbConn, User.TABLE);
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

        /*

        [Test()]
        public void CanUpdate()
        {
            String login0 = TestUtil.NextString();
            String login1 = TestUtil.NextString();
            String login2 = TestUtil.NextString();

            int limit0 = TestUtil.RANDOM.Next(10, 100);
            int limit1 = limit0 + 1;
            int limit2 = limit0 - 1;

            // 1st let's put one in:
            var dbConn = ConnectionFactory.Create();
            User.Insert(dbConn, login0, limit0);
            User found = User.Select(dbConn, login0);
            Assert.AreNotEqual(found, null);

            // now, let's modify the login and update 
            int id = found.Id;
            found.Login = login1;
            //found.Update(dbConn_);

            User foundAgain = User.Select(dbConn, login0);
            User updated = User.Select(dbConn, login1);

            Assert.AreEqual(foundAgain, null);
            Assert.AreEqual(updated.Id, id);
            Assert.AreEqual(updated.Login, login1);
            Assert.AreEqual(updated.LoginLimit, limit0);







        }
*/
    }
}
