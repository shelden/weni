using System;
using System.Data;
using System.Text;
using DataCapture.Workflow.Yeti.Db;

namespace DataCapture.Workflow.Yeti
{
    public class RuleCalculator
    {
        #region constants
        #endregion

        #region members
        #endregion

        #region properties
        #endregion

        #region behavior
        /// <summary>
        /// Finds the next step in the workflow.  That step could be:
        /// * due to an matching / applied rule
        /// * the next step configured in the workflow
        /// * null if this is a terminating step, and no rules apply
        /// </summary>
        /// <returns>The next step.</returns>
        /// <param name="dbConn">connection to the underlying database</param>
        /// <param name="item">Item.</param>
        /// <param name="step">Current step</param>
        public Step FindNextStep(IDbConnection dbConn
            , WorkItemInfo item
            , Step step
            )
        {
            if (item == null) throw new ArgumentNullException("item");
            if (step == null) throw new ArgumentNullException("step");

            var rules = Db.Rule.Select(dbConn, step);
            if (rules == null) return GetStepBasedNext(dbConn, step);
            if (rules.Count == 0) return GetStepBasedNext(dbConn, step);

            foreach (var rule in rules)
            {
                if (!item.ContainsKey(rule.VariableName)) continue;
                String inItem = item[rule.VariableName];

                if (Applies(rule, inItem))
                {
                    return GetRuleBasedNext(dbConn, rule);
                }
            }
            // if none of the rules apply, return step.NextStep
            return GetStepBasedNext(dbConn, step);
        }
        #endregion

        #region internal behavior
        /// <summary>
        /// Returns true iff the value in the rule satisfies the value in
        /// the item. 
        /// For example, if the rule says foo!=someValue, and the item
        /// has the value in foo for otherValue, then this method
        /// returns true.
        /// </summary>
        /// <param name="rule">the rule to apply</param>
        /// <param name="valueInItem">The value in the item</param>
        public static bool Applies(Db.Rule rule
            , String valueInItem
            )
        {
            if (rule == null) throw new ArgumentNullException("rule");
            String valueInRule = rule.VariableValue;

            switch (rule.Comparison)
            {
                case Db.Rule.Compare.Equal:
                    return valueInRule.Equals(valueInItem);
                case Db.Rule.Compare.NotEqual:
                    return !valueInRule.Equals(valueInItem);
                default:
                    var msg = new StringBuilder();
                    msg.Append("apply rule comparison of type ");
                    msg.Append(rule.Comparison);
                    msg.Append(" not yet supported in rule ");
                    msg.Append(rule.ToString());
                    throw new Exception(msg.ToString());
            }
        }

        /// <summary>
        /// Helper method to return the next step based on the specified
        /// rule.  Called after we've determined that the rule applies.
        /// </summary>
        /// <returns>The rule based next.</returns>
        /// <param name="dbConn">connection to the underlying database</param>
        /// <param name="rule">the matching rule</param>
        public static Step GetRuleBasedNext(IDbConnection dbConn, Db.Rule rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            var tmp = Step.Select(dbConn, rule.NextStepId);
            if (tmp == null)
            {
                var msg = new StringBuilder();
                msg.Append("internal error: the rule applies ");
                msg.Append("but we can't find next step #");
                msg.Append(rule.NextStepId);
                msg.Append(" in the database [");
                msg.Append(rule.ToString());
                msg.Append("]");
                throw new Exception(msg.ToString());
            }
            return tmp;
        }

        /// <summary>
        /// Returns the next step to the specified step.
        /// This method is called when no rules
        /// apply.
        /// If the step is a terminating step, it returns null.
        /// Exceptions are thrown if there is no next step (for
        /// non-Terminating steps), or if the next step is
        /// missing.
        /// </summary>
        /// <returns>The step configured to be next</returns>
        /// <param name="dbConn">connection to the underlying database</param>
        /// <param name="step">the current step</param>
        public static Step GetStepBasedNext(IDbConnection dbConn, Step step)
        {
            if (step == null) throw new ArgumentNullException("step");
            if (step.NextStepId == Step.NO_NEXT_STEP
                && step.Type == Step.StepType.Terminating
                )
            {
                // this is ok.  Correctly configured final step.
                return null;
            }

            if (step.NextStepId == Step.NO_NEXT_STEP)
            {
                var msg = new StringBuilder();
                msg.Append("internal error: step has no next step.  ");
                msg.Append("However, it is not a terminating step [");
                msg.Append(step.Type);
                msg.Append("]  ");
                msg.Append(step);
            }


            var tmp = Step.Select(dbConn, step.NextStepId);
            if (tmp == null)
            {
                var msg = new StringBuilder();
                msg.Append("internal error: no rule applies ");
                msg.Append("but the step's next step is missing #");
                msg.Append(step.NextStepId);
                msg.Append(" in the database [");
                msg.Append(step.ToString());
                msg.Append("]");
                throw new Exception(msg.ToString());
            }
            return tmp;
        }
        #endregion
    }
}

