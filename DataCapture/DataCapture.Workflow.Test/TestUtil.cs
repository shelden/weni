using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
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
            switch(RANDOM.Next(0, 4))
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
                tmp.Add("key" + i + "of" + count, TestUtil.NextString());
            }
            return tmp;
        }
        #endregion

    }
}
