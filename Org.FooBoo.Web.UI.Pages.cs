/*
 * $Id: Org.FooBoo.Web.UI.Pages.cs 61 2008-08-07 10:13:21Z tmr $
 *
 * Module:  Org.FooBoo.Web.UI.Pages -- description
 * Created: 23-JAN-2008 21:23
 * Author:  tmr
 */

namespace Org.FooBoo.Web.UI.Pages {

  public class GlobalPage: APage {

    public const string txt_PageName = "GlobalPage";

    private UI.IPage m_LoginPage;
    private UI.IPage m_CurrentPage;

    public GlobalPage (ISessionManager smgr, Core.ICore core):
      base (txt_PageName, smgr, core) {}

    public override bool performAction_Is2Generate () {

      string currPage =
        (getSessionManager ()).getParam (UI.Constants.txt_CurrentPage);
      if (currPage == null) currPage = UI.Pages.InvoiceListPage.txt_PageName;
      ISessionManager pageSessionMgr =
        (getSessionManager ()).create4Page (currPage);
      m_LoginPage = new UI.Pages.LoginPage
        ((getSessionManager ()).create4Page (UI.Pages.LoginPage.txt_PageName), getCore ());

      switch (currPage) {
        case UI.Pages.SetupPage.txt_PageName:
          m_CurrentPage = new UI.Pages.SetupPage (pageSessionMgr, getCore ());
          break;
        case UI.Pages.CustomerManagerPage.txt_PageName:
          m_CurrentPage = new UI.Pages.CustomerManagerPage (pageSessionMgr, getCore ());
          break;
        case UI.Pages.CustomerListPage.txt_PageName:
          m_CurrentPage = new UI.Pages.CustomerListPage (pageSessionMgr, getCore ());
          break;
        case UI.Pages.InvoiceManagePage.txt_PageName:
          m_CurrentPage = new UI.Pages.InvoiceManagePage (pageSessionMgr, getCore ());
          break;
        case UI.Pages.LogoutPage.txt_PageName:
          m_CurrentPage = new UI.Pages.LogoutPage (pageSessionMgr, getCore ());
          break;
        case UI.Pages.InvoiceListPage.txt_PageName:
        default:
          m_CurrentPage = new UI.Pages.InvoiceListPage (pageSessionMgr, getCore ());
          break;
      }

      return true;
    }

    public override void generate (ISessionManager smgr) {

      w ("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"xhtml11.dtd\">");
      w (tagBegin ("html"));
      w (tagBegin ("head"));
      w ("<link rel=\"stylesheet\" href=\"" + getDesignPath ("style.css") + "\" type=\"text/css\" />");
      w ("<meta http-equiv=\"content-type\" content=\"text/html; charset=iso-8859-2\" />");
      w (tag ("title", m_CurrentPage.getTitle ()));
      w (tagEnd ("head"));

      w (tagBegin ("body"));
      w (tagBegin ("form", tagArg ("action", getApplicationUrl ()) +
                           tagArg ("name", Constants.txt_FormName) +
                           tagArg ("enctype", "multipart/form-data") +
                           tagArg ("method", "post") +
                           tagArg ("runat", "server")));

      if (m_CurrentPage.getName () == UI.Pages.LogoutPage.txt_PageName) m_CurrentPage.generate ();

      if (m_LoginPage.performAction_Is2Generate ()) m_LoginPage.generate (null);
      else {
        (getSessionManager ()).setGlobalParam (Constants.txt_CurrentPage, m_CurrentPage.getPageName ());

        w (tagBegin ("div", tagArg ("id", "nav_bar")));

        /* Left part of nav_bar */
        w (ul (li (makeReference (getUserText ("InvoiceList"), getUserText ("InvoiceList_Info"),
                                  Constants.txt_FormName,
                                  Constants.txt_CurrentPage,
                                  Pages.InvoiceListPage.txt_PageName),
                   tagArg ("class", "nav_bar_left_item" +
                     ((Pages.InvoiceListPage.txt_PageName == m_CurrentPage.getName ())? "_active" : null))) +
               li (makeReference (getUserText ("CustomerList"), getUserText ("CustomerList_Info"),
                                  Constants.txt_FormName,
                                  Constants.txt_CurrentPage,
                                  Pages.CustomerListPage.txt_PageName),
                   tagArg ("class", "nav_bar_left_item" +
                     ((Pages.CustomerListPage.txt_PageName == m_CurrentPage.getName ())? "_active" : null))) +
               li (makeReference (getUserText ("Setup"), getUserText ("Setup_Info"),
                                  Constants.txt_FormName,
                                  Constants.txt_CurrentPage,
                                  Pages.SetupPage.txt_PageName),
                   tagArg ("class", "nav_bar_left_item" +
                     ((Pages.SetupPage.txt_PageName == m_CurrentPage.getName ())? "_active" : null))),
               tagArg ("id", "nav_bar_left") ));

        /* Right part of nav_bar */
        w (ul (li (makeReference (getUserText ("Help"), getUserText ("Help_Info"),
                                  Constants.txt_FormName,
                                  Constants.txt_CurrentPage,
                                  null),
                   tagArg ("class", "nav_bar_right_item")) + /* XXX: ENOIMP */
               li (makeReference (getUserText ("LogOut"), getUserText ("LogOut_Info"),
                                  Constants.txt_FormName,
                                  Constants.txt_CurrentPage,
                                  Pages.LogoutPage.txt_PageName),
                   tagArg ("class", "nav_bar_right_item")),
               tagArg ("id", "nav_bar_right") ));

        w (tagEnd ("div"));

        m_CurrentPage.generate ();
      }

      w (tagEnd ("form"));
      w (tagEnd ("body"));
      w (tagEnd ("html"));
    }
  }

