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
    private String m_version = "0.10.*";
    private String m_company = "Vermont Purple, LLC";
    #endregion

    #region properties
    public String Version
    {
      get { return m_version; }
      set { m_version = value; }
    }
    public String Company
    {
      get { return m_company; }
      set { m_company = value; }
    }
    #endregion

    #region constructor
    public VersionSetter(String version)
    {
      m_version = version;
    }
    public VersionSetter(String version, String company)
        : this(version)
    {
      m_company = company;
    }
    #endregion

    #region behavior
    /// <summary>
    /// Edits a .cs AssemblyVersion file in place, setting its
    /// copyright and assembly version attributes to those
    /// you used when you created the object.
    /// </summary>
    /// <param name="fileToEdit">File to edit.</param>
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
    /// <summary>
    /// Extracts the start year from a copyright string.  I.e.,
    /// Copyright (C) 2033: returns 2033
    /// Copyright (C) 2001 - 2010: returns 2001
    /// Assumes nothing older than 2000 for now
    /// </summary>
    /// <returns>The year.</returns>
    /// <param name="value">Value.</param>
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
    /// <summary>
    /// Builds the string we put into copyright statements.
    /// </summary>
    /// <returns>Copyright (C) start - now Your Company, Inc</returns>
    /// <param name="startYyyy">the four digit start year</param>
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
    /// <summary>
    /// Creates a key / value assembly version attribute file line
    /// </summary>
    /// <returns>The line.</returns>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
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
    /// <summary>
    /// Extracts the key portion of from an assembly version attribute
    /// file line.
    /// assumes it already starts with the [assembly: prefix
    /// </summary>
    /// <returns>The key.</returns>
    /// <param name="line">Line.</param>
    private static String GetKey(String line)
    {
      int length = line.Substring(PREFIX_ANY.Length).IndexOf('(');
      if (length < 0) return "";
      return line.Substring(PREFIX_ANY.Length, length);
    }
    /// <summary>
    /// Extracts the key portion of from an assembly version attribute
    /// file line.
    /// assumes it already starts with the [assembly: prefix
    /// </summary>
    /// <returns>The key.</returns>
    /// <param name="line">Line.</param>
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
