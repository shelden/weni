﻿using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using DataCapture.Workflow.Yeti;
using DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Yeti.Test
{
    public static class TestUtil
    {
        #region randomness
        public static readonly Random RANDOM = new Random();
        public static String NextString(int length = 10)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                char letter = 'A';
                if (RANDOM.Next(0, 2) == 0)
                {
                    letter = 'a';
                }
                int offset = RANDOM.Next(0, 26);
                sb.Append((char)(letter + offset));
            }
            return sb.ToString();
        }
        #endregion

        #region Insert Random
        // some utility methods to insert random things like
        // queues, steps, etc.  Sometimes you can't insert, for example,
        // a rule without creating a Step.  These make that easier.
        public static Queue MakeQueue(IDbConnection dbConn)
        {
            String queueName = "Queue" + TestUtil.NextString();
            return Queue.Insert(dbConn, queueName, (TestUtil.RANDOM.Next() % 2 == 0));
        }
        public static Map MakeMap(IDbConnection dbConn)
        {
            String mapName = "Map" + TestUtil.NextString();
            return Map.InsertWithMaxVersion(dbConn, mapName);
        }
        public static Step MakeStep(IDbConnection dbConn)
        {
            String stepName = "Step" + TestUtil.NextString();
            Step.StepType type = Step.StepType.Terminating;
            switch (RANDOM.Next(0, 4))
            {
                case 0:
                    type = Step.StepType.Failure;
                    break;
                case 1:
                    type = Step.StepType.Failure;
                    break;
                case 2:
                    type = Step.StepType.Start;
                    break;
                default:
                    break;
            }
            return Step.Insert(dbConn
                , stepName
                , MakeMap(dbConn)
                , MakeQueue(dbConn)
                , type
                );
        }
        public static User MakeUser(IDbConnection dbConn)
        {
            String userName = "User" + TestUtil.NextString();
            return User.Insert(dbConn, userName, RANDOM.Next(10, 20));
        }
        public static Session MakeSession(IDbConnection dbConn)
        {
            return Session.Insert(dbConn, MakeUser(dbConn));
        }
        public static WorkItem MakeWorkItem(IDbConnection dbConn)
        {
            String workItemName = "Item" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(-100, 100);
            var step = TestUtil.MakeStep(dbConn);
            var session = TestUtil.MakeSession(dbConn);
            return WorkItem.Insert(dbConn, step, workItemName, priority, session);
        }
        #endregion

        #region Approximately Equal
        public static void AssertCloseEnough(DateTime a, DateTime b)
        {
            String FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

            var astring = a.ToString(FORMAT);
            var bstring = b.ToString(FORMAT);
            if (astring != bstring)
            {
                String msg = "Expected DateTimes to be the same: "
                    + astring
                    + " vs "
                    + bstring
                    ;
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }
        #endregion

        #region Select Count
        // Utility function (mostly for unit tests) to do a select count * from
        // table.  If you're writing a GUI, you should use parameters hanging
        // off your DataReader, as opposed to this method.
        public static int SelectCount(IDbConnection dbConn, string table)
        {
            var command = dbConn.CreateCommand();
            var sql = new StringBuilder();
            sql.Append("SELECT COUNT(1) FROM ");
            sql.Append(table);
            command.CommandText = sql.ToString();
            return Convert.ToInt32(command.ExecuteScalar());
        }
        #endregion

        #region WF Connection Helpers
        public static Connection CreateConnected()
        {
            var user = TestUtil.MakeUser(ConnectionFactory.Create());
            var wfConn = new Connection();
            wfConn.Connect(user.Login);
            return wfConn;
        }
        public static Dictionary<String, String> CreateBasicMap()
        {
            var dbConn = ConnectionFactory.Create();

            var map = TestUtil.MakeMap(dbConn);
            var queue = TestUtil.MakeQueue(dbConn);
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

            return new Dictionary<String, String>() {
                    {  "queue", queue.Name }
                    , { "map", map.Name }
                    , {  "startStep", start.Name }
                    , {  "endStep", end.Name }
                };
        }

        public static Dictionary<String, String> CreatePairs()
        {
            Dictionary<String, String> tmp = new Dictionary<String, String>();
            int count = TestUtil.RANDOM.Next(1, 5);
            for (int i = 0; i < count; i++)
            {
                tmp.Add("clef" + (i + 1) + "of" + count, TestUtil.NextString());
            }
            return tmp;
        }

        /// <summary>
        /// Asserts that two things that implement IDictionary have the
        /// same key value pairs.  I.e. a WorkItemItem since it implements
        /// IDictionary.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        public static void AssertSame(IDictionary<String, String> left
            , IDictionary<String, String> right
            )
        {
            if (left == null && right == null) return;
            if (left == null && right != null)
            {
                NUnit.Framework.Assert.Fail("left is null but right has data");
            }
            if (left != null && right == null)
            {
                NUnit.Framework.Assert.Fail("left had data but right is null");
            }

            var msg = new StringBuilder();
            foreach (var key in left.Keys)
            {
                if (!right.ContainsKey(key))
                {
                    msg.Append(", right is missing key [");
                    msg.Append(key);
                    msg.Append("]");
                }
                else if (!left[key].Equals(right[key]))
                {
                    msg.Append(", mismatch in key [");
                    msg.Append(key);
                    msg.Append("]: ");
                    msg.Append(left[key]);
                    msg.Append(" vs ");
                    msg.Append(right[key]);
                }
            }


            foreach (var key in right.Keys)
            {
                if (!left.ContainsKey(key))
                {
                    msg.Append(", left is missing key [");
                    msg.Append(key);
                    msg.Append("]");
                }
                else if (!right[key].Equals(left[key]))
                {
                    msg.Append(", mismatch in key [");
                    msg.Append(key);
                    msg.Append("]: ");
                    msg.Append(left[key]);
                    msg.Append(" vs ");
                    msg.Append(right[key]);
                }
            }

            if (msg.Length >= 2)
            {
                msg.Remove(0, 2); // make pretty by removing the leading ", "
                Console.WriteLine(msg);

                Assert.Fail(msg.ToString());
            }

        }

        /// <summary>
        /// Assert that a work item has all its expected values.
        /// </summary>
        /// <param name="item">the work item returned by the API</param>
        /// <param name="before">a UTC timestamp before the item was entered</param>
        /// <param name="after">a UTC timestamp after the item was returned</param>
        /// <param name="expectedPairs">The Key-value pairs we expect to be in the
        /// work item</param>
        public static void AssertSame(WorkItemInfo item
            , String expectedItemName
            , IDictionary<String, String> expectedPairs
            , DateTime before
            , DateTime after
            , int expectedPriority
            )
        {
            Assert.IsNotNull(item);
            Assert.Greater(item.Id, 0);
            Assert.AreEqual(item.State, WorkItemState.InProgress); // the other states can't really be in a work item info; GetItem() doesn't return them
            Assert.AreEqual(item.Name, expectedItemName);

            Console.WriteLine(before.ToString(DbUtil.FORMAT));
            Console.WriteLine(" " + item.Created.ToString(DbUtil.FORMAT));
            Console.WriteLine("  " + item.Entered.ToString(DbUtil.FORMAT));
            Console.WriteLine("   " + after.ToString(DbUtil.FORMAT));

            Assert.GreaterOrEqual(item.Created, before);
            Assert.GreaterOrEqual(item.Entered, before);
            Assert.LessOrEqual(item.Created, item.Entered);
            Assert.GreaterOrEqual(after, item.Created);
            Assert.GreaterOrEqual(after, item.Entered);

            Assert.AreEqual(item.Priority, expectedPriority);
            AssertSame(expectedPairs, item);
        }

        /// <summary>
        /// Utility method to assert that a work item is in
        /// the right step and map after we've processed it.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="map">name of map</param>
        /// <param name="step">name of step</param>
        public static void AssertRightPlaces(WorkItemInfo item
            , String map
            , String step
            )
        {
            Assert.IsNotNull(item);
            Assert.AreEqual(item.StepName, step);
            Assert.AreEqual(item.MapName, map);
}
        #endregion
    }
}
