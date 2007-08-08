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
 | Author: Russ Young
 |***************************************************************************/


using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.SessionState;
using Simias.Storage;
using Simias.Sync.Http;

namespace Simias.Sync.Web
{
	public class SyncHandler : IHttpHandler, IRequiresSessionState
	{
		const string ServiceString = "SyncService";
		
		public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;
			HttpSessionState Session = context.Session;
			try
			{
				// Make sure that there is a session.
				if ( Session != null )
				{
					HttpService Service = (HttpService)Session[ServiceString];
					// Set no cache-ing.
					Response.Cache.SetCacheability(HttpCacheability.NoCache);

					string httpMethod = Request.HttpMethod;
					SyncMethod method = (SyncMethod)Enum.Parse(typeof(SyncMethod), Request.Headers.Get(SyncHeaders.Method), true);
					if (string.Compare(httpMethod, "POST", true) == 0)
					{
						// Determine What work we need to do.
						switch (method)
						{
							case SyncMethod.StartSync:
								Session.Timeout = 6;
								if (Service != null)
									Service.Dispose();
								Service = new HttpService();
								Session[ServiceString] = Service;
								Service.StartSync(Request, Response, Session);
								break;
							case SyncMethod.GetNextInfoList:
								Service.GetNextInfoList(Request, Response);
								break;
							case SyncMethod.PutNodes:
								Service.PutNodes(Request, Response);
								break;
							case SyncMethod.GetNodes:
								Service.GetNodes(Request, Response);
								break;
							case SyncMethod.PutDirs:
								Service.PutDirs(Request, Response);
								break;
							case SyncMethod.GetDirs:
								Service.GetDirs(Request, Response);
								break;
							case SyncMethod.DeleteNodes:
								Service.DeleteNodes(Request, Response);
								break;
							case SyncMethod.OpenFilePut:
								Service.OpenFilePut(Request, Response);
								break;
							case SyncMethod.OpenFileGet:
								Service.OpenFileGet(Request, Response);
								break;
							case SyncMethod.GetHashMap:
								Service.GetHashMap(Request, Response, false);
								break;
							case SyncMethod.GetHashMap2:
								Service.GetHashMap(Request, Response, true);
								break;
							case SyncMethod.PutHashMap:
								Service.PutHashMap(Request, Response);
								break;
							case SyncMethod.ReadFile:
								Service.ReadFile(Request, Response);
								break;
							case SyncMethod.WriteFile:
								Service.WriteFile(Request, Response);
								break;
							case SyncMethod.CopyFile:
								Service.CopyFile(Request, Response);
								break;
							case SyncMethod.CloseFile:
								Service.CloseFile(Request, Response);
								break;
							case SyncMethod.EndSync:
								Service.EndSync(Request, Response);
								Service.Dispose();
								Session.Remove(ServiceString);
								break;
							default:
								Response.StatusCode = (int)HttpStatusCode.BadRequest;
								break;
						}
					}
					else
					{
						Response.StatusCode = (int)HttpStatusCode.BadRequest;
					}
				}
				else
				{
					Response.StatusCode = (int)HttpStatusCode.BadRequest;
				}
			}
			catch (Exception ex)
			{
				Sync.Log.log.Debug("Request Failed exception\n{0}\n{1}", ex.Message, ex.StackTrace);
				throw ex;
			}
			Response.End();
		}

		public bool IsReusable
		{
			// To enable pooling, return true here.
			// This keeps the handler in memory.
			get { return true; }
		}
	}
}
