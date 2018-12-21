using System;
using System.Text;
using System.Data;
using DataCapture.Workflow.Db;

namespace DataCapture.Workflow.Test
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
            String queueName = TestUtil.NextString();
            return Queue.Insert(dbConn, queueName, (TestUtil.RANDOM.Next() % 2 == 0));
        }
        public static Map MakeMap(IDbConnection dbConn)
        {
            String mapName = TestUtil.NextString();
            return Map.Insert(dbConn, mapName);
        }
        public static Step MakeStep(IDbConnection dbConn)
        {
            String stepName = TestUtil.NextString();
            return Step.Insert(dbConn, stepName, MakeMap(dbConn), MakeQueue(dbConn), 29);
        }
        public static User MakeUser(IDbConnection dbConn)
        {
            String userName = TestUtil.NextString();
            return User.Insert(dbConn, userName, 10);
        }
        public static Session MakeSession(IDbConnection dbConn)
        {
            return Session.Insert(dbConn, MakeUser(dbConn));
        }
        public static WorkItem MakeWorkItem(IDbConnection dbConn)
        {
            String workItemName = TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(1, 100);
            int state = TestUtil.RANDOM.Next(1, 100);
            var step = TestUtil.MakeStep(dbConn);
            var session = TestUtil.MakeSession(dbConn);
            return WorkItem.Insert(dbConn, step, workItemName, state, priority, session);
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
    }
}
