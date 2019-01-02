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
        #endregion

        #region Behavior
        public void Import(IDbConnection dbConn, FileInfo file)
        {
            int found = 0;
            XmlDocument doc = new XmlDocument();
            doc.Load(file.FullName);

            // document element should be "map":
            var map = Map.Insert(dbConn
                , GetRequiredAttribute(doc.DocumentElement, "name")
                );
            Console.WriteLine(map);
                

            var steps = new Dictionary<String, Step>();
            foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            {  
                System.Xml.XmlElement element = (XmlElement)node;

                // assumes we want one queue per step:
                String stepName = GetRequiredAttribute(element, "name");
                String queueName = map.Name + "." + stepName;
                var queue = Queue.Select(dbConn, queueName);
                if (queue == null)
                {
                    queue = Queue.Insert(dbConn, queueName);
                }

                // assume each element here is a step:
                String nextStepName = GetAttribute(element, "next", "blahblahblah");
                var nextStep = steps.ContainsKey(nextStepName)
                    ? steps[nextStepName]
                    : null
                    ;


                // assumes child elements are steps:
                var step = Step.Insert(dbConn
                    , stepName
                    , map
                    , queue
                    , nextStep
                    , ParseEnum<Step.StepType>(GetRequiredAttribute(element, "type"))
                );

                Console.WriteLine(queue);
                Console.WriteLine(step);
                steps.Add(step.Name, step);
                found++;
            }

            if (found <= 0)
            {
                throw new Exception("no start element found in ["
                    + file.FullName
                    + "]"
                    );
            }
        }
        #endregion

    }
}
