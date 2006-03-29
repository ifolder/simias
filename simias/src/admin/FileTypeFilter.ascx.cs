/***********************************************************************
 *  $RCSfile: FileTypeFilter.ascx.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Mike Lasky (mlasky@novell.com)
 * 
 ***********************************************************************/
namespace Novell.iFolderWeb.Admin
{
	using System;
	using System.Collections;
	using System.Data;
	using System.Drawing;
	using System.Resources;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for FileTypeFilter.
	/// </summary>
	public class FileTypeFilter : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// File Type data grid column indices.
		/// </summary>
		private const int FileTypeRegExColumn = 0;
		private const int FileTypeEnabledColumn = 1;
		private const int FileTypeNameColumn = 2;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// File Type policy controls.
		/// </summary>
		protected DataGrid FileTypeList;

		/// <summary>
		/// Event that notifies consumer that the filter list has changed.
		/// </summary>
		public event EventHandler ListChanged = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the file type data source value.
		/// </summary>
		private Hashtable FileTypeSource
		{
			get { return ViewState[ "FileTypeSource" ] as Hashtable; }
			set { ViewState[ "FileTypeSource" ] = value; }
		}

		#endregion

		#region Private Methods
        
		/// <summary>
		/// Creates the file type list view for the web control.
		/// </summary>
		private DataView CreateFileTypeListView()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "FileRegExField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "EnabledField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "FileNameField", typeof( string ) ) );

			// Fill the data table from the saved selected member list.
			Hashtable ht = FileTypeSource;
			FileTypeInfo[] ftInfoList = new FileTypeInfo[ ht.Count ];

			// Copy the Values to the array so that they can be sorted.
			ht.Values.CopyTo( ftInfoList, 0 );
			Array.Sort( ftInfoList );

			foreach( FileTypeInfo fti in ftInfoList )
			{
				dr = dt.NewRow();
				dr[ 0 ] = fti.RegExFileName;
				dr[ 1 ] = fti.IsEnabled;
				dr[ 2 ] = fti.FriendlyFileName;

				dt.Rows.Add( dr );
			}

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Creates a stateful list of file type filters.
		/// </summary>
		/// <param name="policy">User policy object</param>
		/// <returns>A hashtable containing the file type filters.</returns>
		private Hashtable CreateFileTypeSource( UserPolicy policy )
		{
			// Keep the state in a hashtable.
			Hashtable ht = new Hashtable();
			foreach( string s in policy.FileTypesExcludesEffective )
			{
				ht[ s ] = new FileTypeInfo( s, Utils.ConvertFromRegEx( s ), IsFilterEnabled( policy, s ) );
			}

			return ht;
		}

		/// <summary>
		/// Creates a stateful list of file type filters.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		/// <returns>A hashtable containing the file type filters.</returns>
		private Hashtable CreateFileTypeSource( iFolderPolicy policy )
		{
			// Keep the state in a hashtable.
			Hashtable ht = new Hashtable();
			foreach( string s in policy.FileTypesExcludes )
			{
				ht[ s ] = new FileTypeInfo( s, Utils.ConvertFromRegEx( s ), IsFilterEnabled( policy, s ) );
			}

			return ht;
		}

		/// <summary>
		/// Creates a stateful list of file type filters.
		/// </summary>
		/// <param name="policy">System policy object</param>
		/// <returns>A hashtable containing the file type filters.</returns>
		private Hashtable CreateFileTypeSource( SystemPolicy policy )
		{
			// Keep the state in a hashtable.
			Hashtable ht = new Hashtable();
			foreach( string s in policy.FileTypesExcludes )
			{
				ht[ s ] = new FileTypeInfo( s, Utils.ConvertFromRegEx( s ), IsFilterEnabled( policy, s ) );
			}

			return ht;
		}

		/// <summary>
		/// Gets whether the specified file type is in enabled in the exception list.
		/// </summary>
		/// <param name="policy">User policy</param>
		/// <param name="fileType">Name of file type.</param>
		/// <returns></returns>
		private bool IsFilterEnabled( UserPolicy policy, string fileType )
		{
			return ( Array.IndexOf( policy.FileTypesIncludes, fileType ) != -1 ) ? true : false;
		}

		/// <summary>
		/// Gets whether the specified file type is in enabled in the exception list.
		/// </summary>
		/// <param name="policy">iFolder policy</param>
		/// <param name="fileType">Name of file type.</param>
		/// <returns></returns>
		private bool IsFilterEnabled( iFolderPolicy policy, string fileType )
		{
			return ( Array.IndexOf( policy.FileTypesIncludes, fileType ) != -1 ) ? true : false;
		}

		/// <summary>
		/// Gets whether the specified file type is in enabled in the exception list.
		/// </summary>
		/// <param name="policy">System policy</param>
		/// <param name="fileType">Name of file type.</param>
		/// <returns></returns>
		private bool IsFilterEnabled( SystemPolicy policy, string fileType )
		{
			return ( Array.IndexOf( policy.FileTypesIncludes, fileType ) != -1 ) ? true : false;
		}

		/// <summary>
		/// Event handler for when this page is loaded.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				FileTypeList.Columns[ FileTypeEnabledColumn ].HeaderText = GetString( "ALLOW" );
				FileTypeList.Columns[ FileTypeNameColumn ].HeaderText = GetString( "FILENAME" );
				FileTypeList.PagerStyle.NextPageText = GetString( "NEXT" );
				FileTypeList.PagerStyle.PrevPageText = GetString( "PREV" );
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the file type enable checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void FileTypeCheckChanged( Object sender, EventArgs e )
		{
			CheckBox checkBox = sender as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string fileName = item.Cells[ 0 ].Text;
			if ( fileName != "&nbsp;" )
			{
				FileTypeInfo fti = FileTypeSource[ fileName ] as FileTypeInfo;
				if ( fti != null )
				{
					fti.IsEnabled = checkBox.Checked;
				}
			}

			// Refresh the policy view.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();

			// Indicate an event that the list has changed.
			if ( ListChanged != null )
			{
				ListChanged( this, e );
			}
		}

		/// <summary>
		/// Event handler that gets called when the page changes on the data grid.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void FileTypePageChanged( Object sender, DataGridPageChangedEventArgs e )
		{
			FileTypeList.CurrentPageIndex = e.NewPageIndex;
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key">Key to the localized string.</param>
		/// <returns>Localized string.</returns>
		protected string GetString( string key )
		{
			return rm.GetString( key );
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the file type policy for the current user.
		/// </summary>
		/// <param name="policy">User policy.</param>
		public void GetFileTypePolicy( UserPolicy policy )
		{
			// Create a list from the file type policy.
			FileTypeSource = CreateFileTypeSource( policy );
			if ( FileTypeSource.Count > 0 )
			{
				// Build the data view from the table.
				FileTypeList.DataSource = CreateFileTypeListView();
				FileTypeList.DataBind();
				FileTypeList.Visible = true;
			}
			else
			{
				FileTypeList.Visible = false;
			}
		}

		/// <summary>
		/// Gets the file type policy for the current ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy.</param>
		public void GetFileTypePolicy( iFolderPolicy policy )
		{
			// Create a list from the file type policy.
			FileTypeSource = CreateFileTypeSource( policy );
			if ( FileTypeSource.Count > 0 )
			{
				// Build the data view from the table.
				FileTypeList.DataSource = CreateFileTypeListView();
				FileTypeList.DataBind();
				FileTypeList.Visible = true;
			}
			else
			{
				FileTypeList.Visible = false;
			}
		}

		/// <summary>
		/// Gets the file type policy for the system.
		/// </summary>
		/// <param name="policy">System policy.</param>
		public void GetFileTypePolicy( SystemPolicy policy )
		{
			// Create a list from the file type policy.
			FileTypeSource = CreateFileTypeSource( policy );
			if ( FileTypeSource.Count > 0 )
			{
				// Build the data view from the table.
				FileTypeList.DataSource = CreateFileTypeListView();
				FileTypeList.DataBind();
				FileTypeList.Visible = true;
			}
			else
			{
				FileTypeList.Visible = false;
			}
		}

		/// <summary>
		/// Sets the file type policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new file type filter will be set.</param>
		public void SetFileTypePolicy( UserPolicy policy )
		{
			// Build a list of checked file types.
			ArrayList filterList = new ArrayList();
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsEnabled )
				{
					filterList.Add( fti.RegExFileName );
				}
			}

			// Set the user current user policy.
			policy.FileTypesIncludes = filterList.ToArray( typeof( string ) ) as string[];
		}

		/// <summary>
		/// Sets the file type policy for this ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy that the new file type filter will be set.</param>
		public void SetFileTypePolicy( iFolderPolicy policy )
		{
			// Build a list of checked file types.
			ArrayList filterList = new ArrayList();
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsEnabled )
				{
					filterList.Add( fti.RegExFileName );
				}
			}

			// Set the user current user policy.
			policy.FileTypesIncludes = filterList.ToArray( typeof( string ) ) as string[];
		}

		/// <summary>
		/// Sets the file type policy for the system.
		/// </summary>
		/// <param name="policy">System policy that the new file type filter will be set.</param>
		public void SetFileTypePolicy( SystemPolicy policy )
		{
			// Build a list of checked file types.
			ArrayList filterList = new ArrayList();
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsEnabled )
				{
					filterList.Add( fti.RegExFileName );
				}
			}

			// Set the user current system policy.
			policy.FileTypesIncludes = filterList.ToArray( typeof( string ) ) as string[];
		}

		#endregion

		#region Web Form Designer generated code

		/// <summary>
		/// OnInit
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			FileTypeList.PageIndexChanged += new DataGridPageChangedEventHandler( FileTypePageChanged );
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		#region FileTypeInfo Class

		/// <summary>
		/// Class used to hold File Type filter information.
		/// </summary>
		[ Serializable() ]
			private class FileTypeInfo : IComparable
		{
			#region Class Members

			/// <summary>
			/// The regular expression version of the file name.
			/// </summary>
			public string RegExFileName;

			/// <summary>
			/// The friendly version of the file name.
			/// </summary>
			public string FriendlyFileName;

			/// <summary>
			/// If the file name is enabled as a filter.
			/// </summary>
			public bool IsEnabled;

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="regExFileName"></param>
			/// <param name="friendlyFileName"></param>
			/// <param name="enabled"></param>
			public FileTypeInfo( string regExFileName, string friendlyFileName, bool enabled )
			{
				RegExFileName = regExFileName;
				FriendlyFileName = friendlyFileName;
				IsEnabled = enabled;
			}

			#endregion

			#region IComparable Members

			/// <summary>
			/// Compares the current instance with another object of the same type.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public int CompareTo( object obj )
			{
				return String.Compare( FriendlyFileName, ( obj as FileTypeInfo ).FriendlyFileName, false );
			}

			#endregion
		}

		#endregion
	}
}
