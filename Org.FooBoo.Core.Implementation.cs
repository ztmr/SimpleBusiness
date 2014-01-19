/*
 * $Id: Org.FooBoo.Core.Implementation.cs 61 2008-08-07 10:13:21Z tmr $
 *
 * Module:  Org.FooBoo.Core.Implementation -- description
 * Created: 26-JAN-2008 12:31
 * Author:  tmr
 */

namespace Org.FooBoo.Core.Implementation {

  public class Instance: ICore {

    private Data.IObjectStorage m_DataStore;

    public Instance (Data.IObjectStorage storage)
      { m_DataStore = storage; }

    public void startUp ()
      { Logging.LogProvider.write (Logging.LogLevel.INFO, "Starting up core..."); }

    public void shutDown ()
      { Logging.LogProvider.write (Logging.LogLevel.INFO, "Shutting down core..."); }

    public Data.IObjectStorage getDataStore () { return m_DataStore; }

    public Data.IObject createEmptyCredentials ()
      { return (new Data.Abstraction.Object (m_DataStore, new Data.Definition.Credentials (null))); }

    public bool validateCredentials (string login, string password) {

      if (login == "admin" && password == "admin") return true;

      System.Collections.Generic.IList <Data.Definition.Credentials> res =
        m_DataStore.find <Data.Definition.Credentials>
          (delegate (Data.Definition.Credentials c)
           { return (c.Login == login && c.PassWord == password); });

      return ((res.Count <= 0)? false : true);
    }

    private Data.Definition.Invoice createInvoice ()
      { return new Data.Definition.Invoice (getNextInvoiceId (),
          ((Data.Definition.Supplier) (getCreateSupplier ()).getRawObject ())); }

    private Data.Definition.Customer createCustomer ()
      { return new Data.Definition.Customer (null, null,
          new Data.Definition.Address (null, null, null, null)); }

    public Data.IObject createEmptyCustomer ()
      { return (new Data.Abstraction.Object (m_DataStore, createCustomer ())); }

    public Data.IObject createEmptyInvoice  ()
      { return (new Data.Abstraction.Object (m_DataStore, createInvoice ())); }

    private string getNextInvoiceId () {

      Data.IObjectSet ds = getAllInvoices ();
      System.Collections.SortedList list =
        new System.Collections.SortedList ();

      /* XXX: should be replaced by comparing directly in database */
      while (ds.next ()) {
        Data.Definition.Invoice invoice = (Data.Definition.Invoice)
          (ds.getObject ()).getRawObject ();
          list.Add (invoice.Code, invoice.Code);
      }

      Data.Definition.Invoice.Identification id;

      if (list.Count > 0) {
        id = new Data.Definition.Invoice.Identification ((string) list.GetByIndex (list.Count -1));
        if (id.Year == System.DateTime.Now.Year -2000) id.No++;

        return id.ToString ();
      }

      return (new Data.Definition.Invoice.Identification (1)).ToString ();
    }

    public Data.IObjectSet getAllInvoices () {

      System.Collections.Generic.IList <Data.Definition.Invoice> res =
        m_DataStore.find <Data.Definition.Invoice>
          (delegate (Data.Definition.Invoice inv) { return true; });

      return (new Data.Abstraction.ObjectSet (m_DataStore, res.GetEnumerator ()));
    }

    public Data.IObjectSet getAllCustomers () {

      System.Collections.Generic.IList <Data.Definition.Customer> res =
        m_DataStore.find <Data.Definition.Customer>
          (delegate (Data.Definition.Customer cust) { return true; });

      return (new Data.Abstraction.ObjectSet (m_DataStore, res.GetEnumerator ()));
    }

    public Data.IObjectSet getAllSuppliers () {

      System.Collections.Generic.IList <Data.Definition.Supplier> res =
        m_DataStore.find <Data.Definition.Supplier>
          (delegate (Data.Definition.Supplier cust) { return true; });

      return (new Data.Abstraction.ObjectSet (m_DataStore, res.GetEnumerator ()));
    }

    public Data.IObject getInvoiceById (string id) {

      System.Collections.Generic.IList <Data.Definition.Invoice> res =
        m_DataStore.find <Data.Definition.Invoice>
          (delegate (Data.Definition.Invoice inv) { return (inv.Code == id); });

      if (res.Count <= 0) return null;

      return new Data.Abstraction.Object (m_DataStore, res [0]);
    }

    public Data.IObject getCustomerById (string id) {

      System.Collections.Generic.IList <Data.Definition.Customer> res =
        m_DataStore.find <Data.Definition.Customer>
          (delegate (Data.Definition.Customer cust) { return (cust.SubjIdCode == id); });

      if (res.Count <= 0) return null;

      return new Data.Abstraction.Object (m_DataStore, res [0]);
    }

    public Data.IObject getCreateSupplier () {

      System.Collections.Generic.IList <Data.Definition.Supplier> res =
        m_DataStore.find <Data.Definition.Supplier>
          (delegate (Data.Definition.Supplier supplier) { return true; });

      return (new Data.Abstraction.Object (m_DataStore, (res.Count <= 0)?
        new Data.Definition.Supplier () : res [0]));
    }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
