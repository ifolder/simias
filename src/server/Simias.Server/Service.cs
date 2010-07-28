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
*                 $Author: Brady Anderson <banderso@novell.com>
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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

using Simias;
using Simias.Event;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;

namespace Simias.Server
{
	/// <summary>
	/// Class the handles presence as a service
	/// </summary>
	public class Service : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Simias.Server.Authentication authProvider = null;
		private Simias.Server.IUserProvider userProvider = null;
		private Simias.IIdentitySyncProvider syncProvider = null;
		
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public Service()
		{
		}
		#endregion
		
		#region Private Members
        /// <summary>
        /// load the identitysync provider from config file
        /// </summary>
        /// <returns>true if loaded successfully</returns>
		private bool LoadIdentityProvider()
		{
			bool status = false;
			
			// Bootstrap the identity provider from the Simias.config file
			Simias.Configuration config = Store.Config;
			string assemblyName = String.Empty;
			if(Simias.Service.Manager.LdapServiceEnabled == true)
				assemblyName = config.Get( "Identity", "ServiceAssembly" );
			else
				assemblyName = config.Get( "Identity", "Assembly" );
			string userClass = config.Get( "Identity", "Class" );
			
			if ( assemblyName != null && userClass != null )
			{
				log.Debug( "Identity assembly: {0}  class: {1}", assemblyName, userClass );
				Assembly idAssembly = Assembly.LoadWithPartialName( assemblyName );
				if ( idAssembly != null )
				{
					Type type = idAssembly.GetType( userClass );
					if ( type != null )
					{
				log.Debug( "Identity assemblytype name : {0}  namespace: {1}", type.FullName, type.Namespace );
						userProvider = Activator.CreateInstance( type ) as IUserProvider;
						if ( userProvider != null )
						{
							log.Debug( "created user provider instance" );
							User.RegisterProvider( userProvider );
							status = true;
							
							// does this provider support external syncing?
							foreach( Type ctype in idAssembly.GetTypes() )
							{
								foreach( Type itype in ctype.GetInterfaces() )
								{
									if ( Simias.IdentitySync.Service.master && itype == typeof( Simias.IIdentitySyncProvider ) )
									{
										syncProvider = 
											Activator.CreateInstance( ctype ) as IIdentitySyncProvider;
										if ( syncProvider != null )
										{
											Simias.IdentitySync.Service.Register( syncProvider );
											log.Debug( "created sync provider instance" );
										}
										else
										{
											log.Debug( "failed to create an instance of IIdentitySyncProvider" );
										}
										break;
									}	
								}
								
								if ( syncProvider != null )
								{
									break;
								}
							}
						}
						else
							log.Debug( "CreateInstance returned null userProvider" );
					}							
					else
						log.Debug( "GetType returned null" );
				}
				else
					log.Debug( "LoadWithPartialName returned null" );
			}
			
			// If we couldn't load the configured provider
			// load the internal user/identity provider
			if ( status == false )
			{
				if ( userProvider == null )
				{
					log.Info( "Could not load the configured user provider - loading InternalUser" );
					userProvider = new Simias.Server.InternalUser();
					User.RegisterProvider( userProvider );
					status = true;
				}	
			}
			
			return status;
		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		public void Start()
		{
			log.Debug( "Start called" );

			//Thread.Sleep( 30 * 1000 );
			
			// Instantiate the server domain
			// The domain will be created the first time the
			// server is run
			EnterpriseDomain enterpriseDomain = new EnterpriseDomain( true );
			if ( enterpriseDomain != null )
			{
				new Simias.Host.HostProvider( enterpriseDomain.GetServerDomain( false ) );
				//new Simias.Host.HostProvider( new EnterpriseDomain( false ).GetServerDomain( false ) );

				if(Simias.Service.Manager.LdapServiceEnabled == false)
				{
					// Valid enterprise domain - start the external
					// identity sync service
					Simias.IdentitySync.Service.Start();
				}
				
				if ( userProvider == null )
				{
					LoadIdentityProvider();
				}	
				
				if(Simias.Service.Manager.LdapServiceEnabled == false)
				{
					// Register with the domain provider service.
					if ( authProvider == null )
					{
						authProvider = new Simias.Server.Authentication();
						DomainProvider.RegisterProvider( this.authProvider );
					}
				}

				Simias.Server.Catalog.StartCatalogService();
				ExtractMemberPoliciesOnMaster();
				CheckStoreAndLoadRA();
				CheckServerForChageMasterError();
			}
		}

		/// <summary>
		/// Run repair on the node, verify the inconsistancy on the node 
		/// </summary>
		/// <returns> true/false for success/failure</returns>
		public static bool VerifyChangeMaster()
		{
			bool status = true;
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				HostNode[] hosts = Simias.Storage.HostNode.GetHosts(domain.ID);
				foreach(HostNode host in hosts)
				{
					if( domain.Role ==  Simias.Sync.SyncRoles.Master ) 
					{
						if ( host.IsLocalHost == true )
							host.IsMasterHost = true;
						else
							host.IsMasterHost = false;
					}
					domain.Commit(host);
				}
			}
			catch (Exception ex)
			{
				log.Error("Exception throw at VerifiyChangeMaster() : {0} : {1}", ex.Message, ex.StackTrace);
				status = false;
			}
			return status;
		}

