/*
 * $Id: Org.FooBoo.Core.cs 61 2008-08-07 10:13:21Z tmr $
 *
 * Module:  Org.FooBoo.Core -- description
 * Created: 24-JAN-2008 00:13
 * Author:  tmr
 */

namespace Org.FooBoo.Core {

  public interface ICore {

    void startUp ();
    void shutDown ();

    Data.IObjectStorage getDataStore ();

    Data.IObject    createEmptyCredentials ();
    bool            validateCredentials (string login, string password);

    Data.IObject    createEmptyCustomer ();
    Data.IObject    createEmptyInvoice  ();

    Data.IObjectSet getAllCustomers ();
    Data.IObjectSet getAllSuppliers ();
    Data.IObjectSet getAllInvoices  ();

    Data.IObject    getCustomerById (string id);
    Data.IObject    getInvoiceById  (string id);

    Data.IObject    getCreateSupplier ();
  }

  public class Creator {

    public static ICore create (Data.IObjectStorage storage)
      { return new Implementation.Instance (storage); }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
