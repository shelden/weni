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
            int before = DbUtil.SelectCount(dbConn, User.TABLE);
            User.Insert(dbConn, login, 1);
            int after = DbUtil.SelectCount(dbConn, User.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        [Test()]
        public void CannotInsertDuplicate()
        {
            String login = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, User.TABLE);
            User.Insert(dbConn, login, 1);
            int after0 = DbUtil.SelectCount(dbConn, User.TABLE);
            Assert.AreEqual(before + 1, after0);

            // now try and insert again, it should fail:
            string msg = "";
            try
            {
                User.Insert(dbConn, login, 1);
                int after1 = DbUtil.SelectCount(dbConn, User.TABLE);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            Assert.AreNotEqual("", msg);
            Console.WriteLine("expected exception: " + msg);
            //Assert.AreNotEqual(msg.IndexOf("duplicate"), -1);
            int after2 = DbUtil.SelectCount(dbConn, User.TABLE);
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

