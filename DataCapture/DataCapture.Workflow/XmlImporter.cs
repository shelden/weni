using System;
using System.Xml;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using DataCapture.Workflow.Db;

namespace DataCapture.Workflow
{
    public class XmlImporter
    {
        #region Constants
        #endregion

        #region Constructors
        public XmlImporter()
        {
        }
        #endregion

        #region Members
        private Map map_ = null;
        #endregion

        #region Xml 
        /// <summary>
        /// safely extracts a string attribute from an XmlElement.
        /// If it doesn't exist or problems occur, it reutrns
        /// the specified default
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="element">Xml Element</param>
        /// <param name="name">Name of the element</param>
        /// <param name="defaultValue">Default value</param>
        static private String GetAttribute(XmlElement element
            , String name
            , String defaultValue
            )
        {
            if (element == null) return defaultValue;
            XmlAttribute attribute = element.Attributes[name];
            if (attribute == null) return defaultValue;
            if (String.IsNullOrEmpty(attribute.Value)) return defaultValue;
            return attribute.Value;
        }
        /// <summary>
        /// safely extracts a string attribute from an XmlElement.
        /// If it doesn't exist or problems occur, it reutrns
        /// the specified default
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="element">Xml Element</param>
        /// <param name="name">Name of the element</param>
        /// <param name="defaultValue">Default value</param>
        static private bool GetAttribute(XmlElement element
            , String name
            , bool defaultValue
            )
        {
            if (element == null) return defaultValue;
            XmlAttribute attribute = element.Attributes[name];
            if (attribute == null) return defaultValue;
            if (String.IsNullOrEmpty(attribute.Value)) return defaultValue;
            switch(attribute.Value.ToLower())
            {
                case "true":
                case "yes":
                    return true;
                case "false":
                case "no":
                    return false;
                default:
                    break;
            }
            return defaultValue;
        }
        /// <summary>
        /// safely extracts a string attribute from an XmlElement.
        /// If it doesn't exist or problems occur, it reutrns
        /// the specified default
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="element">Xml Element</param>
        /// <param name="name">Name of the element</param>
        /// <param name="defaultValue">Default value</param>
        static private int GetAttribute(XmlElement element
            , String name
            , int defaultValue
            )
        {
            if (element == null) return defaultValue;
            XmlAttribute attribute = element.Attributes[name];
            if (attribute == null) return defaultValue;
            if (String.IsNullOrEmpty(attribute.Value)) return defaultValue;
            try
            {
                return int.Parse(attribute.Value);
            }
            catch
            {
                ;  // no code
            }
            return defaultValue;
        }
        /// <summary>
        /// Like GetAtrribute(), but throws if the element doesn't exist
        /// </summary>
        /// <returns>The required attribute.</returns>
        /// <param name="element">Element.</param>
        /// <param name="name">Name.</param>
        static private String GetRequiredAttribute(XmlElement element
            , String name
            )
        {
            StringBuilder msg = new StringBuilder();
            if (element == null) throw new ArgumentNullException("element");

            XmlAttribute attribute = element.Attributes[name];
            if (attribute == null)
            {
                msg.Append("Missing attribute [");
                msg.Append(name);
                msg.Append("] in element [");
                msg.Append(element.Name);
                msg.Append(']');
                throw new ArgumentException(msg.ToString());
            }
            if (String.IsNullOrEmpty(attribute.Value))
            {
                msg.Append("Empty attribute [");
                msg.Append(name);
                msg.Append("] in element [");
                msg.Append(element.Name);
                msg.Append(']');
                throw new ArgumentException(msg.ToString());
            } 
            return attribute.Value;
        }
        #endregion

        #region enum parsing
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        public static DataCapture.Workflow.Db.Rule.Compare ParseCompare(String value)
        {
            switch (value.ToLower())
            {
                case "greater":
                case "greaterthan":
                case "gt":
                case ">":
                    return DataCapture.Workflow.Db.Rule.Compare.Greater;
                case "less":
                case "lessthan":
                case "lt":
                case "<":
                    return DataCapture.Workflow.Db.Rule.Compare.Less;
                case "equal":
                case "equals":
                case "eq":
                case "==":
                case "=":
                    return DataCapture.Workflow.Db.Rule.Compare.Equal;
                case "notequal":
                case "notequals":
                case "ne":
                case "<>":
                case "!=":
                    return DataCapture.Workflow.Db.Rule.Compare.NotEqual;
                default:
                    break;
            }
            return DataCapture.Workflow.Db.Rule.Compare.Equal;
        }
        #endregion

