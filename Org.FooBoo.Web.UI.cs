/*
 * $Id: Org.FooBoo.Web.UI.cs 61 2008-08-07 10:13:21Z tmr $
 *
 * Module:  Org.FooBoo.Web.UI -- description
 * Created: 23-JAN-2008 15:20
 * Author:  tmr
 */

namespace Org.FooBoo.Web.UI {

  public class Constants {

    public const string txt_CurrentPage   = "CurrentPage";
    public const string txt_FormName      = "MainAppForm";
    public const string txt_MandatorySign = "*";
  }

  public interface IComponent {

    string getName ();
    string getTitle ();
    string getParentPageName ();

    bool   isError ();

    Core.ICore getCore ();

    /* This calls performAction_Is2Generate and then generate (ISessionManager) */
    void   generate ();

    void   generate (Web.ISessionManager smgr);
    bool   performAction_Is2Generate ();
  }

  public interface IPage: IComponent {

    string getPageName ();
  }

  public abstract class AComponent: IComponent {

    private string              m_ComponentName;
    private Web.ISessionManager m_SessionManager;
    private IComponent          m_Parent;

    public AComponent (string name, IComponent parent, Web.ISessionManager smgr)
      { m_ComponentName = name; m_Parent = parent; m_SessionManager = smgr; }

    public string getName () { return m_ComponentName; }

    public virtual string getTitle () { return null; }

    public virtual bool performAction_Is2Generate () { return true; }

    protected void w (string str) { m_SessionManager.write (str); }

    protected ISessionManager getSessionManager () { return m_SessionManager; }

    protected IComponent createComponent <T> (string name, params object [] constructArgs) {

      int baseArgCnt = 3;
      object [] args = new object [constructArgs.Length +baseArgCnt];

      constructArgs.CopyTo (args, baseArgCnt);
      args [0] = name;
      args [1] = this;
      args [2] = (getSessionManager ()).create4Component (name);

      string call = "new " + System.Convert.ToString (typeof (T)) + " (";
      for (int i = 0; i < args.Length; i++) {
        call += System.Convert.ToString (args [i]);
        call += (i < args.Length -1)? ", " : ");";
      }
      Logging.LogProvider.write (Logging.LogLevel.DEBUG, "Call: " + call);

      return (IComponent) System.Activator.CreateInstance (typeof (T), args);
    }

    public virtual Core.ICore getCore () { return m_Parent.getCore (); }

    public abstract void generate (ISessionManager smgr);

    public virtual bool isError () { return false; }

    public virtual string getParentPageName () { return m_Parent.getParentPageName (); }

    public void generate ()
      { if (performAction_Is2Generate ()) generate (getSessionManager ()); }

    protected string getUserText (string key) { return m_SessionManager.getUserText (key); }

    protected string getDesignPath (string fileName) { return "design/" + fileName; }

    protected string getApplicationUrl () { return (getSessionManager ()).getUrl (); }

    public static bool isNull (string val) { return (val == null || val == ""); }

    public static string tag (string name, string body)
      { return tag (name, body, null); }

    public static string tag (string name, string body, string args)
      { return (tagBegin (name, args) +
                  ((body == null)? "&nbsp;" : body) +
                tagEnd (name)); }

    public static string basicTag (string name, string args)
      { return ("<" + name + " " + args + "/>"); }

    public static string tagBegin (string name) { return tagBegin (name, null); }

    public static string tagBegin (string name, string args)
      { return ("<" + name + " " + args + ">"); }

    public static string tagEnd (string name)
      { return ("</" + name + ">"); }

    public static string tagArg (string key) { return tagArg (key, null); }

    public static string tagArg (string key, string val)
      { return (key + ((val != null)? ("=\"" + val + "\" ") : " ")); }

    public static string tableBegin (string name) { return tableBegin (name, null); }

    public static string tableBegin (string name, string args)
      { return tagBegin ("table", tagArg ("name", name) + args); }

    public static string tableEnd ()
      { return tagEnd ("table"); }

    public static string tr (string body, string args)
      { return tag ("tr", body, args); }

    public static string tr (string body)
      { return tr (body, null); }

    public static string trBegin () { return trBegin (null); }

    public static string trBegin (string args)
      { return tagBegin ("tr", args); }

    public static string trEnd () { return tagEnd ("tr"); }

