/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Text;

namespace Simias.Client.Event
{
	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public abstract class SimiasEventArgs : EventArgs
	{
		string					eventData;
		internal char			seperatorChar = '\0';
		DateTime				timeStamp;


		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="time"></param>
		public SimiasEventArgs(DateTime time) :
			this(null, time)
		{
		}

		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="eventData">Data of the event.</param>
		/// <param name="time">The time of this event.</param>
		public SimiasEventArgs(string eventData, DateTime time)
		{
			this.eventData = eventData;
			timeStamp = time;
		}

		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		public SimiasEventArgs() :
			this(null, DateTime.Now)
		{
		}
	
		#region Properties
		
		/// <summary>
		/// Gets the ChangeType for the event.
		/// </summary>
		public string EventData
		{
			get {return eventData;}
			set {eventData = value;}
		}

		/// <summary>
		/// Gets the timestamp for the event.
		/// </summary>
		public DateTime TimeStamp
		{
			get {return timeStamp;}
		}

		#endregion
	}
}
