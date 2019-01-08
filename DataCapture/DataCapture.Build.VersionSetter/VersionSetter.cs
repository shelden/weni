using System;
using System.IO;
using System.Text;
using DataCapture.IO;

namespace DataCapture.Build.VersionSetter
{
    public class VersionSetter
    {
        #region constants
        public static readonly int THIS_YEAR = DateTime.Now.Year;
        public static readonly String PREFIX_ANY = "[assembly: ";
        public static readonly String PREFIX_ASSEMBLY = PREFIX_ANY + "Assembly";
        #endregion

        #region members
        private String version_ = "0.10.*";
        private String company_ = "Vermont Purple, LLC";
        #endregion

        #region properties
        public String Version
        {
            get { return version_; }
            set { version_ = value; }
        }
        public String Company
        {
            get { return company_; }
            set { company_ = value; }
        }
        #endregion

        #region constructor
        public VersionSetter(String version)
        {
            version_ = version;
        }
        public VersionSetter(String version, String company)
            : this(version)
        {
            company_ = company;
        }
        #endregion

        #region behavior
        public void EditInPlace(FileInfo fileToEdit)
        {
            var tmp = new TempFile(".cs");
            String line;
            var input = new System.IO.StreamReader(fileToEdit.FullName);
            var output = new System.IO.StreamWriter(tmp.Value.FullName);
            while ((line = input.ReadLine()) != null)
            {
                String original = line;
                if (string.IsNullOrEmpty(line)
                    || string.IsNullOrWhiteSpace(line)
                    || line.StartsWith("//", StringComparison.CurrentCulture)
                    )
                { /* no code*/ }
                else if (line.StartsWith(PREFIX_ASSEMBLY, StringComparison.CurrentCulture))
                {
                    String key = GetKey(line);
                    String value = GetValue(line);
                    switch (key)
                    {
                        case "AssemblyCompany":
                            line = MakeLine(key, Company);
                            break;
                        case "AssemblyCopyright":
                            int previous = ExtractYear(value);
                            line = MakeLine(key, GetCopyright(previous));
                            break;
                        case "AssemblyVersion":
                            line = MakeLine(key, Version);
                            break;
                        default:
                            // attribute we don't care about.  Skip, writing
                            // incoming line
                            break;
                    }
                }
                if (!original.Equals(line))
                {
                    Console.WriteLine(this.GetType().Name + ": Changing line:");
                    Console.WriteLine("from " + original);
                    Console.WriteLine("to   " + line);
                }
                output.WriteLine(line);
            }
            input.Close();
            output.Close();
            fileToEdit.Delete();
            tmp.Value.MoveTo(fileToEdit.FullName);
        }
        public static int ExtractYear(String value)
        {
            int twoIndex = value.IndexOf('2');
            if (twoIndex < 0) return THIS_YEAR;

            // make sure have enough length in value for yyyy:
            if (value.Length < (twoIndex + 4)) return THIS_YEAR;

            // ...and yyyy starts with 20xx:
            if (value[twoIndex + 1] != '0') return THIS_YEAR;
            
            // should check IsDigit on the last two digits but meh?  Or maybe
            // just parse the thing?
            return 2000
                + 10 * (int)(value[twoIndex + 2] - '0')
                + 1 * (int)(value[twoIndex + 3] - '0')
                ;
        }
        public String GetCopyright(int startYyyy)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Copyright (C) ");
            if (startYyyy >= THIS_YEAR)
            {
                sb.Append(startYyyy);
            }
            else
            {
                sb.Append(startYyyy);
                sb.Append(" - ");
                sb.Append(THIS_YEAR);
            }
            sb.Append(' ');
            sb.Append(Company);
            return sb.ToString();
        }
        public static String MakeLine(String key, String value)
        {
            var sb = new StringBuilder();
            sb.Append(PREFIX_ANY);
            sb.Append(key);
            sb.Append("(\"");
            sb.Append(value);
            sb.Append("\")]");
            return sb.ToString();
        }
        private static String GetKey(String line)
        {
            // assumes it already starts with the [assembly: prefix
            int length = line.Substring(PREFIX_ANY.Length).IndexOf('(');
            if (length < 0) return "";
            return line.Substring(PREFIX_ANY.Length, length);
        }
        private static String GetValue(String line)
        {
            int parenIndex0 = line.IndexOf('\"');
            if (parenIndex0 < 0) return "";
            int parenIndex1 = line.LastIndexOf('\"');
            if (parenIndex1 < 0) return "";
            if (parenIndex1 <= parenIndex0) return "";

            return line.Substring(parenIndex0 + 1, parenIndex1 - parenIndex0 - 1);
        }

        #endregion

        #region ToString()
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("VersionSetter");
            sb.Append(" for v");
            sb.Append(this.Version);
            sb.Append(' ');
            sb.Append(this.GetCopyright(2018));
            return sb.ToString();
        }
        #endregion

    }
}
