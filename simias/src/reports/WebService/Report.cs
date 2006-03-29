using System;
using System.IO;

namespace Novell.iFolder.Enterprise
{
	/// <summary>
	/// Report
	/// </summary>
	public class Report
	{
		private Report()
		{
		}

		public static void WriteHeaderRow(StreamWriter writer, ReportColumn[] columns)
		{
			for(int i=0; i < columns.Length; i++)
			{
				writer.Write("\"{0}\"{1}", columns[i].Header,
					(i < (columns.Length - 1)) ? "," : "");
			}

			writer.WriteLine();
		}

		public static void WriteRow(StreamWriter writer, ReportColumn[] columns, object[] cells)
		{
			for(int i=0; i < columns.Length; i++)
			{
				writer.Write("\"" + columns[i].Format + "\"{1}", cells[i],
					(i < (columns.Length - 1)) ? "," : "");
			}

			writer.WriteLine();
		}
	}
}