		public void CheckServerForChageMasterError()
		{
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				HostNode localhostNode = HostNode.GetLocalHost();
				if ( localhostNode.ChangeMasterState != -1 )
				{
					if( (int)HostNode.changeMasterStates.Verified != localhostNode.ChangeMasterState ) 
					{
						if (VerifyChangeMaster())
						{
							localhostNode.ChangeMasterState = (int)HostNode.changeMasterStates.Verified;
							domain.Commit(localhostNode);
						}
					}
				}
			}
			catch (Exception ex)
			{	//will check at next restart
				log.Error("Exception at CheckServerForChageMasterError:{0} : {1}", ex.Message, ex.StackTrace);
			}
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		public void CheckStoreAndLoadRA()
		{
			SimiasAccessLogger accessLog = new SimiasAccessLogger("Service","Loading RA's");
			Store store = Store.GetStore();
			//Load the RSA for the domain - need to see how this can be migrated --FIXME
                        if(store.DefaultDomain != null)
                        {
                                //Store the DEFAULT certificate(RSA information) for users using the "Server Default" option in client
                                // need to find a better way of representing DEFAULT
                                Simias.Security.RSAStore.CheckAndStoreRSA(store.DefaultRSARA.ToXmlString(true), "DEFAULT", true);
                        }
                        X509Certificate raCert = null;
                        try
                        {
                                Simias.Configuration config = Store.Config;
                                string raPath = config.Get( "Server", "RAPath" );

                                if (raPath != null && raPath != String.Empty && raPath != "")
                                {
                        		string[] racertFiles = Directory.GetFiles( raPath, "*.?er" );
                                        Simias.Security.CertificateStore.CleanCertsFromStore();
                                        foreach ( string file in racertFiles )
                                        {
                                                try
                                                {
                                                 raCert = X509Certificate.CreateFromCertFile(file);
                                                }
                                                catch(CryptographicException ce)
                                                {
                                                        log.Debug("Exception {0}, File: {1}", ce.ToString(), file);
                                                        continue;
                                                }
                                                //Simias.Security.CertificateStore.StoreRACertificate (raCert.GetRawCertData(), raCert.GetName().ToLower(), true);
                                                Simias.Security.CertificateStore.StoreRACertificate (raCert.GetRawCertData(), Path.GetFileNameWithoutExtension(file).ToLower(), true);
						accessLog.LogAccess("CheckStoreAndLoadRA","Loading RecoveryAgent","-",raCert.GetName());
                                        }
                                }
                        }
                        catch (Exception e)
                        {
                                log.Error (e.ToString());
				accessLog.LogAccess("CheckStoreAndLoadRA","Failed Loading RecoveryAgent","-","-");
                        }

                        Simias.Security.CertificateStore.LoadRACertsFromStore(); //this loads all Certs including RA - but client will not have RA
                        if(store.DefaultDomain != null) //load the RSA data from store - only on server
                                Simias.Security.RSAStore.LoadRSAFromStore();
		}

