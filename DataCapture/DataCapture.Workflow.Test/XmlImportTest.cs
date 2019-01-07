using System;
using System.Text;
using System.Collections.Generic;
using DataCapture.Workflow.Db;
using DataCapture.IO;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class XmlImportTest
    {
        #region common / utility
        /// <summary>
        /// Write XML string to a temporary file and import it.  This
        /// process may throw; if so the exception bubbles out for
        /// assertion that that was expected downstream
        /// </summary>
        /// <param name="xml">Xml.</param>
        public void RunImporter(System.Data.IDbConnection dbConn, String xml)
        {
            using (var tmp = new TempFile(".xml"))
            {
                using (var writer = new System.IO.StreamWriter(tmp.FullName))
                {
                    writer.WriteLine(xml);
                }
                var importer = new XmlImporter();
                importer.Import(dbConn, tmp.Value);
            }
        }
        public IDictionary<String, int> BuildCounts(System.Data.IDbConnection dbConn)
        {
            var tmp = new Dictionary<String, int>();
            tmp["steps"] = DbUtil.SelectCount(dbConn, Step.TABLE);
            tmp["rules"] = DbUtil.SelectCount(dbConn, Rule.TABLE);
            tmp["maps"] = DbUtil.SelectCount(dbConn, Map.TABLE);
            var map = Map.Select(dbConn, MAP_NAME);
            tmp["mapVersion"] = map == null ? Map.VERSION - 1 : map.Version;
            return tmp;
        }
        public void AssertIncreases(IDictionary<String, int> before, IDictionary<String, int> after)
        {
            Assert.AreEqual(before["maps"] + 1, after["maps"]); // example has 1 map
            Assert.AreEqual(before["steps"] + 4, after["steps"]); // example has 4 steps
            Assert.AreEqual(before["rules"] + 2, after["rules"]); // example has 2 rules
            Assert.AreEqual(before["mapVersion"] + 1, after["mapVersion"]);
        }
        public void AssertUnchanged(IDictionary<String, int> before, IDictionary<String, int> after)
        {
            Assert.AreEqual(before["maps"], after["maps"]); 
            Assert.AreEqual(before["steps"], after["steps"]); 
            Assert.AreEqual(before["rules"], after["rules"]); 
            Assert.AreEqual(before["mapVersion"], after["mapVersion"]);
        }

        /// <summary>
        /// Runs the importer with an exception expected
        /// </summary>
        /// <param name="dbConn">Db conn.</param>
        /// <param name="xml">Xml.</param>
        /// <param name="containing">a string expected in the bad XML's thrown eception</param>
        public void RunImporterFailingly(System.Data.IDbConnection dbConn
            , String xml
            , String containing
            )
        {
            var before = BuildCounts(dbConn);
            String msg = "";
            try
            {
                RunImporter(dbConn, xml);
            }
            catch(Exception ex)
            {
                msg = ex.Message;
            }
            Assert.That(!String.IsNullOrEmpty(msg), "we expected an exception but it wasn't thrown");
            Console.WriteLine(msg);
            Console.WriteLine("checking if msg contains [" + containing + "]?");
            Assert.That(msg.Contains(containing)
                , "Expected exception containing ["
                + containing
                + "], was not found in message ["
                + msg
                + "]"
                );
            var after = BuildCounts(dbConn);
            AssertUnchanged(before, after);
        }

        #endregion

        #region xml
        public static readonly String NL = Environment.NewLine;
        public static readonly String MAP_NAME = "m.unit";
        public static readonly String EXAMPLE_XML = ""
                + "<anything> " + NL
                + "  <queue name='q.unit.everything' /> " + NL
                + "  <queue name='q.unit.fail' isfail='true' /> " + NL
                + "  <map name='" + MAP_NAME + "' >  " + NL
                + "    <step name='s.unit.end' " + NL
                + "          type='Terminating' " + NL
                + "          queue='q.unit.everything' " + NL
                + "    />  " + NL
                + "    <step name='s.unit.middle1' " + NL
                + "          type='Standard' " + NL
                + "          queue='q.unit.everything' " + NL
                + "          next='s.end' " + NL
                + "    />  " + NL
                + "    <step name='s.unit.middle0' " + NL
                + "          type='Standard' " + NL
                + "          queue='q.unit.everything' " + NL
                + "          next='s.unit.middle1' " + NL
                + "    />  " + NL
                + "    <step name='s.unit.start' " + NL
                + "          type='Terminating' " + NL
                + "          queue='q.unit.everything' " + NL
                + "          next='s.unit.middle0' " + NL
                + "    />  " + NL
                + "    <rule step='s.unit.start' " + NL
                + "          variable='a' " + NL
                + "          comparison='Equals' " + NL
                + "          value='1234' " + NL
                + "          next='s.unit.middle1' " + NL
                + "     /> " + NL
                + "     <rule step='s.unit.middle0' " + NL
                + "           variable='a' " + NL
                + "           comparison='LessThan' " + NL
                + "           value='Smith' " + NL
                + "           next='s.unit.end' " + NL
                + "     /> " + NL
                + "  </map> " + NL
                + "</anything> " + NL
                ;
        #endregion

        [Test()]
        public void CanImport()
        {
            var dbConn = ConnectionFactory.Create();
            var before = BuildCounts(dbConn);
            RunImporter(dbConn, EXAMPLE_XML);
            var map = Map.Select(dbConn, MAP_NAME);
            Assert.IsNotNull(map);
            var after = BuildCounts(dbConn);
            AssertIncreases(before, after);
        }

        [Test()]
        public void AnyRootElementWorks()
        {
            var dbConn = ConnectionFactory.Create();
            var before = BuildCounts(dbConn);
            String root = TestUtil.NextString();
            String xml = EXAMPLE_XML.Replace("anything", root);
            Assert.That(xml.Contains(root));
            Assert.That(!xml.Contains("anything"));
            RunImporter(dbConn, xml);
            var map = Map.Select(dbConn, MAP_NAME);
            Assert.IsNotNull(map);
            var after = BuildCounts(dbConn);
            AssertIncreases(before, after);
        }

        public void CanImportWithLeadingComments()
        {
            var dbConn = ConnectionFactory.Create();
            var before = BuildCounts(dbConn);
            RunImporter(dbConn, "<!-- leading XML comment --> " + EXAMPLE_XML);
            var map = Map.Select(dbConn, MAP_NAME);
            Assert.IsNotNull(map);
            var after = BuildCounts(dbConn);
            AssertIncreases(before, after);
        }

        public void CanImportWithInlineComments()
        {
            var dbConn = ConnectionFactory.Create();
            var before = BuildCounts(dbConn);

            StringBuilder sb = new StringBuilder();
            string[] lines = EXAMPLE_XML.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );
            for (int i = 0; i < lines.Length; i++)
            {
                sb.AppendLine(lines[0]);
                sb.AppendLine("<!-- xml comments are fun " + i + " -->");
            }
            RunImporter(dbConn, sb.ToString());

            var map = Map.Select(dbConn, MAP_NAME);
            Assert.IsNotNull(map);
            var after = BuildCounts(dbConn);
            AssertIncreases(before, after);
        }

        [Test()]
        public void BadXmlThrows()
        {
            var dbConn = ConnectionFactory.Create();
            RunImporterFailingly(dbConn, "bogus", "root level is invalid");
            RunImporterFailingly(dbConn, "<unclosed>", "are not closed");
            RunImporterFailingly(dbConn, "<root>then crap</root>", "crap");
            // we could test bad XML some more, purely, but that's really
            // testing the XML parser, not our code.

            // The other ways in which the XML could be bad include:
            // steps that reference missing queues
            // rules that refererence missing steps
            // step type enums that don't parse
            // queue isfail that doesn't parse?  (This might be reasonable to default to false?)
            // step next for a missing step
            // step without next for a non-terminating step
            // rule comparison that doesn't parse
            // rule with missing value, variable, comparison, next
            // rule, step outside map
            // queue inside map
        }

    }
}
