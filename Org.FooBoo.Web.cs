/*
 * $Id: Org.FooBoo.Web.cs 61 2008-08-07 10:13:21Z tmr $
 *
 * Module:  Org.FooBoo.Web -- description
 * Created: 23-JAN-2008 15:57
 * Author:  tmr
 */

namespace Org.FooBoo.Web {

  public interface ISessionManager {

    string getAuthentized4Login ();
    bool   isAuthentized ();
    void   setAuthentized (string login);

    string getUrl ();
    string getSessionInfo ();

    string makeFullName (string key);

    void   setGlobalParam (string key, string val);
    string getGlobalParam (string key);

    void   setParam (string key, string val);
    void   setParam (string key);
    string getParam (string key);

    ISessionManager create4Page (string pageName);
    ISessionManager create4Component (string prefix);

    string getUserText (string code);

    void write (string line);
  }

  public class IndexPage: System.Web.UI.Page {

    override protected void OnInit (System.EventArgs e)
      { InitializeComponent(); base.OnInit (e); }

    private void InitializeComponent ()
      { this.Load += new System.EventHandler (Page_Load); }

    private void Page_Load (object sender, System.EventArgs e) {

      ApplicationCall app = new ApplicationCall (Request, Response, Session,
                                                 Cache, Global.getCore (),
                                                 Global.getConfig ());
      app.process ();
    }
  }

  public class Global: System.Web.HttpApplication {

    private static Data.IObjectStorage m_DataStore;
    private static Data.IObjectStorage m_ConfigStore;
    private static Config.IConfig      m_Config;
    private static Core.ICore          m_Core;

    protected void Application_Start (object o, System.EventArgs e) {

      Logging.LogProvider.init ();
      Logging.LogProvider.write (Logging.LogLevel.INFO, "Starting up application...");

      m_DataStore   = Data.Creator.createStorage (Util.AppEnv.getObjectStoreName ());
      m_ConfigStore = Data.Creator.createStorage (Util.AppEnv.getConfigStoreName ());
      m_Core        = Core.Creator.create (m_DataStore);
      m_Config      = new Config.Config ("", m_ConfigStore);

      m_Core.startUp ();
    }

    public static Core.ICore getCore () { return m_Core; }

    public static Config.IConfig getConfig () { return m_Config; }

    protected void Application_End (object o, System.EventArgs e) {

      m_Core.shutDown (); m_DataStore.close (); m_ConfigStore.close ();
      Logging.LogProvider.write (Logging.LogLevel.INFO, "Shutting down application...");
    }

    protected void Session_Start (object o, System.EventArgs e)
      { Logging.LogProvider.write (Logging.LogLevel.INFO, "Starting up session..."); }

    protected void Session_End (object o, System.EventArgs e)
      { Logging.LogProvider.write (Logging.LogLevel.INFO, "Shutting down session..."); }

    protected void Application_Error (object sender, System.EventArgs e)
      { Logging.LogProvider.write (Logging.LogLevel.ERROR, Server.GetLastError ()); }
  }

  public class GlobalSessionManager: ISessionManager {

    private const string                             txt_IsAuthentized4Login = "IsAuthentized4Login";

    private System.Web.HttpRequest                   m_Request;
    private System.Web.HttpResponse                  m_Response;
    private System.Web.SessionState.HttpSessionState m_Session;
    private System.Web.Caching.Cache                 m_Cache;
    private Config.IConfig                           m_Config;

    public GlobalSessionManager (System.Web.HttpRequest request,
                                 System.Web.HttpResponse response,
                                 System.Web.SessionState.HttpSessionState session,
                                 System.Web.Caching.Cache cache,
                                 Config.IConfig config) {

      m_Request  = request; m_Response = response;
      m_Session  = session; m_Cache    = cache;
      m_Config = config;

      Logging.LogProvider.write (Logging.LogLevel.DEBUG, "Form variables:");
      foreach (string x in m_Request.Form.Keys)
        Logging.LogProvider.write (Logging.LogLevel.DEBUG,
            ">>> " + x + " = \'" + m_Request.Form [x] + "\'");

      Logging.LogProvider.write (Logging.LogLevel.DEBUG, "Session variables:");
      foreach (string x in m_Session.Keys)
        Logging.LogProvider.write (Logging.LogLevel.DEBUG,
            ">>> " + x + " = \'" + System.Convert.ToString (m_Session [x]) + "\'");
    }