        #region Behavior
        private void Import(IDbConnection dbConn
                , XmlElement element
                , ref Dictionary<String, Step> steps
                , ref Dictionary<String, Queue> queues
            )
        {
            if (element == null) return;

            switch (element.LocalName.ToLower())
            {
                case "map":
                    {
                        map_ = Map.InsertWithMaxVersion(dbConn
                            , GetRequiredAttribute(element, "name")
                            );
                        break;
                    }
                case "queue":
                    {
                        String queueName = GetRequiredAttribute(element, "name");
                        var queue = Queue.Select(dbConn, queueName);
                        if (queue == null)
                        {
                            queue = Queue.Insert(dbConn
                                , GetRequiredAttribute(element, "name")
                                , GetAttribute(element, "isfail", false)
                                );
                        }
                        queues.Add(queue.Name, queue);
                        break;
                    }
                case "step":
                    {
                        String nextStepName = GetAttribute(element, "next", "");
                        Step nextStep = null;
                        if (!String.IsNullOrEmpty(nextStepName) && steps.ContainsKey(nextStepName))
                        {
                            nextStep = steps[nextStepName];
                        }
                        String queueName = GetRequiredAttribute(element, "queue");
                        if (!queues.ContainsKey(queueName))
                        {
                            var msg = new StringBuilder();
                            msg.Append("unknown queue [");
                            msg.Append(queueName);
                            msg.Append("] specified in step [");
                            msg.Append(GetAttribute(element, "name", "unknown"));
                            msg.Append("]");
                            throw new Exception(msg.ToString());
                        }
                        var queue = queues[GetRequiredAttribute(element, "queue")];
                        var step = Step.Insert(dbConn
                            , GetRequiredAttribute(element, "name")
                            , map_
                            , queue
                            , nextStep
                            , ParseEnum<Step.StepType>(GetAttribute(element, "type", "Standard"))
                            );
                        steps.Add(step.Name, step);
                        break;
                    }
                case "rule":
                    {
                        String stepName = GetRequiredAttribute(element, "step");
                        if (!steps.ContainsKey(stepName))
                        {
                            var msg = new StringBuilder();
                            msg.Append("unknown step [");
                            msg.Append(stepName);
                            msg.Append("] specified in rule");
                            throw new Exception(msg.ToString());
                        }
                        String nextStepName = GetRequiredAttribute(element, "next");
                        if (!steps.ContainsKey(nextStepName))
                        {
                            var msg = new StringBuilder();
                            msg.Append("unknown next step [");
                            msg.Append(nextStepName);
                            msg.Append("] specified in rule");
                            throw new Exception(msg.ToString());
                        }

                        var step = steps[stepName];
                        var next = steps[nextStepName];
                        var rule = DataCapture.Workflow.Db.Rule.Insert(dbConn
                            , GetRequiredAttribute(element, "variable")
                            , ParseCompare(GetAttribute(element, "compare", "Equals"))
                            , GetRequiredAttribute(element, "value")
                            , GetAttribute(element, "order", int.MaxValue)
                            , step
                            , next
                            );
                        break;
                    }
                default:
                    Console.WriteLine("Warning: unknown element [" + element.Name + "]");
                    break;
            }

            foreach(var subnode in element.ChildNodes)
            {
                var subelement = (XmlElement)(subnode);
                this.Import(dbConn, subelement, ref steps, ref queues);
            }

        }
        public void Import(IDbConnection dbConn, FileInfo file)
        {
            var transaction = dbConn.BeginTransaction();
            try
            {

                XmlDocument doc = new XmlDocument();
                doc.Load(file.FullName);

                var steps = new Dictionary<String, Step>();
                var queues = new Dictionary<String, Queue>();
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        var element = (XmlElement)node;
                        Import(dbConn, element, ref steps, ref queues);
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        sb.Append("only XML elements are supported.  Your XML contains ");
                        sb.Append(node.NodeType);
                        sb.Append(" [");
                        sb.Append(node.InnerText);
                        sb.Append("]");
                        throw new Exception(sb.ToString());
                    }
                }
                transaction.Commit();
                transaction = null;
            }
            finally
            {
                map_ = null;
                DbUtil.ReallyClose(transaction);
            }
        }
        #endregion

    }
}
