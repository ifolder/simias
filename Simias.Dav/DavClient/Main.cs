// project created on 12/11/2005 at 9:41 AM
using System;
using System.Net;
using System.Web;

using Novell.DavClient;

class MainClass
{
	public static void Main(string[] args)
	{
		string service = "http://192.168.1.99:8086/simias10/sdav.ashx";
		string username = "banderso";
		string password = "novell";
		
		if ( args.Length > 0 && args[0] != null )
		{
			service = args[0];
		}
		
		if ( args.Length > 1 && args[1] != null )
		{
			username = args[1];
		}
		
		if ( args.Length > 2 && args[2] != null )
		{
			password = args[2];
		}
		
		Novell.DavClient.WebState state = new WebState( service, username, password );
		
		Console.WriteLine( "Sending OPTIONS" );
		Novell.DavClient.Options options = new Novell.DavClient.Options( state );
		Console.WriteLine( " sending request" );
		options.Send();
		Console.WriteLine( " send complete - status: " + options.ResponseStatus.ToString() );
		
		if ( options.ResponseStatus == HttpStatusCode.OK )
		{
			Console.WriteLine( " DAV Version: " + options.GetResponseHeader( "DAV" ) );
			Console.WriteLine( " Allow: " + options.GetResponseHeader( "Allow" ) );
			
			// Send an "allprop" request
			Console.WriteLine( "Sending PROPFIND - allprop" );
			PropertyFind pf = new PropertyFind( state, "/", true );
			Console.WriteLine( " sending request" );
			pf.Send();
			Console.WriteLine( " send complete - status: " + options.ResponseStatus.ToString() );
		}
	}
}