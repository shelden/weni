﻿using System;
using System.Collections.Generic;
using LM.DataCapture.Workflow.Yeti;
using LM.DataCapture.Workflow.Yeti.Db;
using NUnit.Framework;

namespace LM.DataCapture.Workflow.Yeti.Test
{
    public class ApiRulesTest
    {
        #region constants
        public static readonly bool SPACES_SMALLER = String.Compare("   Purple", "Purple") < 0;
        public static readonly bool LOWERCASE_SMALLER = String.Compare("z", "Z") < 0;
        public static readonly bool DIGITS_SMALLER = String.Compare("0", "A") < 0;
        #endregion

        #region utility
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
        #endregion

        [Test()]
        public void RuleThatSkipsMiddle()
        {
            DateTime start = TestUtil.FlooredNow();
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

            TestUtil.AssertSame(item0, itemName, pairs, start, priority);
            TestUtil.AssertRightPlaces(item0, names["map"], names["startStep"]);

            Assert.IsNotNull(item0);

            // now finish the item.  The rule skip=true should be applied; and
            // the item should go from the start step to the endStep; skipping
            // the middle step
            item0["skipMiddle"] = "true";
            wfConn.FinishItem(item0);
            var item1 = wfConn.GetItem(names["queue"]);
            TestUtil.AssertSame(item1, itemName, item0, start, priority);
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
        


        [Test()]
        public void VerifyComparisons()
        {
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
            Verify(Db.Rule.Compare.Less, "ABC", "abc", LOWERCASE_SMALLER);
            Verify(Db.Rule.Compare.Less, "abc", "ABC", !LOWERCASE_SMALLER);
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

            Verify(Db.Rule.Compare.Greater, "", "", false);
            Verify(Db.Rule.Compare.Greater, "", "0", true);
            Verify(Db.Rule.Compare.Greater, "", "A", true);
            Verify(Db.Rule.Compare.Greater, "A", "", false);
            Verify(Db.Rule.Compare.Greater, "0", "", false);
            Verify(Db.Rule.Compare.Greater, "ABC", "abc", !LOWERCASE_SMALLER);
            Verify(Db.Rule.Compare.Greater, "abc", "ABC", LOWERCASE_SMALLER);
            Verify(Db.Rule.Compare.Greater, "XXX", "XXX", false);
            Verify(Db.Rule.Compare.Greater, "XXX", "XXY", true);
            Verify(Db.Rule.Compare.Greater, "XXX", "XXW", false);
            Verify(Db.Rule.Compare.Greater, "XXX", "XX", false);
            Verify(Db.Rule.Compare.Greater, "XXX", "XXXa", true);
            Verify(Db.Rule.Compare.Greater, "XXX", "XXX0", true);
            Verify(Db.Rule.Compare.Greater, "XXY", "XXX", false);
            Verify(Db.Rule.Compare.Greater, "XXW", "XXX", true);
            Verify(Db.Rule.Compare.Greater, "XX", "XXX", true);
            Verify(Db.Rule.Compare.Greater, "XXXa", "XXX", false);
            Verify(Db.Rule.Compare.Greater, "XXX0", "XXX", false);


            // Greater is a string comparison.  Just so there's no confusion:
            Verify(Db.Rule.Compare.Greater, "12345", "12345", false);
            Verify(Db.Rule.Compare.Greater, "12345", "12345  ", true);
            Verify(Db.Rule.Compare.Greater, "12345   ", "12345", false);
            Verify(Db.Rule.Compare.Greater, "12345", "12345    ", true);
            Verify(Db.Rule.Compare.Greater, "    12345", "12345", SPACES_SMALLER);
            Verify(Db.Rule.Compare.Greater, "12345", "    12345", !SPACES_SMALLER);
            Verify(Db.Rule.Compare.Greater, "0012345", "12345", true);
            Verify(Db.Rule.Compare.Greater, "12345", "0012345", false);
            Verify(Db.Rule.Compare.Greater, "0xff", "255", true);
            Verify(Db.Rule.Compare.Greater, "255", "0xff", false);
        }

        [Test()]
        public void UnknownOperatorThrows()
        {
            Console.WriteLine("--> UnknownOperatorThrows()");
            Dictionary<int, bool> expectedList = null;
            try
            {
                expectedList = new Dictionary<int, bool>()
            {
                { (int)Db.Rule.Compare.Equal, false }
                , { (int)Db.Rule.Compare.NotEqual, false }
                , { (int)Db.Rule.Compare.Greater, false }
                , { (int)Db.Rule.Compare.Less, false }
                , { (int)Db.Rule.Compare.Less + 1, true }
                , { (int)Db.Rule.Compare.Equal - 1, true }
                , { TestUtil.RANDOM.Next(int.MinValue, 0), true }
                , { TestUtil.RANDOM.Next(100, int.MaxValue), true }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            foreach (var i in expectedList.Keys)
            {

                var expected = expectedList[i];
                Console.WriteLine(i + ") " + expected);
                String msg = "";
                try
                {
                    Db.Rule.Compare op = (Db.Rule.Compare)i;
                    bool verifyShouldReturn = false;
                    switch(op)
                    {
                        case Db.Rule.Compare.Equal:
                            verifyShouldReturn = true;
                            break;
                        case Db.Rule.Compare.NotEqual:
                        case Db.Rule.Compare.Less:
                        case Db.Rule.Compare.Greater:
                            verifyShouldReturn = false;
                            break;
                        default:
                            // doesn't really matter; we want
                            // it to throw
                            break;
                    }
                    Verify(op, "same", "same", verifyShouldReturn);
                }
                catch(Exception ex)
                {
                    msg = ex.Message;
                }

                if (expected)
                {
                    Assert.That(msg != "", "we expected an exception to be thrown for " + i);
                    Assert.That(msg.Contains("unknown rule comparison"));
                    Assert.That(msg.Contains(i.ToString()));
                }
                else
                {
                    Assert.That(String.IsNullOrEmpty(msg)
                        , "we didn't expect an exception for " 
                        + i 
                        + " but we got one anyway: " 
                        + msg
                        );
                }
            }
        }


        struct StepByRuleOrder
        {
            public int middleOrder_;
            public int endOrder_;
            public String stepName_;

            public StepByRuleOrder(int middleOrder
                , int endOrder
                , String stepName)
            {
                middleOrder_ = middleOrder;
                endOrder_ = endOrder;
                stepName_ = stepName;
            }
        }

        [Test()]
        public void RulesAppliedInOrder()
        {

            DateTime startOfTest = TestUtil.FlooredNow();
            var dbConn = ConnectionFactory.Create();

            var names = TestUtil.CreateMapWithRules();

            var start = Step.Select(dbConn, names["startStep"]);
            var middle = Step.Select(dbConn, names["middleStep"]);
            var end = Step.Select(dbConn, names["endStep"]);
            Assert.IsNotNull(start);
            Assert.IsNotNull(middle);
            Assert.IsNotNull(end);

            // then a list of rule orders vs the step name into which
            // we expect the item to end up.  Basically, if the rule's
            // order is lower, we expect it to be applied first.
            var list = new List<StepByRuleOrder>();

            // Note: when setting these up, don't duplicate a rule order.
            // that voilates a DB constraint
            list.Add(new StepByRuleOrder(101, 201, names["middleStep"]));
            list.Add(new StepByRuleOrder(202, 102, names["endStep"]));
            list.Add(new StepByRuleOrder(0, 1, names["middleStep"]));
            list.Add(new StepByRuleOrder(1, 0, names["endStep"]));
            list.Add(new StepByRuleOrder(99999, int.MaxValue, names["middleStep"]));
            list.Add(new StepByRuleOrder(int.MaxValue, 99999, names["endStep"]));

            // rule order is unsigned in the DB, so, note no comparison betwee rule order -5
            // and 5


            int i = 0;
            foreach (var e in list)
            {
                String variable = "var" + i + TestUtil.NextString();
                String target = "shazaam" + i + TestUtil.NextString();
                // now, for each pair of rule orders, create a rule
                // with the specified rule priority / order:
                var middleRule = Db.Rule.Insert(dbConn
                                                , variable
                                                , Db.Rule.Compare.Equal
                                                , target
                                                , e.middleOrder_
                                                , start
                                                , middle
                                                );

                // next, let's add the same rule, variable=target, but
                // with a different order (and different destination,
                // so we can tell):
                var endRule = Db.Rule.Insert(dbConn
                                             , variable
                                             , Db.Rule.Compare.Equal
                                             , target
                                             , e.endOrder_
                                             , start
                                             , end
                                             );


                // now let's insert something with variable=target
                String itemName = "item4orders" + TestUtil.NextString();
                int priority = TestUtil.RANDOM.Next(-100, 100);
                var wfConn = TestUtil.CreateConnected();
                var pairs = TestUtil.CreatePairs();
                pairs[variable] = target;

                wfConn.CreateItem(names["map"]
                    , itemName
                    , names["startStep"]
                    , pairs
                    , priority
                    );

                // it should be in the start step:
                var item0 = wfConn.GetItem(names["queue"]);
                TestUtil.AssertSame(item0, itemName, pairs, startOfTest, priority);
                TestUtil.AssertRightPlaces(item0, names["map"], names["startStep"]);

                // now, let's finish the item.  The rules should be
                // applied based on the rule_order attributes specified
                // in the struct
                wfConn.FinishItem(item0);
                var item1 = wfConn.GetItem(names["queue"]);

                TestUtil.AssertSame(item1, itemName, pairs, startOfTest, priority);
                TestUtil.AssertRightPlaces(item1, names["map"], e.stepName_);
            }
        }

        [Test()]
        public void BadNextStepsThrow()
        {
            // check next step in step.  NO_NEXT_STEP only on Terminating
            //Assert.Fail("check next not yet implemented");

            // check next step in rule.  Non-existent throws

            // check next step in step.  Non-existent throws
        }
    }
}
