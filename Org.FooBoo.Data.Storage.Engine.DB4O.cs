/*
 * $Id: Org.FooBoo.Data.Storage.Engine.DB4O.cs 61 2008-08-07 10:13:21Z tmr $
 */

namespace Org.FooBoo.Data.Storage.Engine.DB4O {

  public class Instance: IObjectStorage {

    private string                           m_StoreName;
    private Db4objects.Db4o.IObjectContainer m_Database;
    private bool                             m_Open;

    public Instance (string storeName)
      { m_StoreName = storeName; init (); }

    private void init () {

      try {
        m_Database = Db4objects.Db4o.Db4oFactory.OpenFile (m_StoreName);
//        Db4objects.Db4o.Config.IConfiguration conf =
//          Db4objects.Db4o.Db4oFactory.Configure ();
        m_Open = true;
      }
      catch (System.Exception ex) {
        reportProblem (ex);
        m_Open = false;
      }
      finally { checkIsOpen (); }
    }

    private void reportProblem (System.Exception ex) {

      Logging.LogProvider.write (Logging.LogLevel.WARN,
          "Caught: " + ex.Message + "\n\n" + ex.StackTrace);
    }

    public void store (object obj) {

      Logging.LogProvider.write (Logging.LogLevel.DEBUG,
          "Storing object: " + System.Convert.ToString (obj));
      lock (m_Database) {
        checkIsOpen ();
        try { m_Database.Set (obj); }
        catch (System.Exception ex) { reportProblem (ex); }
      }
    }

    public void drop (object obj) {

      Logging.LogProvider.write (Logging.LogLevel.DEBUG,
          "Dropping object: " + System.Convert.ToString (obj));
      lock (m_Database) {
        checkIsOpen ();
        try { m_Database.Delete (obj); }
        catch (System.Exception ex) { reportProblem (ex); }
      }
    }

    public System.Collections.Generic.IList <Type> find <Type> (System.Func <Type, System.Boolean> fun) {

      System.Collections.Generic.IList <Type> res = null;

      Logging.LogProvider.write (Logging.LogLevel.DEBUG,
          "Searching object: " + System.Convert.ToString (typeof (Type)));
      lock (m_Database) {
        checkIsOpen ();
        try { res = m_Database.Query <Type> (delegate (Type obj) { return fun (obj); }); }
        catch (System.Exception ex) { reportProblem (ex); }
      }

      return res;
    }

    public System.Collections.IEnumerable find (object obj) {

      Logging.LogProvider.write (Logging.LogLevel.DEBUG,
          "Searching object: " + ((obj != null) ? System.Convert.ToString (obj.GetType ()) : null));
      lock (m_Database) {
        checkIsOpen ();
        try { return m_Database.Get (obj); }
        catch (System.Exception ex) { reportProblem (ex); }
      }

      return null;
    }

    private void checkIsOpen ()
      { if (!m_Open) throw new Exceptions.InternalError ("Database is closed!"); }

    public bool isOpen () { return m_Open; }

    public void close ()
      { lock (m_Database) { checkIsOpen (); m_Database.Close (); m_Open = false; } }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
