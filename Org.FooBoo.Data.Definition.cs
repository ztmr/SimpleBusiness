/*
 * $Id: Org.FooBoo.Data.Definition.cs 61 2008-08-07 10:13:21Z tmr $
 *
 * Module:  Org.FooBoo.Data.Definition -- description
 * Created: 26-JAN-2008 12:38
 * Author:  tmr
 */

namespace Org.FooBoo.Data.Definition {

  /* Access credentials */
  [AbstractizableObjectAttrib]
  public class Credentials {

    private string m_Login;
    private string m_PassWord;

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=false)]
    public  string Login      { get { return m_Login;       }
                                set { m_Login = value;      } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string PassWord   { get { return m_PassWord;    }
                                set { m_PassWord = value;   } }

    public Credentials (string login): this (login, null) {}

    public Credentials (string login, string password)
      { m_Login = login; m_PassWord = password; }
  }

  /* Address of Subject */
  [AbstractizableObjectAttrib]
  public class Address {

    private string m_Street;
    private string m_Town;
    private string m_Country;
    private string m_PostalCode;

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string Street     { get { return m_Street;      }
                                set { m_Street = value;     } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string Town       { get { return m_Town;        }
                                set { m_Town = value;       } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string Country    { get { return m_Country;     }
                                set { m_Country = value;    } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string PostalCode { get { return m_PostalCode;  }
                                set { m_PostalCode = value; } }

    public Address (): this (null, null, null, null) {}

    public Address (string street, string town,
                    string country, string postalCode) {

      m_Street = street; m_Town = town;
      m_Country = country; m_PostalCode = postalCode;
    }

    public override string ToString ()
      { return Street + ", " + Town + ", " + Country; }
  }

  /* To be inherited */
  public abstract class ASubject {

    private string  m_Name;
    private string  m_SurName;
    private Address m_Address;
    private string  m_SubjIdCode;
    private string  m_TaxIdCode;
    private string  m_BankName;
    private string  m_BankAccountCode;

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=false)]
    public  string Name                { get { return m_Name; }
                                         set { m_Name = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string SurName             { get { return m_SurName; }
                                         set { m_SurName = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  Address Address            { get { return m_Address; }
                                         set { m_Address = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=true, ReadOnly=false, Mandatory=true)]
    public  string SubjIdCode          { get { return m_SubjIdCode; }
                                         set { m_SubjIdCode = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=false)]
    public  string TaxIdCode           { get { return m_TaxIdCode; }
                                         set { m_TaxIdCode = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=false)]
    public  string BankName            { get { return m_BankName; }
                                         set { m_BankName = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=false)]
    public  string BankAccountCode     { get { return m_BankAccountCode; }
                                         set { m_BankAccountCode = value; } }

    public ASubject (): this (null) {}

    public ASubject (string subjIdCode):
      this (null, null, null, subjIdCode, null, null, null) {}

    public ASubject (string name, string surName, Address address):
      this (name, surName, address, null, null, null, null) {}

    public ASubject (string name, string surName, Address address,
                     string subjIdCode, string taxIdCode,
                     string bankName, string bankAccountCode) {

      m_Name = name; m_SurName = surName; m_Address = address;
      m_SubjIdCode = subjIdCode; m_TaxIdCode = taxIdCode;
      m_BankName = bankName; m_BankAccountCode = bankAccountCode;
    }

    public override string ToString ()
      { return (m_Name + " " + m_SurName + ", " +
          ((m_Address == null)? "UnKnownAddress" :
                (m_Address.Street + ", " + m_Address.Town))); }
  }

  /* Stored only once -- it's businessman's profile */
  [AbstractizableObjectAttrib]
  public class Supplier: ASubject {

    private string m_BusinessRegCode;
    private string m_RegistrationAutority;
    private bool   m_IsVatPayer;

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string BusinessRegCode      { get { return m_BusinessRegCode; }
                                          set { m_BusinessRegCode = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=false)]
    public  string RegistrationAutority { get { return m_RegistrationAutority; }
                                          set { m_RegistrationAutority = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  bool IsVatPayer             { get { return m_IsVatPayer; }
                                          set { m_IsVatPayer = value; } }

    public Supplier (): this (null, null, null) {}

    public Supplier (string id): base (id) {}

    public Supplier (string name, string surName, Address address):
      this (name, surName, address, null, null, null, null, null, null, true) {}

    public Supplier (string name, string surName, Address address,
                     string subjIdCode, string taxIdCode,
                     string bankName, string bankAccountCode,
                     string businessRegCode, string registrationAutority,
                     bool isVatPayer): base (name, surName, address, subjIdCode,
                                             taxIdCode, bankName, bankAccountCode) {

      m_BusinessRegCode = businessRegCode;
      m_RegistrationAutority = registrationAutority;
      m_IsVatPayer = isVatPayer;
    }
  }

  /* Represents each subject we make business with */
  [AbstractizableObjectAttrib]
  public class Customer: ASubject {

    public Customer (): base () {}

    public Customer (string id): base (id) {}

    public Customer (string name, string surName,
                     Address address): base (name, surName, address) {}
  }

  [AbstractizableObjectAttrib]
  public enum EPaymentMethod { BankTransfer, Cash };

  /* An Invoice -- just one per one businessman's trade */
  [AbstractizableObjectAttrib]
  public class Invoice {

    private Identification        m_Code;
    private string                m_ServiceDescription;
    private string                m_Price;
    private string                m_Currency;
    private EPaymentMethod        m_PaymentMethod;
    private System.DateTime       m_DateOfIssuance;
    private System.DateTime       m_DateOfTaxablePayment;
    private System.DateTime       m_PurgeDate;
    private Customer              m_Customer;
    private Supplier              m_Supplier;

    [AbstractizableFieldAttrib(PrimaryKey=true, ReadOnly=true, Mandatory=true)]
    public  string                Code
                      { get { return m_Code.ToString (); }
                        set { m_Code = new Identification (value); } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string ServiceDescription   { get { return m_ServiceDescription; }
                                          set { m_ServiceDescription = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  string Price                { get { return m_Price; }
                                          set { m_Price = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=false)]
    public  string Currency             { get { return m_Currency; }
                                          set { m_Currency = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true, SelectAble=true)]
    public  EPaymentMethod PaymentMethod  { get { return m_PaymentMethod; }
                                            set { m_PaymentMethod = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  System.DateTime DateOfIssuance { get { return m_DateOfIssuance; }
                                             set { m_DateOfIssuance = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  System.DateTime DateOfTaxablePayment { get { return m_DateOfTaxablePayment; }
                                                   set { m_DateOfTaxablePayment = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true)]
    public  System.DateTime PurgeDate     { get { return m_PurgeDate; }
                                            set { m_PurgeDate = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true, SelectAble=true)]
    public  Customer Customer             { get { return m_Customer; }
                                            set { m_Customer = value; } }

    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true, SelectAble=true)]
//    [AbstractizableFieldAttrib(PrimaryKey=false, ReadOnly=false, Mandatory=true, SelectAble=false)]
    public  Supplier Supplier             { get { return m_Supplier; }
                                            set { m_Supplier = value; } }

    public Invoice (): this ((new Identification (0)).ToString ()) {}

    public Invoice (string id):
      this (id, null) {}

    public Invoice (string id, Supplier supplier):
      this (new Identification (id), supplier, null, null, null, null) {}

    public Invoice (Identification idCode, Supplier supplier,
                    Customer customer, string serviceDescription,
                    string price, string currency) {

      m_Code = idCode;
      m_Supplier = supplier; m_Customer = customer;
      m_ServiceDescription = serviceDescription;
      m_Price = price; m_Currency = currency;
      m_PaymentMethod      = EPaymentMethod.BankTransfer;
      long nowTicks        = System.DateTime.Now.Ticks;
      long deltaTicks      = (new System.TimeSpan (14, 0, 0, 0)).Ticks;
      DateOfIssuance       = new System.DateTime (nowTicks);
      DateOfTaxablePayment = new System.DateTime (nowTicks + deltaTicks);
      PurgeDate            = new System.DateTime (nowTicks + deltaTicks);
    }

    public class Identification: System.IComparable {

      private const string cPrefix = "FP-";

      private int m_Year;
      private int m_No;

      public  int Year    { get { return m_Year;  }
                            set { m_Year = value; } }

      public  int No      { get { return m_No;  }
                            set { m_No = value; } }

      private void reportInvalidId (string id)
        { throw new Exceptions.InternalError
            ("invalid invoice id: " + id); }

      public Identification (string fullId) {

        if (fullId == null || fullId == "")
          { m_No = 0; m_Year = System.DateTime.Now.Year -2000; return; }

        if (!fullId.StartsWith (cPrefix)) reportInvalidId (fullId);

        string [] parts = (fullId.Replace (cPrefix, "")).Split ('-');

        if (parts.Length != 2) reportInvalidId (fullId);

        m_Year = System.Convert.ToInt32 (parts [0]);
        m_No   = System.Convert.ToInt32 (parts [1]);
      }

      public Identification (int no, int year)
        { m_No = no; m_Year = year; }

      public Identification (int no):
        this (no, System.DateTime.Now.Year -2000) {}

      public int CompareTo (object obj) {

        if (!(obj is Identification))
          throw new Exceptions.InternalError ("type error!");

        Identification cmpTo = (Identification) obj;

        if      (cmpTo.Year > this.Year) return -1;
        else if (cmpTo.Year < this.Year) return +1;
        else if (cmpTo.No   > this.No)   return -1;
        else if (cmpTo.No   < this.No)   return +1;
        else                             return  0;
      }

      public override string ToString ()
        { return System.String.Format ("{0}{1:00}-{2:000}", cPrefix, m_Year, m_No); }
    }
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
