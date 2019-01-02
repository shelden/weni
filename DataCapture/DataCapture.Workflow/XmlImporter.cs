using System;
using System.Xml;
using System.IO;
using System.Data;

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

        #region Behavior
        public void Import(IDbConnection dbConn, FileInfo file)
        {
            int found = 0;
            using (var reader = XmlReader.Create(file.FullName))
            {
                while (reader.Read())
                {
                    // Only detect start elements.
                    if (reader.IsStartElement())
                    {
                        found++;
                        Console.WriteLine("found startline");
                    }
                }
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
