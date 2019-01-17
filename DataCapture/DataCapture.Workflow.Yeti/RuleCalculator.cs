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
        public static Step GetNextStepOrThrow(IDbConnection dbConn, Db.Rule rule)
        {
            var tmp = Step.Select(dbConn, rule.NextStepId);
            if (tmp == null)
            {
                var msg = new StringBuilder();
                msg.Append("internal error: rule applies ");
                msg.Append("but we can't find next step #");
                msg.Append(rule.NextStepId);
                msg.Append(", ");
                msg.Append(rule.ToString());
                throw new Exception(msg.ToString());
            }
            return tmp;
        }

        public Step Apply(IDbConnection dbConn
            , WorkItemInfo item
            , Step currentStep
            )
        {
            var rules = Db.Rule.Select(dbConn, currentStep);
            if (rules == null) return null;
            if (rules.Count == 0) return null;


            foreach (var rule in rules)
            {
                Console.WriteLine("--- " + rule);
                if (!item.ContainsKey(rule.VariableName)) continue;
                String inRule = rule.VariableValue;
                String inItem = item[rule.VariableName];

                if (Applies(rule, inRule, inItem))
                {
                    return GetNextStepOrThrow(dbConn, rule);
                }
            }
            // if none of the rules apply, return step.NextStep

            if (currentStep.NextStepId == Step.NO_NEXT_STEP)
                return null;
            if (currentStep.Type == Step.StepType.Terminating)
                return null;
            var tmp = Step.Select(dbConn, currentStep.NextStepId); // XXX or throw

            Console.WriteLine("<-- Apply()");

            return tmp;
        }
        #endregion
    }
}

