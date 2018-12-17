using System;
using System.Data;
using DataCapture.Workflow.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class CrudStepTest
    {
        #region Util
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
        #endregion

        [Test()]
        public void CanInsertWithoutFollowing()
        {
            String stepName = TestUtil.NextString();

            int type = TestUtil.RANDOM.Next(2, 100);
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Step.TABLE);

            var step = Step.Insert(dbConn, stepName, makeMap(dbConn), makeQueue(dbConn), type);

            Assert.AreEqual(step.NextStepId, Step.NO_NEXT_STEP);

            int after = DbUtil.SelectCount(dbConn, Step.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        [Test()]
        public void CanInsertWithFollowing()
        {
            String step1Name = TestUtil.NextString();
            String step0Name = TestUtil.NextString();
            int type = TestUtil.RANDOM.Next(2, 100);
            var dbConn = ConnectionFactory.Create();
            int before = DbUtil.SelectCount(dbConn, Step.TABLE);
            var queue = makeQueue(dbConn);
            var map = makeMap(dbConn);

            var step1 = Step.Insert(dbConn, step1Name, map, queue, type);
            var step0 = Step.Insert(dbConn, step0Name, map, queue, step1, type);

            Assert.AreEqual(step1.NextStepId, Step.NO_NEXT_STEP);
            Assert.AreEqual(step0.NextStepId, step1.Id);

            int after = DbUtil.SelectCount(dbConn, Step.TABLE);
            Assert.AreEqual(before + 2, after);
        }

        [Test()]
        // if you don't close your reader correctly, you get an
        // error inserting in this order...
        public void CanSelectThenInsert()
        {
            String name = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            var step = Step.Select(dbConn, name);
            Assert.AreEqual(step, null); // because name is a random string

            Step.Insert(dbConn, name, makeMap(dbConn), makeQueue(dbConn), 99);
            step = Step.Select(dbConn, name);

            Assert.AreEqual(step.Name, name);
            Assert.AreEqual(step.Type, 99);
            Assert.AreEqual(step.NextStepId, Step.NO_NEXT_STEP);
            Assert.GreaterOrEqual(step.Id, 1);
        }
    }
}
