/*
 * $Id: Org.FooBoo.Config.cs 44 2008-03-22 02:14:25Z tmr $
 */

namespace Org.FooBoo.Config {

  public class Settings {

    public static string getAppConfig (string key) {

      try {
        return System.Configuration.ConfigurationManager.AppSettings [key];
      }
      catch {
        /* XXX: When the logging is not initialized yet, this will fail */
//        Logging.LogProvider.write (Logging.LogLevel.WARN,
//            "Configuration error: " + ex.Message +
//            "\n" + ex.StackTrace);
        return null;
      }
    }
  }

  public interface IConfig {

    void   set (string key, string val, bool isFull);
    void   set (string key, string val);
    string get (string key, bool isFull);
    string get (string key);

    System.Collections.Generic.IList <ConfigRecord> getList ();
    IConfig getChild (string code);
  }

  public class NameSpace {

    public static string UserText = "UserText";
  }

  public class ConfigRecord {

    private string m_Key;
    private string m_Val;

    public ConfigRecord (string key, string val)
      { m_Key = key; m_Val = val; }

    public ConfigRecord (string key): this (key, null) {}

    public ConfigRecord (): this (null, null) {}

    public string Key { get { return m_Key; } }

    public string Value { get { return m_Val; } }
  }

  public class Config: IConfig {

    private string              m_Code;
    private Data.IObjectStorage m_Database;

    public Config (string code, Data.IObjectStorage db)
      { m_Code = code; m_Database = db; }

    internal string getFullKey (string key) { return m_Code + "_" + key; }

    public void set (string key, string val) { set (key, val, false); }

    public void set (string key, string val, bool isFull) {

      key = isFull ? key : getFullKey (key);

      ConfigRecord cr = getInternal (key, true);
      if (cr != null) m_Database.drop (cr);

      m_Database.store (new ConfigRecord (key, val));
    }

    public string get (string key) { return get (key, false); }

    public string get (string key, bool isFull) {

      ConfigRecord cr = getInternal (key, isFull);

      /* If nothing found, create key to be filled later */
      if (cr == null) {
        set (key, null, isFull);
        return null;
      }

      return cr.Value;
    }

    private ConfigRecord getInternal (string key, bool isFull) {

      key = (isFull ? key : getFullKey (key));

      System.Collections.Generic.IList <ConfigRecord> res =
        m_Database.find <ConfigRecord> (delegate (ConfigRecord cr) { return (cr.Key == key); });
      if (res == null) return null;

      System.Collections.IEnumerator rec = res.GetEnumerator ();
      if (!rec.MoveNext ()) return null;

      return ((ConfigRecord) rec.Current);
    }

    public System.Collections.Generic.IList <ConfigRecord> getList () {

      return m_Database.find <ConfigRecord> (delegate (ConfigRecord cr) { return true; });
    }

    public IConfig getChild (string code)
      { return (new Config (getFullKey (code), m_Database)); }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
