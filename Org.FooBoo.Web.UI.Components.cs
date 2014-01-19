/*
 * $Id: Org.FooBoo.Web.UI.Components.cs 61 2008-08-07 10:13:21Z tmr $
 *
 * Module:  Org.FooBoo.Web.UI.Components -- description
 * Created: 25-JAN-2008 11:32
 * Author:  tmr
 */

namespace Org.FooBoo.Web.UI.Components {

  public class ObjectSetListing <T>: AComponent {

    private Data.IObjectSet m_ObjectSet;
    private string          m_RefTargetPage;

    public ObjectSetListing (string name, IComponent parent,
                             ISessionManager smgr, string rowRefTarget):
      base (name, parent, smgr)
        { m_RefTargetPage = rowRefTarget; m_ObjectSet = findData (); }

    public void setFilter (Data.IObjectSetFilter filter)
      { m_ObjectSet.setFilter (filter); }

    public void setOrder (Data.IObjectSetOrder order)
      { m_ObjectSet.setOrder (order); }

    private Data.IObjectSet findData () {

      System.Collections.Generic.IList <T> res =
        (getCore ()).getDataStore ().find <T>
          (delegate (T inv) { return true; }); 

      return (new Data.Abstraction.ObjectSet ((getCore ()).getDataStore (), res.GetEnumerator ()));
    }

    private void generateHeader (Data.IObject dataObject) {

      string [] fieldNames = dataObject.getAllFieldNames ();
      w (trBegin (tagArg ("class", "listing_title")));
        w (td (getUserText ("title"), tagArg ("colspan", fieldNames.Length.ToString ())));
      w (trEnd ());
      w (trBegin (tagArg ("class", "listing_header")));
      foreach (string fieldName in fieldNames)
        w (td (getUserText (fieldName)));
      w (trEnd ());
    }

    private string makeRowRef (Data.IObject dataObject, string paramName) {

      /* If the RefTargetPage is not specified, the list is read-only */
      if (m_RefTargetPage == null) return null;
      else return makeArgJSRef (Constants.txt_FormName,
          Constants.txt_CurrentPage, m_RefTargetPage, paramName,
          (dataObject.getField (dataObject.getKeyFieldName ())).getValueString ());
    }

    private void generateRow (Data.IObject dataObject, int i, string paramName) {

      w (trBegin (tagArg ("class", ((i++ % 2 == 0)? "listing_roweven" : "listing_rowodd")) +
                  makeRowRef (dataObject, paramName)));
      foreach (Data.IObjectField field in dataObject.getAllFields ())
        w (td (field.getValueString ()));
      w (trEnd ());
    }

    private void generateMenu () {

      /* If the RefTargetPage is not specified, the list is read-only */
      if (m_RefTargetPage == null) return;
      w (tableBegin (getName () + "_Menu", tagArg ("class", "listing_menu")));
        w (trBegin ());
          w (td (imageButton ("btn_create_inactive.gif", "btn_create_active.gif",
                  getUserText ("CreateItem"),
                  Constants.txt_CurrentPage, m_RefTargetPage),
                tagArg ("class", "listing_menu_item")));
          w (td (imageButton ("btn_filter_inactive.gif", "btn_filter_active.gif",
                  getUserText ("FilterItems"),
                  Constants.txt_CurrentPage, getParentPageName ()),
                tagArg ("class", "listing_menu_item")));
        w (trEnd ());
      w (tableEnd ());
    }

    public override void generate (ISessionManager smgr) {

      int i = 0; string paramName = string.Empty;
      generateMenu ();
      w (tableBegin (getName () + "_Table", tagArg ("class", "listing")));
      while (m_ObjectSet != null && m_ObjectSet.next ()) {
        Data.IObject dataObject =
          m_ObjectSet.getObject ();

        if (m_ObjectSet.isFirst ()) {
          paramName = m_RefTargetPage + "_" +
                      dataObject.getKeyFieldName ();
          smgr.setGlobalParam (paramName, "");
          generateHeader (dataObject);
        }

        generateRow (dataObject, i++, paramName);
      }
      w (tableEnd ());
    }
  }

  public class DataObjectSelector: AComponent {

    private Data.IObjectSet m_DataObjectSet;
    private object          m_Selected;

    public DataObjectSelector (string name, IComponent parent,
                               ISessionManager smgr,
                               Data.IObjectSet dataObjectSet,
                               object selected): base (name, parent, smgr)
      { m_DataObjectSet = dataObjectSet; m_Selected = selected; }

    public override void generate (ISessionManager smgr) {

      string items = "";
      if (m_DataObjectSet != null) {
        while (m_DataObjectSet.next ()) {
          Data.IObject itemObj = m_DataObjectSet.getObject ();
          if (itemObj == null) continue;
          object itemRawObj = itemObj.getRawObject ();
          string itemName = (itemRawObj == null)? "--" : System.Convert.ToString (itemRawObj);
          string itemId   = (itemObj.isEnum ())? itemName : (itemObj.getKeyFieldVal ()).getValueString ();
          items += (tag ("option", itemName, tagArg ("value", itemId) +
             ((m_Selected == itemRawObj)? tagArg ("selected", null) : null)));
          /* XXX: selected, the default item -- sometimes is not working */
        }
      }
      w (comboBox (null, items, tagArg ("class", "selectbox")));
    }
  }

