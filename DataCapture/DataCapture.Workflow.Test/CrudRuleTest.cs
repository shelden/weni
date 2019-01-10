using System;
using DataCapture.Workflow.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class CrudRuleTest
    {
        #region Util
        public static Rule.Compare nextCompare()
        {
            switch(TestUtil.RANDOM.Next(0, 4))
            {
                case 0:
                    return Rule.Compare.Equal;
                case 1:
                    return Rule.Compare.NotEqual;
                case 2:
                    return Rule.Compare.Less;
                case 3:
                    return Rule.Compare.Greater;
            }
            return (Rule.Compare)(999999);
        }
        #endregion

        [Test()]
        public void CanInsert()
        {
            String key = TestUtil.NextString();
            String value = TestUtil.NextString();
            int order = TestUtil.RANDOM.Next(10, 100);
            var dbConn = ConnectionFactory.Create();
            int before = TestUtil.SelectCount(dbConn, Rule.TABLE);
            Rule.Insert(dbConn
                , key
                , nextCompare()
                , value
                , order
                , TestUtil.MakeStep(dbConn)
                , TestUtil.MakeStep(dbConn)
                );
            int after = TestUtil.SelectCount(dbConn, Rule.TABLE);
            Assert.AreEqual(before + 1, after);
        }

        [Test()]
        public void InsertAndSelectEqual()
        {
            String key = TestUtil.NextString(1);
            String value = TestUtil.NextString();
            var dbConn = ConnectionFactory.Create();
            var rule = Rule.Select(dbConn, -1); 
            Assert.AreEqual(rule, null);

            var step0 = TestUtil.MakeStep(dbConn);
            var step1 = TestUtil.MakeStep(dbConn);

            var inserted = Rule.Insert(dbConn
                , key
                , nextCompare()
                , value
                , TestUtil.RANDOM.Next(1, 1000)
                , step0
                , step1
                );

            Assert.AreNotEqual(inserted, null);

            var selected = Rule.Select(dbConn, inserted.Id);
            Assert.AreNotEqual(selected, inserted);

            // now make sure they have the same attribites.  Prebably
            // better if we overloaded Equals?
            Assert.AreEqual(inserted.Id, selected.Id);
            Assert.AreEqual(inserted.VariableName, selected.VariableName);
            Assert.AreEqual(inserted.Comparison, selected.Comparison);
            Assert.AreEqual(inserted.RuleOrder, selected.RuleOrder);
            Assert.AreEqual(inserted.VariableValue, selected.VariableValue);
            Assert.AreEqual(inserted.StepId, selected.StepId);
            Assert.AreEqual(inserted.NextStepId, selected.NextStepId);
            Assert.AreEqual(inserted.StepId, step0.Id);
            Assert.AreEqual(inserted.NextStepId, step1.Id);
        }
    }
}
