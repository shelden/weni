using System;
using System.Data;
using System.Text;

namespace DataCapture.Workflow.Yeti.Db
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
        public static readonly String INSERT = ""
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
        public static readonly String SELECT_BY_ID = ""
                + "select rule_id "
                + "       , step_id "
                + "       , rule_order "
                + "       , variable_name "
                + "       , variable_value "
                + "       , comparison "
                + "       , next_step_id "
                + " from "
                + TABLE + " "
                + "WHERE 0 = 0 "
                + "AND   rule_id = @id "
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
            , Rule.Compare compare
            , String variableValue
            , int ruleOrder
            , Step step
            , Step nextStep
            )

        {
            IDbCommand command = dbConn.CreateCommand();
            command.CommandText = INSERT + " ; " + DbUtil.GET_KEY;

            DbUtil.AddParameter(command, "@rule_order", ruleOrder);
            DbUtil.AddParameter(command, "@variable_name", variableName);
            DbUtil.AddParameter(command, "@variable_value", variableValue);
            DbUtil.AddParameter(command, "@comparison", (int)(compare));
            DbUtil.AddParameter(command, "@step_id", step.Id);
            DbUtil.AddParameter(command, "@next_step_id", nextStep.Id);

            int id = Convert.ToInt32(command.ExecuteScalar());
            return new Rule(id
                , step.Id
                , variableName
                                , compare

                , variableValue
                                , ruleOrder

                , nextStep.Id
                );
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
        #endregion

        #region ToString()
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            sb.Append(' ');
            sb.Append(this.Id);
            sb.Append(", ");
            sb.Append(this.VariableName);
            switch(this.Comparison)
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
            sb.Append(this.VariableValue);
            sb.Append(", next=");
            sb.Append(this.NextStepId);
            return sb.ToString();
        }
        #endregion
    }
}