  /*
  public class DataMainEditor <T>: DataObjectEditor {

    public DataMainEditor (string name, IComponent parent,
        ISessionManager smgr): base (name, parent, smgr, null)
      { setDataObject (findCreateObject ()); }

    private object getObjectId () {

      Data.IObject obj = getCreateEmptyObject ();
      string keyFieldName = obj.getKeyFieldName ();
      string keyFormFieldName = getParentPageName () + "_" + keyFieldName;
      (obj.getKeyFieldVal ()).setValue ((getSessionManager ()).getGlobalParam (keyFormFieldName));

      return (obj.getKeyFieldVal ()).getValue ();
    }

    private Data.IObject findCreateObject () {

      object id = null;
      Data.IObject res = findObjectById (id);

      return ((res != null)? res : getCreateEmptyObject ());
    }

    private Data.IObject getCreateEmptyObject () {

      // XXX: ENOIMP
      return null;
    }

    private Data.IObject findObjectById (object id) {

      System.Collections.Generic.IList <T> res =
        ((getCore ()).getDataStore ()).find <T>
          (delegate (T obj) {
             Data.IObject iobj = new Data.Abstraction.Object ((getCore ()).getDataStore (), obj);
             object keyVal = (iobj.getKeyFieldVal ()).getValue ();
             return (keyVal == id);
           });

      if (res.Count <= 0) return null;

      return new Data.Abstraction.Object ((getCore ()).getDataStore (), res [0]);
    }
  }
  */

  public class DataObjectEditor: AComponent {

    private Data.IObject    m_DataObject;
    private Util.StringList m_ErrorFields;
    private bool            m_IsSaved;
    private bool            m_IsDeleted;
    private bool            m_IsSubEditor;
    private string          m_CallBackPage;
    private bool            m_SavingDisabled;
    private bool            m_DroppingDisabled;
    private System.Collections.Generic.Dictionary <string, DataObjectEditor> m_SubEditors;

    private const string txt_Submitted = "Submitted";
    private const string txt_Deleted   = "Deleted";
    private const string txt_Saved     = "Saved";
    private const string txt_NotSaved  = "NotSaved";

    public DataObjectEditor (string name, IComponent parent, ISessionManager smgr,
                             Data.IObject dataObject):
      this (name, parent, smgr, dataObject, null) {}

    public DataObjectEditor (string name, IComponent parent, ISessionManager smgr,
                             Data.IObject dataObject, string callBack):
      this (name, parent, smgr, dataObject, callBack, false) {}

    public DataObjectEditor (string name, IComponent parent, ISessionManager smgr,
                             Data.IObject dataObject, string callBack, bool isSubEditor):
      this (name, parent, smgr, dataObject, callBack, isSubEditor, false, false) {}

    public DataObjectEditor (string name, IComponent parent, ISessionManager smgr,
                             Data.IObject dataObject, string callBack, bool isSubEditor,
                             bool savingDisabled, bool droppingDisabled):
      base (name, parent, smgr) { m_DataObject = dataObject; m_IsSubEditor = isSubEditor;
        m_ErrorFields = new Util.StringList (); m_IsSaved = true; m_DroppingDisabled = droppingDisabled;
        m_IsDeleted = false; m_CallBackPage = callBack; m_SavingDisabled = savingDisabled;
        m_SubEditors = new System.Collections.Generic.Dictionary <string, DataObjectEditor> (); }

    public Data.IObject getDataObject () { return m_DataObject; }

    protected void setDataObject (Data.IObject obj) { m_DataObject = obj; }

    public override bool isError () { return (m_ErrorFields.Count > 0); }

