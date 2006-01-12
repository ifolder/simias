using System;
using System.Web;

namespace Simias.Rss
{
	/// <summary>
	/// Summary description for RssHeaders.
	/// </summary>
	public class Headers
	{
		private HttpContext ctx;
		public Headers( HttpContext Context )
		{
			ctx = Context;
		}

		public void SendStartTag()
		{
			ctx.Response.ContentType = "text/xml";
			ctx.Response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
			ctx.Response.Write( "<rss version=\"2.0\">" );
		}

		public void SendEndTag()
		{
			ctx.Response.Write( "</rss>" );
		}
	}
}
