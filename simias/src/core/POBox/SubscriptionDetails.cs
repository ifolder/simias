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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 *
 ***********************************************************************/

using System;

namespace Simias.POBox
{
	/// <summary>
	/// Subscription Details
	/// </summary>
	[Serializable]
	public class SubscriptionDetails
	{
		/// <summary>
		/// The subscription collection dir node id
		/// </summary>
		public string DirNodeID;

		/// <summary>
		/// The subscription collection dir node name
		/// </summary>
		public string DirNodeName;

		/// <summary>
		/// The subscription collection service URL
		/// </summary>
		public string CollectionUrl;
	}
}