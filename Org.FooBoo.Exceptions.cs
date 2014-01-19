/*
 * $Id: Org.FooBoo.Exceptions.cs 42 2008-01-27 05:34:03Z tmr $
 *
 * Module:  Org.FooBoo.Exceptions -- description
 * Created: 26-JAN-2008 12:00
 * Author:  tmr
 */

namespace Org.FooBoo.Exceptions {

  public class InternalError: System.ApplicationException {

    public InternalError (string message, params object [] args):
      base (System.String.Format ("ImplementationError: " + message, args)) {}
  }
}

// vim: fdm=syntax:fdn=3:tw=74:ts=2:syn=cs
