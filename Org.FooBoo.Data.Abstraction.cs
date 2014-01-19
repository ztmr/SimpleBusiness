/*
 * $Id: Org.FooBoo.Data.Abstraction.cs 60 2008-08-03 21:08:04Z tmr $
 *
 * Module:  Org.FooBoo.Data.Abstraction -- description
 * Created: 26-JAN-2008 12:37
 * Author:  tmr
 */

namespace Org.FooBoo.Data.Abstraction {

  public class ObjectField: IObjectField {

    private string         m_Name;
    private System.Type    m_Type;
    private object         m_Value;
    private bool           m_IsPrimaryKey;
    private bool           m_IsReadOnly;
    private bool           m_IsMandatory;
    private bool           m_IsSelectAble;
    private IObject        m_ParentObject;
    private IObjectStorage m_ObjectStore;

    public ObjectField (IObject parent, IObjectStorage storage,
                        string name, System.Type fieldType, object val,
                        bool isPrimaryKey, bool isReadOnly, bool isMandatory,
                        bool isSelectAble)
      { m_ParentObject = parent; m_ObjectStore = storage;
        m_Name = name; m_Type = fieldType; m_Value = val;
        m_IsPrimaryKey = isPrimaryKey; m_IsReadOnly = isReadOnly;
        m_IsMandatory = isMandatory; m_IsSelectAble = isSelectAble; }

    private void checkTypeCompat (System.Type type) {

      /*
      if (type != m_Value.GetType ())
        throw new Exceptions.InternalError ("Incompatible ({0}, {1}) values!",
            type, m_Value.GetType ());
            */
    }

    public object getValue () {

      object res = m_Value;

      if (res == null) {
        try { res = System.Activator.CreateInstance (getValueType ()); }
        catch (System.Exception ex) { Logging.LogProvider.write (ex); }
      }

      return res;
    }

    public IObject getAsSubObject () { return new Object (m_ObjectStore, getValue ()); }

    public void setValue (object val)
      { checkTypeCompat (val.GetType ()); m_Value = val; }

    public string getValueString ()
      { return System.Convert.ToString (m_Value); }

    public System.Type getValueType () { return m_Type; }

    public bool isCompatWith (IObjectField field)
      { return (field.getValueType () == getValueType ()); }

    public void save () { m_ParentObject.setField (this); m_ParentObject.save (); }

    public bool isPrimaryKey () { return m_IsPrimaryKey; }

    public bool isReadOnly () { return m_IsReadOnly; }

    public bool isMandatory () { return m_IsMandatory; }

    public bool isSelectAble () { return m_IsSelectAble; }

    public bool isSubObject () {

      if (!(System.Convert.ToString (m_Type)).Contains ("Data.Definition")) return false;

      try   { IObject subObj = new Object (m_ObjectStore, getValue ()); }
      catch { return false; }

      return true;
    }

    public bool isEnum () { return (getValueType ().IsEnum); }

    public IObjectSet getPossibleValues () {

      System.Collections.IEnumerator enumerator;

      if (isEnum ()) {
        System.Array vals = System.Enum.GetValues (m_Type);
        enumerator = vals.GetEnumerator ();
      }
      else {
        enumerator = (m_ObjectStore.find (System.Activator.CreateInstance (m_Type))).GetEnumerator ();
//        System.Collections.Generic.IList <type> res =
//          m_ObjectStore.find <type> (delegate (type obj) { return true; });
//        enumerator = res.GetEnumerator ();
      }

      return new ObjectSet (m_ObjectStore, enumerator);
    }

    public string getName () { return m_Name; }
  }

  public class Object: IObject {

    private object         m_Object;
    private IObjectStorage m_ObjectStore;

    public Object (IObjectStorage storage, object dataObject)
      { checkTypeCompat (dataObject.GetType ());
        m_ObjectStore = storage; m_Object = dataObject; }

    private void checkCompatField (IObjectField field) {

      if (!(getField (field.getName ())).isCompatWith (field))
        throw new Exceptions.InternalError
          ("Incompatible ({0}, {1}) fields!",
           field.getValueType (),
           (getField (field.getName ())).getValueType ());
    }

    private void checkTypeCompat (System.Type type) {

      if ((type.GetCustomAttributes (
              typeof (AbstractizableObjectAttrib), false)).Length < 1)
        throw new Exceptions.InternalError
          ("Type {0} has no AbstractizableObject attribute!", type);
    }

    private System.Reflection.PropertyInfo [] getProperties () {

      return (m_Object.GetType ()).GetProperties
        (System.Reflection.BindingFlags.Public |
         System.Reflection.BindingFlags.Instance);
    }

    public System.Type getType () { return (m_Object == null)? null : m_Object.GetType (); }

    public bool isEnum () { return (getType ()).IsEnum; }

    public void save () { drop (); m_ObjectStore.store (getRawObject ()); }

