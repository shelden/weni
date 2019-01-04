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
        public static readonly String EXAMPLE_XML = ""
        + "<anything> " + NL
        + "  <queue name='q.everything' /> " + NL
        + "  <map name='m.example' >  " + NL
        + "    <step name='s.end' " + NL
        + "          type='Terminating' " + NL
        + "          queue='q.everything' " + NL
        + "     />  " + NL
        + "    <step name='s.middle1' " + NL
        + "          type='Standard' " + NL
        + "          queue='q.everything' " + NL
        + "          next='s.end' " + NL
        + "     />  " + NL
        + "    <step name='s.middle0' " + NL
        + "          type='Standard' " + NL
        + "          queue='q.everything' " + NL
        + "          next='s.middle1' " + NL
        + "     />  " + NL
        + "    <step name='s.start' " + NL
        + "          type='Terminating' " + NL
        + "          queue='q.everything' " + NL
        + "          next='s.middle0' " + NL
        + "     />  " + NL
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
            using (var tmp = new TempFile(".xml"))
            {
                using (var writer = new System.IO.StreamWriter(tmp.FullName))
                {
                    writer.WriteLine(EXAMPLE_XML);
                }
                var importer = new XmlImporter();
                importer.Import(dbConn, tmp.Value);
            }
            int afterMaps = DbUtil.SelectCount(dbConn, Map.TABLE);
            int afterSteps = DbUtil.SelectCount(dbConn, Step.TABLE);
            int afterRules = DbUtil.SelectCount(dbConn, Rule.TABLE);
            // don't check queues, because it's ok if they already exist 
            Assert.AreEqual(beforeMaps + 1, afterMaps); // example has 1 map
            Assert.AreEqual(beforeSteps + 4, afterSteps); // example has 4 steps
            Assert.AreEqual(beforeRules + 0, afterRules); // example will have rules...but doesn't yet
        }

    }
}
