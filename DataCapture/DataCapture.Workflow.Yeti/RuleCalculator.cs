using System;
using System.Data;
using System.Text;
using DataCapture.Workflow.Yeti.Db;

namespace DataCapture.Workflow.Yeti
{
    class RuleCalculator
    {
        #region constants
        #endregion

        #region members
        #endregion

        #region properties
        #endregion

        #region behavior
        public static bool Applies(Db.Rule rule, String inRule, String inItem)
        {
            switch(rule.Comparison)
            {
                case Db.Rule.Compare.Equal:
                    return inRule.Equals(inItem);
                case Db.Rule.Compare.NotEqual:
                    return !inRule.Equals(inItem);
                default:
                    var msg = new StringBuilder();
                    msg.Append("apply rule comparison of type ");
                    msg.Append(rule.Comparison);
                    msg.Append(" not yet supported in rule ");
                    msg.Append(rule.ToString());
                    throw new Exception(msg.ToString());
            }
        }
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

        public Step FindNextStep(IDbConnection dbConn
            , WorkItemInfo item
            , Step currentStep
            )
        {
            var rules = Db.Rule.Select(dbConn, currentStep);
            if (rules == null) return GetStepBasedNext(dbConn, currentStep);
            if (rules.Count == 0) return GetStepBasedNext(dbConn, currentStep);


            foreach (var rule in rules)
            {
                Console.WriteLine("--- " + rule);
                if (!item.ContainsKey(rule.VariableName)) continue;
                String inRule = rule.VariableValue;
                String inItem = item[rule.VariableName];

                if (Applies(rule, inRule, inItem))
                {
                    return GetRuleBasedNext(dbConn, rule);
                }
            }
            // if none of the rules apply, return step.NextStep
            return GetStepBasedNext(dbConn, currentStep);
}
        #endregion
    }
}

