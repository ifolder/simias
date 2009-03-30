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
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
 
using System;

using Simias.Event;
using Simias.Client.Event;

namespace Simias.Authentication
{
	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling need credentials events.
	/// </summary>
	public delegate void NeedCredentialsEventHandler(NeedCredentialsEventArgs args);

	#endregion

	/// <summary>
	/// Summary description for NeedCredentialsEventSubscriber.
	/// </summary>
	public class NeedCredentialsEventSubscriber
	{
		#region Events

		/// <summary>
		/// Event to recieve Need Credentials event.
		/// </summary>
		public event NeedCredentialsEventHandler NeedCredentials;
		
		#endregion

		#region Private Fields

		DefaultSubscriber	subscriber = null;
		bool		enabled;
		bool		alreadyDisposed;
		
		#endregion

		#region Constructor/Finalizer

        /// <summary>
        /// Constructor
        /// </summary>
		public NeedCredentialsEventSubscriber()
		{
			enabled = true;
			alreadyDisposed = false;
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent += new SimiasEventHandler(OnNeedCredentials);
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~NeedCredentialsEventSubscriber()
		{
			Dispose(true);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets and set the enabled state.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
			}
		}

		#endregion

		#region Callbacks

        /// <summary>
        /// Callback for the event Need credentials
        /// </summary>
        /// <param name="args">Simias event</param>
		private void OnNeedCredentials(SimiasEventArgs args)
		{
			try
			{
				if (enabled)
				{
					NeedCredentialsEventArgs cArgs = args as NeedCredentialsEventArgs;
					if (cArgs != null)
					{
						if (NeedCredentials != null)
						{
							Delegate[] cbList = NeedCredentials.GetInvocationList();
							foreach (NeedCredentialsEventHandler cb in cbList)
							{
								try 
								{ 
									cb(cArgs);
								}
								catch
								{
									NeedCredentials -= cb;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				new SimiasException(args.ToString(), ex);
			}
		}

		#endregion

		#region Private Methods

        /// <summary>
        /// Called to cleanup any resources
        /// </summary>
        /// <param name="inFinalize">True or false</param>
		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					alreadyDisposed = true;
					
					// Deregister delegates.
					subscriber.SimiasEvent -= new SimiasEventHandler(OnNeedCredentials);
					subscriber.Dispose();
					if (!inFinalize)
					{
						GC.SuppressFinalize(this);
					}
				}
			}
			catch {};
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Called to cleanup any resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