    public override bool performAction_Is2Generate () {

      m_ErrorFields.Clear ();
      Logging.LogProvider.write (Logging.LogLevel.DEBUG, "PERFORM ACTION");

      if ((getSessionManager ()).getParam (txt_Deleted) != null) {
        Logging.LogProvider.write (Logging.LogLevel.DEBUG, "DELETE");
        m_DataObject.drop ();
        changePage (m_CallBackPage);
      }

      if ((getSessionManager ()).getParam (txt_Submitted) != null ||
          (getSessionManager ()).getParam (txt_Deleted)   != null || m_IsSubEditor) {
        m_IsSaved = false;
        foreach (string fieldName in m_DataObject.getAllFieldNames ()) {
          Data.IObjectField field = null;
          DataObjectEditor subEditor = null;
          object val = null;
          try {
            field = m_DataObject.getField (fieldName);
            if (field.isSubObject () && !field.isSelectAble ()) {
              Logging.LogProvider.write (Logging.LogLevel.DEBUG,
                  "SubEditor (" + fieldName + ") detected...");
              try { subEditor = m_SubEditors [fieldName]; }
              catch {
                m_SubEditors [fieldName] = subEditor = (DataObjectEditor)
                  createComponent <DataObjectEditor> (fieldName,
                      field.getAsSubObject (), null, true);
              }
              subEditor.performAction_Is2Generate ();
              Data.IObject dataSubObj = subEditor.getDataObject ();
              val = dataSubObj.getRawObject ();
            }
            else { val = (getSessionManager ()).getParam (fieldName); }

            Logging.LogProvider.write (Logging.LogLevel.DEBUG,
                "SET FIELD: " + fieldName + " => " + System.Convert.ToString (val));

            if (val != null) {
              field.setValue (val);
              m_DataObject.setField (field);
            }
          }
          catch (System.Exception ex) { Logging.LogProvider.write (ex); }
          finally {
            if (field != null && field.isMandatory () &&
                (val == null ||
                 (val is string && (string) val == "") ||
                 (subEditor != null && subEditor.isError ()))) {
              Logging.LogProvider.write (Logging.LogLevel.DEBUG, "BAD INPUT");
              m_ErrorFields.Add (field.getName ());
            }
          }
        }

        if (!m_DroppingDisabled && (getSessionManager ()).getParam (txt_Deleted) != null) {
          Logging.LogProvider.write (Logging.LogLevel.DEBUG, "DELETE");
          m_DataObject.drop ();
          m_IsDeleted = true;
          changePage (m_CallBackPage);
        }
        else if (!m_SavingDisabled && !m_IsSubEditor && !isError ()) {
          m_DataObject.save ();
          m_IsSaved = true;
        }
      }

      return true;
    }

    public override void generate (ISessionManager smgr) {

      Logging.LogProvider.write (Logging.LogLevel.DEBUG,
          (getSessionManager ()).makeFullName ("GENERATE"));

      if (m_IsDeleted) {
        w (tag ("h1", getUserText ("Deleted")));
        w (makeReference (getUserText ("CallBack"), getUserText ("CallBack_Info"),
              Constants.txt_FormName, Constants.txt_CurrentPage,
              m_CallBackPage));
        return;
      }

      foreach (string badField in m_ErrorFields)
        { Logging.LogProvider.write (Logging.LogLevel.DEBUG, "badField => " + badField); }

      w (tableBegin (getName () + "_Table", tagArg ("class", "editor")));
      if (!m_IsSubEditor) {
        w (trBegin (tagArg ("class", "editor_title")));
          w (td (getUserText ("title"), tagArg ("colspan", "3")));
        w (trEnd ());
      }
      int i = 1;
      foreach (Data.IObjectField field in m_DataObject.getAllFields ()) {
        string rowClass = (i++ % 2 == 0)? "editor_roweven" : "editor_rowodd";
        string fieldName = field.getName ();
        bool isSubEditor = false;
        string mandatorySign = UI.Constants.txt_MandatorySign;
        IComponent subComponent = null;
        if (field.isSubObject ()) {
          if (field.isSelectAble ()) {
            subComponent = createComponent <DataObjectSelector> (fieldName,
                field.getPossibleValues (), field.getValue ());
          }
          else {
            try { subComponent = m_SubEditors [fieldName]; }
            catch {
              subComponent = createComponent <DataObjectEditor> (fieldName,
                  field.getAsSubObject (), null, true);
              m_SubEditors [fieldName] = (DataObjectEditor) subComponent;
            }
            mandatorySign = null;
            isSubEditor = true;
          }
        }
        rowClass = (m_ErrorFields.Contains (fieldName) &&
                    !isSubEditor)? "editor_rowerror" : rowClass;
        rowClass += (isSubEditor)? "_base" : null;
        w (trBegin (tagArg ("class", rowClass)));
          w (td (getUserText (field.getName ()) +
                (field.isMandatory ()? mandatorySign : null),
                tagArg ("class", "editor_header")));
          w (tdBegin (tagArg ("class", "editor_value") + tagArg ("colspan", "2")));
            Logging.LogProvider.write (Logging.LogLevel.DEBUG,
                "generate: fieldName = " + field.getName () + " (" +
                field.getValueType () + ")");

            if (subComponent != null) subComponent.generate (smgr);
            else w (textBox (fieldName, field.getValueString (),
                             field.isReadOnly ()));
          w (tdEnd ());
        w (trEnd ());
      }
      string status; string statusClass;
      if (m_IsSaved) { status = txt_Saved;    statusClass = "editor_saved"; }
      else           { status = txt_NotSaved; statusClass = "editor_notsaved"; }
      if (!m_IsSubEditor) {
        w (tr (td (getUserText (status), tagArg ("class", statusClass)) +
               td (button (txt_Submitted, getUserText (txt_Submitted)),
                   tagArg ("class", "editor_buttonbar") +
                   (m_DroppingDisabled? tagArg ("colspan", "2") : null)) +
               (m_DroppingDisabled? null :
               td (button (txt_Deleted, getUserText (txt_Deleted)),
                   tagArg ("class", "editor_buttonbar")))));
      }
      w (tableEnd ());
    }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