    public string getAuthentized4Login () { return (string) m_Session [txt_IsAuthentized4Login]; }

    public bool isAuthentized () { return (getAuthentized4Login () != null); }

    public void setAuthentized (string login) { m_Session [txt_IsAuthentized4Login] = login; }

    public string getUrl () { return m_Request.Url.AbsoluteUri; }

    public string makeFullName (string key) { return "Global_"; }

    public void setGlobalParam (string key, string val) { setParam (key, val); }

    public string getGlobalParam (string key) { return getParam (key); }

    public void setParam (string key, string val)
      { write (UI.AComponent.hidden (key, val)); }

    public void setParam (string key) { setParam (key, null); }

    public string getParam (string key) { return m_Request.Form [key]; }

    public string encode (string txt)
      { return (((txt == null)? null : System.Web.HttpUtility.HtmlEncode (txt))); }

    public void write (string line) { m_Response.Write (line + "\n"); }

    public string getSessionInfo ()
      { return System.String.Format (
          "[ ref=\"{0}\"; agent=\"{1}\"; addr=\"{2}\"; name=\"{3}\"; ]",
          m_Request.UrlReferrer, m_Request.UserAgent,
          m_Request.UserHostAddress, m_Request.UserHostName); }

    public ISessionManager create4Page (string pageName)
      { return new ComponentSessionManager (pageName, this); }

    public ISessionManager create4Component (string prefix)
      { return new ComponentSessionManager (prefix, this); }

    public string getUserText (string code) {

      Config.IConfig conf = m_Config.getChild (Config.NameSpace.UserText);
      string text = conf.get (code);

      if (text == null) conf.set (code, "UT_" + code);

      return conf.get (code);
    }
  }

  public class ComponentSessionManager: ISessionManager {

    private string          m_ComponentName;
    private ISessionManager m_Parent;

    public ComponentSessionManager (string name, ISessionManager parent)
      { m_ComponentName = name; m_Parent = parent; }

    public string getUrl () { return m_Parent.getUrl (); }

    public string getSessionInfo () { return m_Parent.getSessionInfo (); }

    public bool isAuthentized () { return m_Parent.isAuthentized (); }

    public void setAuthentized (string login) { m_Parent.setAuthentized (login); }

    public string getAuthentized4Login () { return m_Parent.getAuthentized4Login (); }

    public string makeFullName (string key)
      { return (m_ComponentName + ((key != null) ? ("_" + key) : null)); }

    public void setGlobalParam (string key, string val)
      { m_Parent.setGlobalParam (key, val); }

    public string getGlobalParam (string key)
      { return m_Parent.getGlobalParam (key); }

    public void setParam (string key, string val)
      { m_Parent.setParam (makeFullName (key), val); }

    public void setParam (string key)
      { m_Parent.setParam (makeFullName (key)); }

    public string getParam (string key)
      { return m_Parent.getParam (makeFullName (key)); }

    public ISessionManager create4Page (string pageName)
      { return m_Parent.create4Page (pageName); }

    public ISessionManager create4Component (string prefix)
      { return m_Parent.create4Component (makeFullName (prefix)); }

    public void write (string line) { m_Parent.write (line); }

    public string getUserText (string key)
      { return m_Parent.getUserText (makeFullName (key)); }
  }

  public class ApplicationCall {

    private ISessionManager m_SessionManager;
    private Core.ICore      m_Core;

    public ApplicationCall (System.Web.HttpRequest request,
                            System.Web.HttpResponse response,
                            System.Web.SessionState.HttpSessionState session,
                            System.Web.Caching.Cache cache,
                            Core.ICore core,
                            Config.IConfig config) {

      m_Core           = core;
      m_SessionManager = new GlobalSessionManager (request, response, session, cache, config);
    }

    public void process () {

      Logging.LogProvider.write (Logging.LogLevel.INFO,
          "Processing request: " + m_SessionManager.getSessionInfo ());

      (new UI.Pages.GlobalPage (m_SessionManager, m_Core)).generate ();
    }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
