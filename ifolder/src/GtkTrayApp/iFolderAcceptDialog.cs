/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
	public class iFolderAcceptDialog : Dialog
	{
		private Entry		pathEntry;

		public string Path
		{
			get
			{
				return pathEntry.Text;
			}
		}

		public iFolderAcceptDialog(iFolder ifolder) : base()
		{
			this.Title = "Setup iFolder \"" + ifolder.Name + "\"";
			this.SetDefaultSize (500, 200);

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));

//			this.BorderWidth = 10;
			VBox dialogBox = new VBox();
			dialogBox.Spacing = 10;
			dialogBox.BorderWidth = 10;
			dialogBox.Homogeneous = false;
			this.VBox.PackStart(dialogBox, true, true, 0);


			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
						"Setup iFolder: \"" + ifolder.Name + "\"</span>");

			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0; l.Yalign = 0;
			dialogBox.PackStart(l, false, false, 0);


			VBox detailBox = new VBox();
			dialogBox.PackStart(detailBox, true, true, 0);

			l = new Label("iFolder Details:");
			l.Xalign = 0;
			detailBox.PackStart(l, false, false, 0);

			TextView tv = new TextView();
			tv.LeftMargin = 10;
			tv.RightMargin = 10;
			tv.Editable = false;
			tv.CursorVisible = false;
			TextBuffer buffer = tv.Buffer;
			buffer.Text = string.Format("iFolder name: {0}\niFolder Owner:{1}\n\niFolder Description: {2}", ifolder.Name, ifolder.Owner, ifolder.Description);

			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(tv);
			detailBox.PackStart(sw, true, true, 0);


			l = new Label("Choose a location for the iFolder to be created on this computer.");

			l.LineWrap = false;
			l.Xalign = 0; l.Yalign = 0;
			dialogBox.PackStart(l, false, false, 0);



			VBox locBox = new VBox();
			dialogBox.PackEnd(locBox, false, true, 0);

			Label pathLabel = new Label("iFolder Location:");
			pathLabel.Xalign = 0;
			locBox.PackStart(pathLabel, false, true, 0);

			HBox pathBox = new HBox();
			pathBox.Spacing = 10;
			locBox.PackStart(pathBox, false, true, 0);

			pathEntry = new Entry();
			pathEntry.Changed += new EventHandler(OnPathChanged);
			pathBox.PackStart(pathEntry, true, true, 0);

			Button pathButton = new Button(Stock.Open);
			pathButton.Clicked += new EventHandler(OnChoosePath);
			pathBox.PackEnd(pathButton, false, false, 0);


			this.VBox.ShowAll();

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.SetResponseSensitive(ResponseType.Ok, false);
		}


		private void OnChoosePath(object o, EventArgs args)
		{
			// create a file selection dialog and turn off all of the
			// file operations and controlls
			FileSelection fs = new FileSelection ("Choose a folder...");
			fs.FileList.Parent.Hide();
			fs.SelectionEntry.Hide();
			fs.FileopDelFile.Hide();
			fs.FileopRenFile.Hide();
			fs.TransientFor = this;

			int rc = fs.Run ();
			fs.Hide();
			if(rc == -5)
			{
				pathEntry.Text = fs.Filename;
			}
		}



		private void OnPathChanged(object obj, EventArgs args)
		{
			if(pathEntry.Text.Length > 0)
			{
				this.SetResponseSensitive(ResponseType.Ok, true);
			}
			else
			{
				this.SetResponseSensitive(ResponseType.Ok, false);
			}
		}


	}
}
