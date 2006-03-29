using System;

namespace Novell.iFolder.Enterprise
{
	/// <summary>
	/// Report Column
	/// </summary>
	public class ReportColumn
	{
		string header;
		string format;

		public ReportColumn(string header) : this(header, "{0}")
		{
		}

		public ReportColumn(string header, string format)
		{
			this.header = header;
			this.format = format;
		}

		public string Header
		{
			get { return header; }
		}

		public string Format
		{
			get { return format; }
		}
	}
}