		/// <summary>
		/// It removes All User's POBox . Before that it extracts all the policy related information from User's POBox and store them
		/// as part of member object. This is used in cases where server is upgraded from a lower version to 3.7 . Even if server is
		/// not upgraded, whenever server is restarted, this code will remove POBox for all users.
		/// It does not search those policies which were not present in earlier version. e.g sharing
		/// </summary>
		public void ExtractMemberPoliciesOnMaster()
		{
			log.Debug("ExtractMemberPoliciesOnMaster: entered");
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain) ;
			HostNode hNode = null;
			HostNode mNode = null;
			HostNode masterNode = null;
			bool OnMaster = false;
			try
			{
				if(domain == null)
				{
					throw new Exception("store not initialized yet");
				}		
				hNode = HostNode.GetLocalHost();
				Collection c = domain;
				ICSList members = c.GetMemberList();
				foreach(ShallowNode sn in members)
				{
					Member member = new Member(c, sn);
					if(member == null)
					{
						//throw new Exception("cannot form member object !!!!");
						log.Debug("bug : Member is null !!!");
						continue;
					}
					if (member.IsType("Host"))
					{
						continue;
					}	
					if(hNode == null )
					{
						log.Debug("local host is null");
						return;
					}
					mNode = member.HomeServer;
					if( mNode == null)
					{
						continue;
					}
					masterNode = HostNode.GetMaster(domain.ID);
					OnMaster = false;
					if(hNode.UserID == masterNode.UserID)
					{
						// local host is master so no need to do web-service call during commit 
						OnMaster = true;
					}
					ExtractMemberPolicies(domain.ID, member, sn, OnMaster);	
				
				}
			}
			catch(Exception ex)
			{
				log.Debug("Extracting of member policies from local server and setting on master failed..."+ex.ToString());
			}
			
		}
		


		/// <summary>
		/// Extracts all the member policies from POBox and stores them as part of member object on master . No aggregation is done. 
		/// </summary>
		public void ExtractMemberPolicies(string domainID, Member member, ShallowNode sn, bool OnMaster)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(domainID) ;
			string RuleList = "RuleList";
			Simias.Policy.Rule rule = null;
			int Value = -1;
			bool committed = false;
			Property FileTypeProperty;
			Property EncProperty;
			Property SyncIntervalProperty;
			ICSList list;
				
			POBox.POBox poBox = POBox.POBox.FindPOBox( store, domainID, member.UserID );
			if(poBox == null)
			{
				return;
			}

			// First extract rule based policies

			// Extracting FileType policy
			list = poBox.Search( PropertyTags.PolicyID, Simias.Policy.FileTypeFilter.FileTypeFilterPolicyID, SearchOp.Equal );
			Simias.Policy.Policy tempPolicy = null;
			foreach (ShallowNode snl in list)
			{
				tempPolicy = new Simias.Policy.Policy(poBox, snl);
				if (tempPolicy.IsSystemPolicy)
				{
					MultiValuedList mvl = tempPolicy.Properties.GetProperties(RuleList);
					if (mvl.Count > 0)
					{
						log.Debug("mvl count for filetype filter is"+mvl.Count);
						foreach (Property p in mvl)
						{
							if (p != null)
							{
								rule = new Simias.Policy.Rule(p.Value);
								FileTypeProperty = new Property(Simias.Policy.FileTypeFilter.FileTypeFilterPolicyID, rule.ToString());	
								FileTypeProperty.ServerOnlyProperty = true;
								member.Properties.AddNodeProperty(FileTypeProperty);
							}
						}
					}
				}
			}			
			GetRuleForPolicyID(poBox, Simias.Policy.DiskSpaceQuota.DiskSpaceQuotaPolicyID, ref member);
			GetRuleForPolicyID(poBox, Simias.Policy.FileSizeFilter.FileSizeFilterPolicyID, ref member);
			Value = GetValueForPolicyID(poBox, Simias.Policy.SecurityState.EncryptionStatePolicyID, Simias.Policy.SecurityState.StatusTag);	
			if(Value != -1)
			{
				EncProperty = new Property(Simias.Policy.SecurityState.EncryptionStatePolicyID, Value);
				EncProperty.ServerOnlyProperty = true;
				member.Properties.ModifyProperty(EncProperty);
			}	
			Value = GetValueForPolicyID(poBox, Simias.Policy.SyncInterval.SyncIntervalPolicyID, Simias.Policy.SyncInterval.IntervalTag);	
			if(Value != -1)
			{
				SyncIntervalProperty = new Property(Simias.Policy.SyncInterval.SyncIntervalPolicyID, Value);
				SyncIntervalProperty.ServerOnlyProperty = true;
				member.Properties.ModifyProperty(SyncIntervalProperty);
			}

			if(OnMaster)
			{
				// User is provisioned on master so no need to call web-service
				domain.Commit(member);
				poBox.Commit(poBox.Delete());
				log.Debug("Committed member's property(policy) on master successfully so deleting his POBox.. "+member.FN);
			}
			else
			{
			
				committed = CommitOnMaster(domainID, member, sn);
				if(committed == true)
				{

					log.Debug("Committed member's property(policy) on master successfully so deleting his POBox.. "+member.FN);
					poBox.Commit(poBox.Delete());
				}
				else
				{
					log.Debug("Could not commit member's property(policy) on master so not deleting his POBox.. Next time it can be tried");
			
				}
			}	
		}
		
        /// <summary>
        /// Commits the member object on master server, will be called by slave
        /// </summary>
        /// <param name="domainID">domain id</param>
        /// <param name="member">member object to be committed</param>
        /// <param name="sn">shallow node object, but currently unused</param>
        /// <returns>true if success</returns>
		public bool CommitOnMaster(string domainID, Member member, ShallowNode sn)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(domainID);
			string userID = store.GetUserIDFromDomainID(domain.ID);
			HostNode mNode = HostNode.GetMaster(domainID);
			bool result = false;
			if(mNode == null)
			{
				return false;
			}
			try
			{
				Node ModifiedNode = member as Node;
				log.Debug("going to call xnode constr and loading its : ");
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(ModifiedNode.Properties.ToString());
				log.Debug("modifiednode.prop.string is : "+ModifiedNode.Properties.ToString());

				SimiasConnection smConn = new SimiasConnection(domainID, userID, SimiasConnection.AuthType.PPK, mNode);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = mNode.PublicUrl;
				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");
				log.Debug("going to call svc.commitdomainmember");
				result = svc.CommitDomainMember(domain.ID, xNode);
				log.Debug("returned from web-service call and return is  "+result);
			}
			catch(Exception ex)
			{
				log.Debug("simiasconnection to master failed: "+ex.ToString());
				log.Debug("Could not establish connection to master for user: "+member.UserID);
				result = false;
			}
			return result;
		}


		/// <summary>
		/// Given an poBox object , it will return the rule for a PolicyID 
		/// only one rule is returned
		/// </summary>
		public void GetRuleForPolicyID(POBox.POBox poBox, string PolicyID, ref Member member)
		{
			string RuleList = "RuleList";
			Simias.Policy.Rule rule = null;
			ICSList list = poBox.Search( PropertyTags.PolicyID, PolicyID, SearchOp.Equal );
			foreach (ShallowNode sn in list)
			{
				Simias.Policy.Policy tempPolicy = new Simias.Policy.Policy(poBox, sn);
				if (tempPolicy.IsSystemPolicy)
				{
					MultiValuedList mvl = tempPolicy.Properties.GetProperties(RuleList);
					if (mvl.Count > 0)
					{
						foreach (Property p in mvl)
						{
							if (p != null)
							{
								rule = new Simias.Policy.Rule(p.Value);
								Property prop = new Property(PolicyID, rule.ToString());
								prop.ServerOnlyProperty = true;
								member.Properties.ModifyProperty(prop);
								break;
							}
						}
					}
				}
			}		
			return ;
		}

		/// <summary>
		/// Given an poBox object , it will return the value for a PolicyID , and PolicyTag . 
		/// Only one value is returned
		/// </summary>
		public int GetValueForPolicyID(POBox.POBox poBox, string PolicyID, string PolicyTag)
		{
			int PolicyValue = -1;
			ICSList list = poBox.Search( PropertyTags.PolicyID, PolicyID, SearchOp.Equal );
			foreach (ShallowNode sn in list)
			{
				Simias.Policy.Policy tempPolicy = new Simias.Policy.Policy(poBox, sn);
				if (tempPolicy.IsSystemPolicy)
				{
					MultiValuedList mvl = tempPolicy.Properties.GetProperties(PolicyTag);
					if (mvl.Count > 0)
					{
						foreach (Property p in mvl)
						{
							if (p != null)
							{
								PolicyValue = (int) p.Value;
								break;
							}
						}
					}
				}
			}		
			return PolicyValue;
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom(int message, string data)
		{
			return 0;
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			log.Debug( "Stop called" );

			Simias.Server.Catalog.StopCatalogService();
			Simias.IdentitySync.Service.Stop();
			
			if ( syncProvider != null )
			{
				IdentitySync.Service.Unregister( syncProvider );
				syncProvider = null;
			}
			
			if ( authProvider != null )
			{
				DomainProvider.Unregister( authProvider );
				authProvider = null;
			}
			
			if ( userProvider != null )
			{
				User.UnregisterProvider( userProvider );
				userProvider = null;
			}
		}
		#endregion
	}
}
