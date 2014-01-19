/*
 * $Id: Org.FooBoo.Data.cs 60 2008-08-03 21:08:04Z tmr $
 *
 * Module:  Org.FooBoo.Data -- description
 * Created: 26-JAN-2008 12:48
 * Author:  tmr
 */

namespace Org.FooBoo.Data {

  public class Creator {

    public static IObjectStorage createStorage (string storeName)
      { return new Storage.Engine.DB4O.Instance (storeName); }
  }

  public interface IObjectStorage {

    System.Collections.Generic.IList <Type> find <Type> (System.Func <Type, System.Boolean> fun);
    System.Collections.IEnumerable find (object obj);
    void store (object obj);
    void drop (object obj);
    bool isOpen ();
    void close ();
  }

  public interface IObjectField {

//    Type   getValue <Type> ();
    object getValue ();
    void   setValue (object val);

    System.Type getValueType   ();
    string      getValueString ();

    IObject     getAsSubObject    ();
    IObjectSet  getPossibleValues ();

    void save ();

    bool isCompatWith (IObjectField field);
    bool isEnum ();

    bool isPrimaryKey ();
    bool isReadOnly   ();
    bool isMandatory  ();
    bool isSubObject  ();
    bool isSelectAble ();

    string getName ();
  }

  public interface IObject {

    string              getKeyFieldName ();
    IObjectField        getKeyFieldVal  ();

    string []           getAllFieldNames ();
    IObjectField []     getAllFields ();

    IObjectField        getField (string name);
    void                setField (IObjectField field);

    void                save ();
    void                drop ();

    object              getRawObject ();
    System.Type         getType ();

    bool checkData (); /* Check if all mandatory fields are filled */
    bool isEnum ();
  }

  public interface IObjectSetFilter {
  }

  public interface IObjectSetOrder {
  }

  public interface IObjectSet {

    void setFilter (IObjectSetFilter filter);
    void setOrder  (IObjectSetOrder  order);

    IObject getObject ();

    void reset ();
    bool next ();
    bool isFirst ();
  }

  public class AbstractizableObjectAttrib: System.Attribute {}

  public class AbstractizableFieldAttrib: System.Attribute {

    private bool m_PrimaryKey;
    private bool m_ReadOnly;
    private bool m_Mandatory;
    private bool m_SelectAble;

    public  bool PrimaryKey { get { return m_PrimaryKey; } set { m_PrimaryKey = value; } }
    public  bool ReadOnly   { get { return m_ReadOnly;   } set { m_ReadOnly   = value; } }
    public  bool Mandatory  { get { return m_Mandatory;  } set { m_Mandatory  = value; } }
    public  bool SelectAble { get { return m_SelectAble; } set { m_SelectAble = value; } }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
