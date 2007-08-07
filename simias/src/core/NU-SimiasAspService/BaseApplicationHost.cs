/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |
 | Mono.ASPNET.BaseApplicationHost
 |
 | Authors:
 |	Gonzalo Paniagua Javier (gonzalo@ximian.com)
 |  Lluis Sanchez Gual (lluis@ximian.com)
 |***************************************************************************/


using System;

namespace Mono.ASPNET
{
	public class BaseApplicationHost : MarshalByRefObject, IApplicationHost
	{
		string path;
		string vpath;
		IRequestBroker requestBroker;
		EndOfRequestHandler endOfRequest;
		
		public BaseApplicationHost ()
		{
			endOfRequest = new EndOfRequestHandler (EndOfRequest);
		}

		public override object InitializeLifetimeService ()
		{
			return null; // who wants to live forever?
		}
		
		public string Path {
			get {
				if (path == null)
					path = AppDomain.CurrentDomain.GetData (".appPath").ToString ();

				return path;
			}
		}

		public string VPath {
			get {
				if (vpath == null)
					vpath =  AppDomain.CurrentDomain.GetData (".appVPath").ToString ();

				return vpath;
			}
		}

		public AppDomain Domain {
			get { return AppDomain.CurrentDomain; }
		}
		
		public IRequestBroker RequestBroker
		{
			get { return requestBroker; }
			set { requestBroker = value; }
		}
		
		protected void ProcessRequest (MonoWorkerRequest mwr)
		{
			if (!mwr.ReadRequestData ()) {
				EndOfRequest (mwr);
				return;
			}
			
			mwr.EndOfRequestEvent += endOfRequest;
			mwr.ProcessRequest ();
		}

		public void EndOfRequest (MonoWorkerRequest mwr)
		{
			try {
				mwr.CloseConnection ();
			} catch {}
		}
	}
}