    public void drop () {

      foreach (IObjectField fld in getAllFields ())
        Logging.LogProvider.write (Logging.LogLevel.DEBUG,
            "DROP => " + fld.getName () + " = " + fld.getValueString ());

      System.Collections.IEnumerator enu =
        (m_ObjectStore.find (System.Activator.CreateInstance (getType (),
               (getKeyFieldVal ()).getValueString ()))).GetEnumerator ();

      while (enu.MoveNext ()) { m_ObjectStore.drop (enu.Current); }
    }

    public string getKeyFieldName () {

      foreach (IObjectField field in getAllFields ())
        if (field.isPrimaryKey ()) return field.getName ();

      return null;
    }

    public IObjectField getKeyFieldVal () {

      string fieldName = getKeyFieldName ();
      return ((fieldName == null)? null : getField (fieldName));
    }

    public string [] getAllFieldNames () {

      Util.StringList list = new Util.StringList ();

      foreach (System.Reflection.PropertyInfo pi in getProperties ())
        list.Add (pi.Name);

      return list.toArray ();
    }

    public IObjectField [] getAllFields () {

      System.Collections.ArrayList list =
        new System.Collections.ArrayList ();

      foreach (System.Reflection.PropertyInfo pi in getProperties ()) {
        object [] attribs = pi.GetCustomAttributes (typeof (AbstractizableFieldAttrib), false);
        if (attribs.Length > 0) list.Add (getField (pi.Name));
      }

      return (IObjectField []) list.ToArray (typeof (IObjectField));
    }

    private System.Reflection.PropertyInfo getProperty (string name)
      { return (m_Object.GetType ()).GetProperty (name); }

    public IObjectField getField (string name) {

      if (name == null) return null;
      bool isFieldPrimaryKey = false;
      bool isFieldReadOnly   = true;
      bool isFieldMandatory  = false;
      bool isFieldSelectAble = false;
      System.Reflection.PropertyInfo pi = getProperty (name);

      object [] attribs =
        pi.GetCustomAttributes (typeof (AbstractizableFieldAttrib), false);

      if (attribs.Length > 0) {
        AbstractizableFieldAttrib attrib =
          (AbstractizableFieldAttrib) attribs [0];
        isFieldPrimaryKey = attrib.PrimaryKey;
        isFieldReadOnly   = attrib.ReadOnly;
        isFieldMandatory  = attrib.Mandatory;
        isFieldSelectAble = attrib.SelectAble;
      }

      return new ObjectField (this, m_ObjectStore, name, pi.PropertyType,
          pi.GetValue (m_Object, null), isFieldPrimaryKey,
          isFieldReadOnly, isFieldMandatory, isFieldSelectAble);
    }

    public void setField (IObjectField field) {

//      checkCompatField (field);

      System.Reflection.PropertyInfo property =
        getProperty (field.getName ());

      object obj = field.getValue ();
      object [] attribs =
        (obj.GetType ()).GetCustomAttributes (
            typeof (AbstractizableObjectAttrib), false);

      Logging.LogProvider.write (Logging.LogLevel.DEBUG,
          "field={0}; type={1}; propType={2};",
          field.getName (), obj.GetType (), property.PropertyType);
//      if (attribs.Length > 0) {
        if (property.PropertyType == typeof (System.String))
          obj = System.Convert.ToString (obj);
        else if (property.PropertyType == typeof (System.DateTime))
          obj = System.Convert.ToDateTime (obj);
        else if (property.PropertyType.IsEnum)
          obj = System.Enum.Parse (property.PropertyType, System.Convert.ToString (obj));
        else {
          try {
            /* Try to find it in a data-store */
            if (field.isSelectAble ()) {
              System.Collections.IEnumerator enumerator =
                (m_ObjectStore.find (System.Activator.CreateInstance (field.getValueType (), obj))).GetEnumerator ();
              enumerator.MoveNext ();
              obj = enumerator.Current;
            }

            /* Create a new one */
            else {
              obj = System.Activator.CreateInstance (property.PropertyType, field.getValue ());
            }
          }
          catch (System.Exception ex) {
            Logging.LogProvider.write (Logging.LogLevel.DEBUG,
                "Setting a raw value ({0}) without any conversion...", obj);
//            Logging.LogProvider.write (Logging.LogLevel.WARN,
//                "Value '{0}' couldn't be set, because we don't know how to deal with this type...", obj);
//            obj = null;
          }
        }
//      }

      property.SetValue (m_Object, obj, null);
    }

    public object getRawObject ()
      { return m_Object; }

    public bool checkData () { return true; }
  }

  public class ObjectSet: IObjectSet {

    private IObjectStorage                 m_DataStore;
    private int                            m_Cnt;
    private System.Collections.IEnumerator m_Enum;

    public ObjectSet (IObjectStorage storage,
        System.Collections.IEnumerator en)
      { m_DataStore = storage; m_Cnt = 0; m_Enum = en; }

    public void setFilter (IObjectSetFilter filter) {}

    public void setOrder (IObjectSetOrder order) {}

    public IObject getObject ()
      { return new Object (m_DataStore, m_Enum.Current); }

    public void reset () { m_Enum.Reset (); }

    public bool next () { m_Cnt++; return m_Enum.MoveNext (); }

    public bool isFirst () { return (m_Cnt < 2); }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
