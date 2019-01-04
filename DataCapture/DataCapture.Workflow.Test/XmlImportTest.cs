using System;
using System.IO;
using DataCapture.Workflow.Db;
using DataCapture.IO;
using NUnit.Framework;

namespace DataCapture.Workflow.Test
{
    public class XmlImportTest
    {
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
            int beforeMaps = DbUtil.SelectCount(dbConn, Map.TABLE);
            int beforeSteps = DbUtil.SelectCount(dbConn, Step.TABLE);
            int beforeRules = DbUtil.SelectCount(dbConn, Rule.TABLE);
            var map = Map.Select(dbConn, MAP_NAME);
            int beforeMapVersion = map == null ? Map.VERSION - 1 : map.Version;
            using (var tmp = new TempFile(".xml"))
            {
                using (var writer = new System.IO.StreamWriter(tmp.FullName))
                {
                    writer.WriteLine(EXAMPLE_XML);
                }
                var importer = new XmlImporter();
                importer.Import(dbConn, tmp.Value);
            }
            map = Map.Select(dbConn, MAP_NAME);
            int afterMaps = DbUtil.SelectCount(dbConn, Map.TABLE);
            int afterSteps = DbUtil.SelectCount(dbConn, Step.TABLE);
            int afterRules = DbUtil.SelectCount(dbConn, Rule.TABLE);
            // don't check queues, because it's ok if they already exist
            // by name...making this quick & dirty technique not quite 
            // right
            Assert.AreEqual(beforeMaps + 1, afterMaps); // example has 1 map
            Assert.AreEqual(beforeSteps + 4, afterSteps); // example has 4 steps
            Assert.AreEqual(beforeRules + 2, afterRules); // example has 2 rules
            Assert.AreEqual(beforeMapVersion + 1, map.Version);
        }

    }
}
