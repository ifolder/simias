// project created on 12/11/2005 at 9:41 AM
using System;
using System.Net;
using System.Web;


class MainClass
{
	public static void Main(string[] args)
	{
		Console.WriteLine( "Sending OPTIONS" );
		
		Novell.DavClient.Options options = new Novell.DavClient.Options( "http://192.168.1.99:8086/simias10/sdav.ashx", "banderso", "baci2002" );
		Console.WriteLine( " sending request" );
		options.Send();
		Console.WriteLine( " send complete - status: " + options.ResponseStatus.ToString() );
		
		if ( options.ResponseStatus == HttpStatusCode.OK )
		{
			Console.WriteLine( " DAV Version: " + options.GetResponseHeader( "DAV" ) );
			Console.WriteLine( " Allow: " + options.GetResponseHeader( "Allow" ) );
		}
	}
}