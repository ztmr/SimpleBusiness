/*
 * $Id: Org.FooBoo.Util.cs 45 2008-03-23 00:30:55Z tmr $
 */

namespace Org.FooBoo.Util {

  public class StringList: System.Collections.Specialized.StringCollection {

    public string[] toArray () {

      string[] _array = new string [this.Count];
      this.CopyTo (_array, 0); 

      return _array;
    }
  }

  public class Reflection {

    public static object createGeneric (System.Type generic, System.Type innerType, params object [] args) {

      System.Type specificType = generic.MakeGenericType (new System.Type [] { innerType });
      return System.Activator.CreateInstance (specificType, args);
    }
  }

  public class Misc {

    public static string byte2Hex (byte [] bytes) {

      System.Text.StringBuilder sb = new System.Text.StringBuilder (bytes.Length * 2);

      foreach (byte b in bytes)
        sb.AppendFormat("{0:x2}", b);

      return sb.ToString();
    }

    public static byte [] hex2Byte (string hexString) {

      int n = hexString.Length;

      byte[] bytes = new byte [n / 2];

      for (int i = 0; i < n; i += 2)
        bytes [i / 2] = System.Convert.ToByte (hexString.Substring (i, 2), 16);

      return bytes;
    }
  }

  public class AppEnv {

    public static System.Reflection.Assembly getExeAsmImage ()
      { return System.Reflection.Assembly.GetExecutingAssembly (); }

    public static string getAppImagePath () {

      string asmPath = ((getExeAsmImage ()).GetName ()).CodeBase;

      /* NOTE: Ehm, Windoze... :-) */
      string urlBegin;
      if (Util.Platform.isUnix ()) urlBegin = "file://";
      else                         urlBegin = "file:///";
      asmPath = asmPath.Replace (urlBegin, "");

      return asmPath;
    }

    public static string getAppPath ()
      { return System.IO.Path.GetDirectoryName (getAppImagePath ()); }

    public static string getObjectStoreName ()
      { return System.IO.Path.Combine (getAppRootPath (), "Org.FooBoo.Data.Store.db"); }

    public static string getConfigStoreName ()
      { return System.IO.Path.Combine (getAppRootPath (), "Org.FooBoo.Config.Store.db"); }

    public static string getNodeName ()
      { return System.Environment.MachineName; }

    public static string getUserName ()
      { return System.Environment.UserName; }

    public static string getAppRootPath () {

      /* XXX: just for now :-) */
      return System.IO.Path.Combine (getAppPath (), "../");
    }

    public static string getAppRootSubDir (string name) {

      string path = System.IO.Path.Combine (getAppRootPath (), name);
      FileSystem.createDir (path);

      return path;
    }

    public static string getBinPath () { return getAppPath (); }

    public static string getLogPath () { return getAppRootSubDir ("log"); }
  }

  public class Platform {

    public static bool isUnix () {

      int p = (int) System.Environment.OSVersion.Platform;

      return ((p == 4) || (p == 128));
    }

    public static bool isMono ()
      { return (System.Type.GetType ("Mono.Runtime") != null); }
  }

  public class FileSystem {

    public const int F_OK = 0;
    public const int X_OK = 1;
    public const int W_OK = 2;
    public const int R_OK = 4;

    /* Emulate POSIX access () call */
    /* NOTE: dumb solution, but ACL way is much complicated
     */
    public static bool access (string fileName, int mode) {

      bool res = true;

      if (hasFlag (mode, F_OK)) res &= System.IO.File.Exists (fileName);
      if (hasFlag (mode, X_OK)) res &= true; // Formally :-)

      if (hasFlag (mode, R_OK)) {
        try { System.IO.File.OpenRead (fileName); res &= true; }
        catch (System.Exception) { res &= false; }
      }

      if (hasFlag (mode, W_OK)) {
        try { System.IO.File.OpenWrite (fileName); res &= true; }
        catch (System.Exception) { res &= false; }
      }

      return res;
    }

    private static bool hasFlag (int mode, int flag)
      { return ((mode & flag) == flag); }

    public static bool isSubLocation (string path1, string path2) {

      if (path1 == null || path2 == null) return false;
      if (path1.Length  >= path2.Length)  return false;

      return (path1.StartsWith (path2));
    }

    public static void storeFile (string fileName, byte [] data, bool overWrite) {

      if (access (fileName, F_OK)) {
        if (overWrite) System.IO.File.Delete (fileName);
        else           return;
      }

      System.IO.FileStream fileStream =
        System.IO.File.Create (fileName);

      fileStream.Write (data, 0, data.Length);
      fileStream.Flush (); fileStream.Close ();
    }

    public static void dropFile (string fileName) {

      System.IO.File.Delete (fileName);
    }

    public static void createDir (string dirName) {

      if (!System.IO.Directory.Exists (dirName)) {
        System.IO.Directory.CreateDirectory (dirName);
      }
    }

    public static string getBaseName (string path)
      { return System.IO.Path.GetFileName (path); }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
