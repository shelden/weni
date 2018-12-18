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
        public static Queue makeQueue(IDbConnection dbConn)
        {
            String queueName = TestUtil.NextString();
            return Queue.Insert(dbConn, queueName);
        }
        public static Map makeMap(IDbConnection dbConn)
        {
            String mapName = TestUtil.NextString();
            return Map.Insert(dbConn, mapName);
        }
        public static Step makeStep(IDbConnection dbConn)
        {
            String stepName = TestUtil.NextString();
            return Step.Insert(dbConn, stepName, makeMap(dbConn), makeQueue(dbConn), 29);
        }
        #endregion
    }
}
