using System;
using System.IO;
using System.Collections;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Policy;

namespace Novell.iFolder.Enterprise
{
	/// <summary>
	/// iFolder Report
	/// </summary>
	public class iFolderReport
	{
		enum ColumnID
		{
			ReportTime,
			iFolderSystem,
			iFolderServer,
			iFolderID,
			iFolderName,
			iFolderSize,
			iFolderPath,
			iFolderQuota,
			MemberCount,
			FileCount,
			DirectoryCount,
			OwnerID,
			OwnerName,
			OwnerCN,
			OwnerDN,
			OwnerQuota,
			OwnerLastLogin,
			OwnerDisabled,
			PreviousOwner,
			OrphanedOwner
		};

		static readonly int count = Enum.GetNames(typeof(ColumnID)).Length;

		static readonly ReportColumn[] columns = new ReportColumn[count];
		
		static iFolderReport()
		{
			// columns
			columns[(int)ColumnID.ReportTime] = new ReportColumn("Report Time", "{0:G}");
			columns[(int)ColumnID.iFolderSystem] = new ReportColumn("iFolder System");
			columns[(int)ColumnID.iFolderServer] = new ReportColumn("iFolder Server");
			columns[(int)ColumnID.iFolderID] = new ReportColumn("iFolder ID");
			columns[(int)ColumnID.iFolderName] = new ReportColumn("iFolder Name");
			columns[(int)ColumnID.iFolderSize] = new ReportColumn("iFolder Size (MB)", "{0:N02}");
			columns[(int)ColumnID.iFolderPath] = new ReportColumn("iFolder Path");
			columns[(int)ColumnID.iFolderQuota] = new ReportColumn("iFolder Quota (MB)", "{0:N02}");
			columns[(int)ColumnID.MemberCount] = new ReportColumn("Member Count");
			columns[(int)ColumnID.FileCount] = new ReportColumn("File Count");
			columns[(int)ColumnID.DirectoryCount] = new ReportColumn("Directory Count");
			columns[(int)ColumnID.OwnerID] = new ReportColumn("Owner ID");
			columns[(int)ColumnID.OwnerName] = new ReportColumn("Owner Name");
			columns[(int)ColumnID.OwnerCN] = new ReportColumn("Owner CN");
			columns[(int)ColumnID.OwnerDN] = new ReportColumn("Owner DN");
			columns[(int)ColumnID.OwnerQuota] = new ReportColumn("Owner Quota (MB)", "{0:N02}");
			columns[(int)ColumnID.OwnerLastLogin] = new ReportColumn("Owner Last Login", "{0:G}");
			columns[(int)ColumnID.OwnerDisabled] = new ReportColumn("Owner Disabled");
			columns[(int)ColumnID.PreviousOwner] = new ReportColumn("Previous Owner");
			columns[(int)ColumnID.OrphanedOwner] = new ReportColumn("Orphaned Owner");
		}

		private iFolderReport()
		{
		}

		public static void Generate(string path, DateTime timestamp)
		{
			const double MB = 1024 * 1024;

			string fileName = Path.Combine(path,
				String.Format("ifolder-{0}.csv", timestamp.ToString("yyyyMMdd-HHmmss")));

			StreamWriter file = File.CreateText(fileName);

			try
			{
                Store store = Store.GetStore();
                Domain domain = store.GetDomain(store.DefaultDomain);

                // headers
				Report.WriteHeaderRow(file, columns);

				// list iFolders
				ICSList ifolders = store.GetCollectionsByType("iFolder");

				foreach(ShallowNode sn in ifolders)
				{
					object[] cells = new object[count];

					Collection ifolder = store.GetCollectionByID(sn.ID);
					Member owner = domain.GetMemberByID(ifolder.Owner.UserID);

					// cells
					cells[(int)ColumnID.ReportTime] = timestamp;
					cells[(int)ColumnID.iFolderSystem] = domain.Name;
					cells[(int)ColumnID.iFolderServer] = "localhost";
					cells[(int)ColumnID.iFolderID] = ifolder.ID;
					cells[(int)ColumnID.iFolderName] = ifolder.Name;
					cells[(int)ColumnID.iFolderSize] = ifolder.StorageSize / MB;
					cells[(int)ColumnID.iFolderPath] = ifolder.UnmanagedPath;
					cells[(int)ColumnID.iFolderQuota] = DiskSpaceQuota.GetLimit(ifolder) / MB;
					cells[(int)ColumnID.MemberCount] = ifolder.GetMemberList().Count;
					cells[(int)ColumnID.FileCount] = ifolder.GetNodesByType(NodeTypes.FileNodeType).Count;
					cells[(int)ColumnID.DirectoryCount] = ifolder.GetNodesByType(NodeTypes.DirNodeType).Count;
					cells[(int)ColumnID.OwnerID] = owner.UserID;
					cells[(int)ColumnID.OwnerName] = owner.FN;
					cells[(int)ColumnID.OwnerCN] = owner.Name;
					cells[(int)ColumnID.OwnerDN] = owner.Properties.GetSingleProperty("DN");
					cells[(int)ColumnID.OwnerQuota] = DiskSpaceQuota.Get(owner).Limit / MB;
					cells[(int)ColumnID.OwnerLastLogin] = owner.Properties.GetSingleProperty("LastLogin");
					cells[(int)ColumnID.OwnerDisabled] = domain.IsLoginDisabled(owner.UserID);
					cells[(int)ColumnID.PreviousOwner] = ifolder.PreviousOwner;
					cells[(int)ColumnID.OrphanedOwner] = ifolder.Properties.GetSingleProperty("OrphanedOwner");

					Report.WriteRow(file, columns, cells);
				}
			}
			catch(Exception e)
			{
				file.WriteLine();
				file.WriteLine();
				file.WriteLine(e);
				file.WriteLine(e.StackTrace);
			}
			finally
			{
				file.Close();
			}
		}
	}
}