  public class LoginPage: APage {

    public const string txt_PageName = "LoginPage";

    public LoginPage (ISessionManager smgr, Core.ICore core):
      base (txt_PageName, smgr, core) {}

    public override bool performAction_Is2Generate () {

      ISessionManager smgr = getSessionManager ();

      if (smgr.isAuthentized ()) return false;

      string login    = smgr.getParam ("CredentialsEditor_Login");
      string password = smgr.getParam ("CredentialsEditor_PassWord");

      if ((getCore ()).validateCredentials (login, password)) {
        smgr.setAuthentized (login);
        return false;
      }

      return true;
    }

    public override void generate (ISessionManager smgr) {

      Data.IObject credentials = (getCore ()).createEmptyCredentials ();
      IComponent credentialsEditor = createComponent
        <Components.DataObjectEditor> ("CredentialsEditor", credentials,
            null, false, true, true);
      /* Saving/dropping disabled, we only want l/p to be POSTed back */

      w (tagBegin ("div", tagArg ("id", "login_box")));
        credentialsEditor.generate ();
      w (tagEnd ("div"));
    }
  }

  public class LogoutPage: APage {

    public const string txt_PageName = "LogoutPage";

    public LogoutPage (ISessionManager smgr, Core.ICore core):
      base (txt_PageName, smgr, core) {}

    public override bool performAction_Is2Generate ()
      { (getSessionManager ()).setAuthentized (null); return true; }

    public override void generate (ISessionManager smgr) {}
  }

  public class SetupPage: APage {

    public const string txt_PageName = "SetupPage";

    public SetupPage (ISessionManager smgr, Core.ICore core):
      base (txt_PageName, smgr, core) {}

    public override void generate (ISessionManager smgr) {

      Data.IObject supplier = (getCore ()).getCreateSupplier ();
      IComponent supplierEditor = createComponent
        <Components.DataObjectEditor> ("SupplierEditor", supplier,
            null, false, false, true); /* Dropping is disabled.. */
      supplierEditor.generate ();

      /*
      Data.IObject credentials = (getCore ()).getCreateCredentials ();
      IComponent credentialsEditor = createComponent <Components.DataObjectEditor> ("CredentialsEditor", credentials);
      credentialsEditor.generate ();
      */

      /* More editors for another configuration parameters... */
    }
  }

  public class InvoiceListPage: APage {

    public const string txt_PageName = "InvoiceListPage";

    public InvoiceListPage (ISessionManager smgr, Core.ICore core):
      base (txt_PageName, smgr, core) {}

    public override void generate (ISessionManager smgr) {

      IComponent table = createComponent
        <Components.ObjectSetListing <Data.Definition.Invoice>>
          ("InvoiceListing", Pages.InvoiceManagePage.txt_PageName);
      table.generate ();
    }
  }

  public class InvoiceManagePage: APage {

    public const string txt_PageName = "InvoiceManagePage";

    public InvoiceManagePage (ISessionManager smgr, Core.ICore core):
      base (txt_PageName, smgr, core) {}

    public override void generate (ISessionManager smgr) {

      Data.IObject emptyInvoice = (getCore ()).createEmptyInvoice ();
      Data.IObject invoice      = null;

      string invoiceId = smgr.getParam (emptyInvoice.getKeyFieldName ());

      if (!isNull (invoiceId))
        invoice = (getCore ()).getInvoiceById (invoiceId);

      IComponent editor = createComponent <Components.DataObjectEditor> ("InvoiceEditor",
          ((invoice == null)? emptyInvoice : invoice), InvoiceListPage.txt_PageName);
      editor.generate ();
    }
  }

  public class CustomerListPage: APage {

    public const string txt_PageName = "CustomerListPage";

    public CustomerListPage (ISessionManager smgr, Core.ICore core):
      base (txt_PageName, smgr, core) {}

    public override void generate (ISessionManager smgr) {

      IComponent table = createComponent
        <Components.ObjectSetListing <Data.Definition.Customer>>
          ("CustomerListing", Pages.CustomerManagerPage.txt_PageName);
      table.generate ();
    }
  }

  public class CustomerManagerPage: APage {

    public const string txt_PageName = "CustomerManagerPage";

    public CustomerManagerPage (ISessionManager smgr, Core.ICore core):
      base (txt_PageName, smgr, core) {}

    public override void generate (ISessionManager smgr) {

      Data.IObject emptyCustomer = (getCore ()).createEmptyCustomer ();
      Data.IObject customer      = null;

      string customerId = smgr.getParam (emptyCustomer.getKeyFieldName ());

      if (!isNull (customerId))
        customer = (getCore ()).getCustomerById (customerId);

      IComponent editor = createComponent <Components.DataObjectEditor> ("CustomerEditor",
          ((customer == null)? emptyCustomer : customer), CustomerListPage.txt_PageName);
      editor.generate ();
    }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
