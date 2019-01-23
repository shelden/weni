using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace LM.DataCapture.Workflow.Yeti.Db
{
    public class Rule
    {
        #region Enums
        public enum Compare
        {
            Equal = 1
            , NotEqual = 2
            , Greater = 3
            , Less = 4
        };
        #endregion

        #region Constants
        public static readonly String TABLE = "workflow.rules";
        private static readonly String INSERT = ""
                + "insert into "
                + TABLE
                + " ( "
                + "     step_id "
                + "     , rule_order "
                + "     , variable_name "
                + "     , variable_value "
                + "     , comparison "
                + "     , next_step_id "
                + ") "
                + "values ("
                + "     @step_id "
                + "     , @rule_order "
                + "     , @variable_name "
                + "     , @variable_value "
                + "     , @comparison "
                + "     , @next_step_id "
                + ") "
		;
        private static readonly String SELECT_BASE = ""
            + "SELECT rule_id "
            + "       , step_id "
            + "       , rule_order "
            + "       , variable_name "
            + "       , variable_value "
            + "       , comparison "
            + "       , next_step_id "
            + " FROM "
            + TABLE + " "
            + "WHERE 0 = 0 "
            ;
        private static readonly String SELECT_BY_ID = SELECT_BASE
            + "AND       rule_id = @id ";
        private static readonly String SELECT_BY_STEP_ID = SELECT_BASE
            + "AND       step_id = @step_id "
            + "ORDER BY  rule_order "
            + "          , rule_id "
            ;
        #endregion

        #region Properties
        public int Id { get; set; }
        public int StepId { get; set; }
        public int RuleOrder { get; set; }
        public String VariableName { get; set; }
        public String VariableValue { get; set; }
        public Compare Comparison { get; set; }
        public int NextStepId { get; set; }
        #endregion

        #region Constructors
        public Rule(int id
            , int stepId
            , String variableName
            , Compare comparison
            , String variableValue
            , int ruleOrder
            , int nextStepId
            )
        {
            Id = id;
            StepId = stepId;
            RuleOrder = ruleOrder;
            VariableName = variableName;
            VariableValue = variableValue;
            Comparison = comparison;
            NextStepId = nextStepId;
        }
        public Rule(IDataReader reader)
            : this(DbUtil.GetInt(reader, "rule_id")
                  , DbUtil.GetInt(reader, "step_id")
                  , DbUtil.GetString(reader, "variable_name")
                  , (Rule.Compare)DbUtil.GetInt(reader, "comparison")
                  , DbUtil.GetString(reader, "variable_value")
                  , DbUtil.GetInt(reader, "rule_order")
                  , DbUtil.GetInt(reader, "next_step_id")
                  )
        { /* no code */ }
        #endregion

        #region CRUD: Insert
        public static Rule Insert(IDbConnection dbConn
            , String variableName
            , Rule.Compare operation
            , String variableValue
            , int ruleOrder
            , Step step
            , Step nextStep
            )

        {
            try
            {
                // arg checks here in try block, so the building of message can only be
                // done 1x:
                if (ruleOrder < 0) throw new ArgumentException("ruleOrder is unsigned.  Use >= 0");
                if (step == null) throw new ArgumentNullException("step");
                if (nextStep == null) throw new ArgumentNullException("nextStep");

                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;

                DbUtil.AddParameter(command, "@rule_order", ruleOrder);
                DbUtil.AddParameter(command, "@variable_name", variableName);
                DbUtil.AddParameter(command, "@variable_value", variableValue);
                DbUtil.AddParameter(command, "@comparison", (int)(operation));
                DbUtil.AddParameter(command, "@step_id", step.Id);
                DbUtil.AddParameter(command, "@next_step_id", nextStep.Id);

                int id = Convert.ToInt32(command.ExecuteScalar());
                return new Rule(id
                    , step.Id
                    , variableName
                    , operation
                    , variableValue
                    , ruleOrder
                    , nextStep.Id
                    );
            }
            catch(Exception ex)
            {
                var msg = new StringBuilder();
                msg.Append("error adding rule [");
                msg.Append(variableName);
                msg.Append(Pretty(operation));
                msg.Append(variableValue);
                msg.Append("], order ");
                msg.Append(ruleOrder);
                msg.Append(" from step [");
                msg.Append(step);
                msg.Append("] to [");
                msg.Append(nextStep);
                msg.Append("]: ");
                msg.Append(ex.Message);
                //Console.WriteLine(msg);
                //Console.WriteLine(INSERT);
                throw new Exception(msg.ToString(), ex);
            }
        }
        #endregion

        #region CRUD: Select
        public static Rule Select(IDbConnection dbConn, int ruleId)
        {
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_ID;
                DbUtil.AddParameter(command, "@id", ruleId);
                reader = command.ExecuteReader();

                if (reader == null) return null;
                if (!reader.Read()) return null;

                return new Rule(reader); 
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }
        }

        /// <summary>
        /// Find all the rules hanging of a specified step, in order
        /// </summary>
        /// <returns>The select.</returns>
        /// <param name="dbconn">connection to database</param>
        /// <param name="step">The step</param>
        public static IList<Rule> Select(IDbConnection dbConn, Step step)
        {
            var tmp = new List<Rule>();
            IDataReader reader = null;
            try
            {
                IDbCommand command = dbConn.CreateCommand();
                command.CommandText = SELECT_BY_STEP_ID;
                DbUtil.AddParameter(command, "@step_id", step.Id);
                reader = command.ExecuteReader();
                if (reader == null) return null;
                while(reader.Read())
                {
                    tmp.Add(new Rule(reader));
                }
                return tmp;
            }
            finally
            {
                DbUtil.ReallyClose(reader);
            }

        }
        #endregion

        #region ToString()
        public static String Pretty(Db.Rule.Compare operation)
        {
            var sb = new StringBuilder();
            switch (operation)
            {
                case Rule.Compare.Equal:
                    sb.Append("==");
                    break;
                case Rule.Compare.NotEqual:
                    sb.Append("!=");
                    break;
                case Rule.Compare.Greater:
                    sb.Append(">");
                    break;
                case Rule.Compare.Less:
                    sb.Append("<");
                    break;
                default:
                    sb.Append("??");
                    break;
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            sb.Append(' ');
            sb.Append(this.Id);
            sb.Append(", ");
            sb.Append(this.VariableName);
            sb.Append(Pretty(this.Comparison));
            sb.Append(this.VariableValue);
            sb.Append(", next=");
            sb.Append(this.NextStepId);
            return sb.ToString();
        }
        #endregion
    }
}
