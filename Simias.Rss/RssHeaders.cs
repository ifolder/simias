using System;
using System.Web;

namespace Simias.RssFeed
{
	/// <summary>
	/// Summary description for RssHeaders.
	/// </summary>
	public class Headers
	{
		static public void SendStartTag( HttpContext Ctx )
		{
			Ctx.Response.ContentType = "text/xml";
			Ctx.Response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
			Ctx.Response.Write( "<rss version=\"2.0\">" );
		}

		static public void SendEndTag( HttpContext Ctx )
		{
			Ctx.Response.Write( "</rss>" );
		}
	}
}
