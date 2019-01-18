using System;
using System.Collections.Generic;
using DataCapture.Workflow.Yeti;
using DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace DataCapture.Workflow.Yeti.Test
{
    public class ApiRulesTest
    {
        [Test()]
        public void RuleThatSkipsMiddle()
        {
            DateTime start = DateTime.UtcNow;
            String itemName = "item" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(-100, 100);
            var wfConn = TestUtil.CreateConnected();
            var names = TestUtil.CreateMapWithRules();
            var pairs = TestUtil.CreatePairs();
            wfConn.CreateItem(names["map"]
                , itemName
                , names["startStep"]
                , pairs
                , priority
                );
            var item0 = wfConn.GetItem(names["queue"]);

            DateTime post = DateTime.UtcNow;
            TestUtil.AssertSame(item0, itemName, pairs, start, post, priority);
            TestUtil.AssertRightPlaces(item0, names["map"], names["startStep"]);

            Assert.IsNotNull(item0);

            // now finish the item.  The rule skip=true should be applied; and
            // the item should go from the start step to the endStep; skipping
            // the middle step
            item0["skipMiddle"] = "true";
            wfConn.FinishItem(item0);
            var item1 = wfConn.GetItem(names["queue"]);
            post = DateTime.UtcNow;
            TestUtil.AssertSame(item1, itemName, item0, start, post, priority);
            TestUtil.AssertRightPlaces(item1, names["map"], names["endStep"]);


            // now, since we're in an end step, finishing the item should make
            // it go away:
            wfConn.FinishItem(item1);

            var item2 = wfConn.GetItem(names["queue"]);
            Assert.IsNull(item2);

            // make sure everything is gone from the DB, now that the item
            // is complete:

            var dbConn = ConnectionFactory.Create();
            Assert.IsNull(WorkItem.Select(dbConn, item0.Id));
            Assert.IsNull(WorkItem.Select(dbConn, item1.Id));
            Assert.AreEqual(0, WorkItemData.SelectAll(dbConn, item0.Id).Count);
            Assert.AreEqual(0, WorkItemData.SelectAll(dbConn, item1.Id).Count);
        }
        
        private void Verify(Db.Rule.Compare op
            , String left
            , String right
            , bool expected
            )
        {
            var msg = new System.Text.StringBuilder();
            try
            {
                var rule = new Db.Rule(-1 // not in db, so no id
                    , -1 // not in db, so no step
                    , "variable name irrelevant for RC.Applies()"
                    , op
                    , left
                    , TestUtil.RANDOM.Next() // order also irrelvant for this test
                    , Step.NO_NEXT_STEP
                    );
                msg.Append("Rule with operator ");
                msg.Append(op);
                msg.Append(" [");
                msg.Append(left);
                msg.Append("] vs [");
                msg.Append(right);
                msg.Append("] we expect to evaulate to ");
                msg.Append(expected);

                bool result = RuleCalculator.Applies(rule, right);

                if (result != expected)
                {
                    Console.WriteLine(msg);
                }
                Assert.That(result == expected, msg.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(msg);
                Console.WriteLine("but it threw: " + ex);
                throw;
            }
        }

        [Test()]
        public void VerifyComparisons()
        {
            bool LOWER_SMALLER = String.Compare("a", "A") < 0;
            bool SPACES_SMALLER = String.Compare("   Purple", "Purple") < 0;

            var randomString = TestUtil.NextString();
            Verify(Db.Rule.Compare.Equal, "a", "a", true);
            Verify(Db.Rule.Compare.Equal, randomString, randomString, true);
            Verify(Db.Rule.Compare.Equal, "a", "b", false);
            Verify(Db.Rule.Compare.Equal, "Qwerty", "Qwerty", true);
            Verify(Db.Rule.Compare.Equal, "", "", true);
            Verify(Db.Rule.Compare.Equal, "ABC", "abc", false);
            Verify(Db.Rule.Compare.Equal, randomString, TestUtil.NextString(), false);

            // Equal is a string comparison.  Just so there's no confusion:
            Verify(Db.Rule.Compare.Equal, "12345", "12345", true);
            Verify(Db.Rule.Compare.Equal, "12345", "12345  ", false);
            Verify(Db.Rule.Compare.Equal, "    12345", "12345", false);
            Verify(Db.Rule.Compare.Equal, "0012345", "12345", false);
            Verify(Db.Rule.Compare.Equal, "0xff", "255", false);

            Verify(Db.Rule.Compare.NotEqual, "a", "a", false);
            Verify(Db.Rule.Compare.NotEqual, randomString, randomString, false);
            Verify(Db.Rule.Compare.NotEqual, "a", "b", true);
            Verify(Db.Rule.Compare.NotEqual, "Qwerty", "Qwerty", false);
            Verify(Db.Rule.Compare.NotEqual, "", "", false);
            Verify(Db.Rule.Compare.NotEqual, "ABC", "abc", true);
            Verify(Db.Rule.Compare.NotEqual, randomString, TestUtil.NextString(), true);

            // NotEqual is a string comparison.  Just so there's no confusion:
            Verify(Db.Rule.Compare.NotEqual, "12345", "12345", false);
            Verify(Db.Rule.Compare.NotEqual, "12345", "12345  ", true);
            Verify(Db.Rule.Compare.NotEqual, "    12345", "12345", true);
            Verify(Db.Rule.Compare.NotEqual, "0012345", "12345", true);
            Verify(Db.Rule.Compare.NotEqual, "0xff", "255", true);

            Verify(Db.Rule.Compare.Less, "", "", false);
            Verify(Db.Rule.Compare.Less, "", "0", false);
            Verify(Db.Rule.Compare.Less, "", "A", false);
            Verify(Db.Rule.Compare.Less, "A", "", true);
            Verify(Db.Rule.Compare.Less, "0", "", true);
            Verify(Db.Rule.Compare.Less, "ABC", "abc", LOWER_SMALLER);
            Verify(Db.Rule.Compare.Less, "abc", "ABC", !LOWER_SMALLER);

            Verify(Db.Rule.Compare.Less, "XXX", "XXX", false);
            Verify(Db.Rule.Compare.Less, "XXX", "XXY", false);
            Verify(Db.Rule.Compare.Less, "XXX", "XXW", true);
            Verify(Db.Rule.Compare.Less, "XXX", "XX", true);
            Verify(Db.Rule.Compare.Less, "XXX", "XXXa", false);
            Verify(Db.Rule.Compare.Less, "XXX", "XXX0", false);


            Verify(Db.Rule.Compare.Less, "XXY", "XXX", true);
            Verify(Db.Rule.Compare.Less, "XXW", "XXX", false);
            Verify(Db.Rule.Compare.Less, "XX", "XXX", false);
            Verify(Db.Rule.Compare.Less, "XXXa", "XXX", true);
            Verify(Db.Rule.Compare.Less, "XXX0", "XXX", true);

            // Less is a string comparison.  Just so there's no confusion:
            Verify(Db.Rule.Compare.Less, "12345", "12345", false);
            Verify(Db.Rule.Compare.Less, "12345", "12345  ", false);
            Verify(Db.Rule.Compare.Less, "12345   ", "12345", true);
            Verify(Db.Rule.Compare.Less, "12345", "12345    ", false);
            Verify(Db.Rule.Compare.Less, "    12345", "12345", !SPACES_SMALLER);
            Verify(Db.Rule.Compare.Less, "12345", "    12345", SPACES_SMALLER);
            Verify(Db.Rule.Compare.Less, "0012345", "12345", false);
            Verify(Db.Rule.Compare.Less, "12345", "0012345", true);
            Verify(Db.Rule.Compare.Less, "0xff", "255", false);
            Verify(Db.Rule.Compare.Less, "255", "0xff", true);
        }
    }
}