    public static string td (string body, string args)
      { return tag ("td", body, args); }

    public static string td (string body) { return td (body, null); }

    public static string tdBegin () { return tdBegin (null); }

    public static string tdBegin (string args)
      { return tagBegin ("td", args); }

    public static string tdEnd () { return tagEnd ("td"); }

    public static string ul (string body) { return ul (body, null); }

    public static string ul (string body, string args)
      { return tag ("ul", body, args); }

    public static string li (string body)
      { return tag ("li", body, null); }

    public static string li (string body, string args)
      { return tag ("li", body, args); }

    public static string textArea (string val, string args)
      { return tag ("textarea", val, args); }

    public static string input (string type, string name, string val)
      { return input (type, name, val, false); }

    public static string input (string type, string name, string val, bool isReadOnly)
      { return basicTag ("input", tagArg ("type", type) +
                                  tagArg ("name", name) +
                                  tagArg ("value", val) +
                                  (isReadOnly? tagArg ("readonly") : "")); }

    public string button (string name, string val)
      { return input ("submit", (getSessionManager ()).makeFullName (name), val, false); }

    public static string hidden (string name, string val)
      { return input ("hidden", name, val, false); }

    public string textBox (string name, string val)
      { return textBox (name, val, false); }

    public string textBox (string name, string val, bool isReadOnly)
      { return input ("text", (getSessionManager ()).makeFullName (name), val, isReadOnly); }

    public string comboBox (string name, string items) { return comboBox (name, items); }

    public string comboBox (string name, string items, string args)
      { return tag ("select", items, tagArg ("name", (getSessionManager ()).makeFullName (name)) + args); }

    public string makeReference (string body, string title, string formName,
                                 string property, string val) {

      return (tag ("a", body, tagArg ("href", getApplicationUrl ()) + tagArg ("title", title) +
              makeArgJSRef (formName, property, val)));
    }

    public string image (string name) { return basicTag ("img", tagArg ("src", getDesignPath (name))); }

    public string imageButton (string inactive, string active, string help, string param, string val) {
      return tag ("img", help, tagArg ("src", getDesignPath (inactive)) + tagArg ("alt", help) +
          tagArg ("onMouseOver", "src=\'" + getDesignPath (active) + "\';") +
          tagArg ("onMouseOut", "src=\'" + getDesignPath (inactive) + "\';") +
          makeArgJSRef (Constants.txt_FormName, param, val));
    }

    public static string makeJSSubmitCall (string formName)
      { return ("document." + formName + ".submit (); return (false); "); }

    public static string makeJSPropertyAssign (string formName, string property, string val)
      { return ("document." + formName + "." + property + ".value = \'" + val + "\'; "); }

    public static string makeJSOnClick (string body)
      { return (tagArg ("style", "cursor:hand") + tagArg ("onClick", body)); }

    public static string makeArgJSRef (string formName, string property, string val)
      { return makeJSOnClick (makeJSPropertyAssign (formName, property, val) +
                              makeJSSubmitCall (formName)); }

    public static string makeArgJSRef (string formName, string prop1, string val1,
                                       string prop2, string val2)
      { return makeJSOnClick (makeJSPropertyAssign (formName, prop1, val1) +
                              makeJSPropertyAssign (formName, prop2, val2) +
                              makeJSSubmitCall (formName)); }

    public static string makeJSScript (string scriptBody)
      { return (tag ("script", scriptBody, tagArg ("type", "text/javascript"))); }

    /* XXX: bind it after generate... */
    /* XXX: generate error checking -- if there was an error, do not change page...*/
    protected virtual void afterAction () { changePage (null); }

    /* Generate change page script, page will be changed after submit */
    protected void changePage (string page) {

      if (page != null)
        w (makeJSScript (makeJSPropertyAssign (Constants.txt_FormName,
                Constants.txt_CurrentPage, page) +
              makeJSSubmitCall (Constants.txt_FormName)));
    }
  }

  public abstract class APage: AComponent, IPage {

    private Core.ICore m_Core;

    public APage (string name, ISessionManager smgr, Core.ICore core):
      base (name, null, smgr) { m_Core = core; }

    public override Core.ICore getCore () { return m_Core; }

    public string getPageName () { return getName (); }

    public override string getParentPageName () { return getPageName (); }

    public override string getTitle () { return getUserText (getPageName () + "_title"); }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
