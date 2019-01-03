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
        public static readonly String EXAMPLE_XML = ""
            + "<map name='" + TestUtil.NextString() + "' > " + Environment.NewLine
            + "<step name='end'     type='Terminating'               /> " + Environment.NewLine
            + "<step name='middle1' type='Standard'    next='end'     /> " + Environment.NewLine
            + "<step name='middle0' type='Standard'    next='end'     /> " + Environment.NewLine
            + "<step name='start'   type='Start'       next='middle0' /> " + Environment.NewLine
            + "</map>" + Environment.NewLine
            + ""
            ;

        #endregion

        [Test()]
        public void CanImport()
        {
            using (var tmp = new TempFile(".xml"))
            {
                using (var writer = new System.IO.StreamWriter(tmp.FullName))
                {
                    writer.WriteLine(EXAMPLE_XML);
                }
                var importer = new XmlImporter();
                importer.Import(ConnectionFactory.Create(), tmp.Value);
            }
        }

    }
}
