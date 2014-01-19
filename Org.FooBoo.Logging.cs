/*
 * $Id: Org.FooBoo.Logging.cs 58 2008-08-02 22:06:05Z tmr $
 */

namespace Org.FooBoo.Logging {

  public enum LogLevel: int
    { DEBUG = 0, INFO = 1, WARN = 2, ERROR = 3, FAIL = 4 };

  public interface ILogger {

    void write (LogLevel level, string caller,
                string message, params object [] args);
  }

  public class LogProvider {

    private static System.Collections.ArrayList m_Loggers;
    private static bool                         m_AfterInit = false;

    private class OutputMode {
      public const string Console  = "CONSOLE";
      public const string File     = "FILE";
      public const string Combined = "COMBINED";
    }

    public static void init () {

      if (m_AfterInit) return;

      m_Loggers = new System.Collections.ArrayList ();

      string logOutput = Config.Settings.getAppConfig ("LogOutput");

      if (logOutput == null) logOutput = OutputMode.Combined;
      else                   logOutput = logOutput.ToUpper ();

      switch (logOutput) {
        case OutputMode.Console:
          m_Loggers.Add (new ConsoleLogger ());
          break;

        case OutputMode.File:
          m_Loggers.Add (new FileLogger ());
          break;

        case OutputMode.Combined:
        default:
          m_Loggers.Add (new ConsoleLogger ());
          m_Loggers.Add (new FileLogger ());
          break;
      }

      m_AfterInit = true;
      write ("LogProvider initialized.");
    }

    private static void testAfterInit () {

      if (!m_AfterInit)
        throw new Exceptions.InternalError ("not initialized!");
    }

    private static string getCaller (int depth) {

      System.Diagnostics.StackFrame sf;
      System.Reflection.MethodBase mb;

      sf = new System.Diagnostics.StackFrame (depth);
      mb = sf.GetMethod ();

      return mb.DeclaringType.Namespace + "." +
             mb.DeclaringType.Name + ":" + mb.Name;
    }

    public static void write (LogLevel level, string message,
                              params object [] args) {

      testAfterInit ();
      foreach (ILogger logger in m_Loggers)
        lock (logger) {
          logger.write (level, getCaller (2), message, args);
        }
    }

    public static void write (string message, params object [] args) {

      testAfterInit ();
      foreach (ILogger logger in m_Loggers)
        lock (logger) {
          logger.write (LogLevel.INFO, getCaller (2), message, args);
        }
    }

    public static void write (LogLevel level, System.Exception ex)
      { write (level, "Caught: " + ex.Message + "\n\n" + ex.StackTrace); }

    public static void write (System.Exception ex)
      { write (LogLevel.WARN, ex); }
  }

  internal abstract class ALogger: ILogger {

    private string [] m_Levels;
    private LogLevel  m_Level;
    private string    m_TimeFormat;
    private string    m_MessageFormat;

    internal void init () {

      m_Levels = new string [5];

      m_Levels [(int) LogLevel.DEBUG] = "DEBUG";
      m_Levels [(int) LogLevel.INFO]  = "INFO";
      m_Levels [(int) LogLevel.WARN]  = "WARN";
      m_Levels [(int) LogLevel.ERROR] = "ERROR";
      m_Levels [(int) LogLevel.FAIL]  = "FAIL";

      m_Level         = str2Level (Config.Settings.getAppConfig ("LogLevel"));

      m_TimeFormat    = Config.Settings.getAppConfig ("LogTimeFormat");
      m_TimeFormat    = (m_TimeFormat != null) ? m_TimeFormat : "HH:mm:ss.ffff";

      m_MessageFormat = getMessageFormat (Config.Settings.getAppConfig ("LogMessageFormat"));
    }

    private string getMessageFormat (string fmt) {

      fmt = (fmt != null) ? fmt : "~T: [~L]: ~M: ~E";
      fmt = fmt.Replace ("~T", "{0}");
      fmt = fmt.Replace ("~L", "{1}");
      fmt = fmt.Replace ("~M", "{2}");
      fmt = fmt.Replace ("~E", "{3}");

      return fmt;
    }

    internal string level2Str (LogLevel level) { return m_Levels [(int) level]; }

    internal LogLevel str2Level (string level) {

      if (level == null) return LogLevel.INFO;
      level = level.ToUpper ();

      for (int i = 0; i < 5; i++)
        if (m_Levels [i] == level) return ((LogLevel) i);

      return LogLevel.INFO;
    }

    internal bool is2Write (LogLevel level) { return (level >= m_Level); }

    private string formatMessage (string level, string caller,
                                  string message, params object [] args) {

      System.DateTime dt = System.DateTime.Now;
      message            = System.String.Format (message, args);

      return System.String.Format (m_MessageFormat,
                                   dt.ToString (m_TimeFormat),
                                   level, caller, message);
    }

    internal abstract void writeAction (string message);

    public void write (LogLevel level, string caller,
                       string message, params object [] args) {

      if (is2Write (level))
        writeAction (formatMessage (level2Str (level), caller, message, args));
    }
  }

  internal class FileLogger: ALogger {

    public FileLogger () { init (); }

    internal override void writeAction (string message) {

      System.DateTime dt = System.DateTime.Now;
      string dir         = Config.Settings.getAppConfig ("LogDirectory");
      string filePath    = System.IO.Path.Combine (
          ((System.IO.Directory.Exists (dir)) ? dir : Util.AppEnv.getLogPath ()),
          dt.ToString ("yyyy_MM_dd") + ".log");

      if (!System.IO.File.Exists (filePath)) {
        System.IO.FileStream fs = System.IO.File.Create (filePath);
        fs.Close ();
      }

      System.IO.StreamWriter sw = System.IO.File.AppendText (filePath);
      sw.WriteLine (message);
      sw.Flush ();
      sw.Close ();
    }
  }

  internal class ConsoleLogger: ALogger {

    public ConsoleLogger () { init (); }

    internal override void writeAction (string message)
      { System.Console.WriteLine (message); }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
