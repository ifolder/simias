using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Simias;
using Simias.Web;
using Simias.Client;
using Simias.Server;
using Simias.Storage;
using Simias.Policy;
using Simias.Storage.Provider;

public class MissingNodesReport 
{
	private static string SimiasDataPath = "/var/simias/data/simias/";

               private enum ColumnID
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
                        OrphanedOwner,
                        LastSyncTime
                };
 /// <summary>
        /// Search Properties
        /// </summary>
        public enum SearchProperty
        {
                /// <summary>
                /// iFolder User Username
                /// </summary>
                UserName,

                /// <summary>
                /// iFolder User Full Name
                /// </summary>
                FullName,

                /// <summary>
                /// iFolder User Last Name
                /// </summary>
                LastName,

                /// <summary>
                /// iFolder User First Name
                /// </summary>
                FirstName,

                /// <summary>
                /// Groups and its members
                /// </summary>
                GroupOnly,

                /// <summary>
                /// iFolder User First Name
                /// </summary>
                HomeServerName,
        }
/// <summary>
        /// Search Operation
        /// </summary>
        public enum SearchOperation
        {
                /// <summary>
                /// Begins With
                /// </summary>
                BeginsWith,

                /// <summary>
                /// Ends With
                /// </summary>
                EndsWith,

                /// <summary>
                /// Contains
                /// </summary>
                Contains,

                /// <summary>
                /// Equals
                /// </summary>
                Equals,
        }


	private static readonly int count = Enum.GetNames( typeof( ColumnID ) ).Length;	
	static string disabledAtProperty = "IdentitySynchronization:DisabledAt";
	static int Main( string[] args)
	{
		if(args.Length != 1 )
		{
			Console.WriteLine("Usage: ./DeleteMissingNodes.sh <username>");
			return 0;
		}
		string username = args[0];
		Console.WriteLine("username entered is: "+username);
		GetReports(username);	
		return 0;
	}

	public static void GetReports(string username )
        {
		//if( replace == false ) Console.WriteLine("You have opted to view and not delete..Use enable to modify also "); else Console.WriteLine("It will modify the members");	
                Store.Initialize(SimiasDataPath, true , -1);
                Store store = Store.GetStore();
                if( store == null)
                        Console.WriteLine("store could not be initialized.....");
                Domain domain = store.GetDomain(store.DefaultDomain);

					Console.WriteLine("**************************IFOLDER REPORT********************* ");
					Console.WriteLine();
					Console.WriteLine();
			try
			{
				ArrayList allMembers = getAllDomainUsers(domain, username);
				int count = 1;
				foreach(Member member in allMembers)
				{
					Console.WriteLine("\n\n      User no: "+count);
					count++;
					try
					{
						CheckiFolderConsistency(store, domain, member);
						break;
					}
					catch(Exception e)
					{
						Console.WriteLine("Got exception, so continuing with next user.");
					}

				}
				
                        }
                        catch ( Exception ex )
                        {
                                Console.WriteLine();
                                Console.WriteLine();
                                Console.WriteLine( ex );
                                Console.WriteLine( ex.StackTrace );
                        }
			finally
			{	
                		Store.DeleteInstance();
			}
        }

	public static void CheckiFolderConsistency(Store store, Domain domain, Member member)
	{
				ICSList ColList = store.GetCollectionsByOwner( member.UserID );
                                // Now match the total number of files and dirs in the node and that on physical filesystem.
                                string UnManagedPath = null;
                                long missingFile = 0;
				string errorLine = "";
                                foreach( ShallowNode sn in ColList)
                                {

                                        Collection c = store.GetCollectionByID( sn.ID );
                                        if( c != null )
                                        {
                                                DirNode rootNode = c.GetRootDirectory();
                                                if (rootNode != null)
                                                {
                                                        Simias.Storage.Property localPath = rootNode.Properties.GetSingleProperty( PropertyTags.Root );
                                                        if( localPath != null)
                                                        {
                                                                UnManagedPath = localPath.Value as string;
								/*  Iterate over all the file nodes
								*/
                                                                ICSList FileList = c.GetNodesByType(NodeTypes.FileNodeType);
								errorLine += ("\n\niFolder Name: " + c.Name +"\n");
								errorLine += ("                Action If Any: ");
                                                                foreach (ShallowNode sn2 in FileList)
                                                                {
									try
									{
     	                                                                   Node fileNode = c.GetNodeByID(sn2.ID);
       	                                                                 Simias.Storage.Property property = fileNode.Properties.GetSingleProperty(PropertyTags.FileSystemPath);
       	                                                                 if (property != null)
       	                                                                 {
       	                                                                         string filePath = property.Value.ToString();
       	                                                                         string fullPath = Path.Combine(UnManagedPath, filePath);;
       	                                                                         if(! File.Exists( fullPath) )
       	                                                                         {
												String fileName = fileNode.Name;//fileNode.Properties.GetSingleProperty( "name" ).Value as string;
        	                                                                                // File entry in nodelist is not present in actual path so this user cannot be moved.
												c.Commit(c.Delete(fileNode));
												errorLine+= ( "       File Node Deleted: "+fileName);
               	                                                                                missingFile++;
												
               	                                                                 }
               	                                                         }
									}
									catch(Exception e)
									{Console.WriteLine("some exception while iteration of files.");}
                                                                }

								/*   Iterate over all the folder nodes.     */
                                                                ICSList DirList = c.GetNodesByType(NodeTypes.DirNodeType);
                                                                foreach (ShallowNode sn2 in DirList)
                                                                {
									try
									{
     	                                                                   Node folNode = c.GetNodeByID(sn2.ID);
       	                                                                 Simias.Storage.Property property = folNode.Properties.GetSingleProperty(PropertyTags.FileSystemPath);
       	                                                                 if (property != null)
       	                                                                 {
       	                                                                         string filePath = property.Value.ToString();
       	                                                                         string fullPath = Path.Combine(UnManagedPath, filePath);;
       	                                                                         if(! Directory.Exists( fullPath) )
       	                                                                         {
												String folderName = folNode.Name;
        	                                                                                // File entry in nodelist is not present in actual path so this user cannot be moved.
												c.Commit(c.Delete(folNode));
												errorLine+= ( "       Folder Node Deleted: "+folderName);
												missingFile++;
												
               	                                                                 }
               	                                                         }
									}
									catch(Exception e)
									{Console.WriteLine("some exception while iteration of files.");}
                                                                }

								
                                                        }
                                                }
                                        }
                                }
				Console.WriteLine();
				Console.WriteLine();
				if(missingFile == 0)
				{
					Console.WriteLine("*********************"+ member.FN +"   ("+member.Name+")           Passed  *******************");

				}
				else
				{
					Console.WriteLine("*********************"+ member.FN +"   ("+member.Name+")         " +missingFile+" inconsistent files or folders  *******************");
					Console.WriteLine(errorLine);

				}	
	}

	public static ArrayList getAllDomainUsers(Domain domain, string username)
	{
		SearchProperty searchProperty = SearchProperty.UserName;
		SearchOperation searchOperation = SearchOperation.BeginsWith;
		string pattern = "*";
		Simias.Storage.SearchPropertyList SearchPrpList = new Simias.Storage.SearchPropertyList();
		//SearchPrpList.Add(searchProperty, pattern, searchOperation);
		SearchPrpList.Add("DN","*", SearchOp.Exists);
		ICSList searchList = domain.Search(SearchPrpList);
		ArrayList list = new ArrayList();
		Member member = null;
		Console.WriteLine("Total No Of Users in Domain: "+searchList.Count);
		foreach(ShallowNode sn in searchList)
                {
                        try
                        {
			     if (sn.IsBaseType(NodeTypes.MemberType))
			     {
 	                            member = new Member(domain, sn);
				    Member mem = domain.GetMemberByID(member.UserID);
				    //add only those members to list whose home server is current server
				     if(mem.HomeServer != null)
					     if(String.Equals(mem.Name, username) && mem.HomeServer.UserID == HostNode.GetLocalHost().UserID)
					     {
						    	list.Add(mem);
							break;
					     }
			    }
	                }
                        catch (Exception ex)
                        {
                                                Console.WriteLine("Error: "+ex.Message);
                                                Console.WriteLine("Error Trace: "+ex.StackTrace);
                        }
                }
		Console.WriteLine("Total No Of Users for whom Home Server is current server: "+list.Count);
		return list;
	}

	        /// <summary>
        /// Class used to keep track of outstanding searches.
        /// </summary>
        internal class SearchState : IDisposable
        {
                #region Class Members
                /// <summary>
                /// Table used to keep track of outstanding search entries.
                /// </summary>
                static private Hashtable searchTable = new Hashtable();

                /// <summary>
                /// Indicates whether the object has been disposed.
                /// </summary>
                private bool disposed = false;

                /// <summary>
                /// Handle used to store and recall this context object.
                /// </summary>
                private string contextHandle = Guid.NewGuid().ToString();

                /// <summary>
                /// Identifier for the domain that is being searched.
                /// </summary>
                private string domainID;

                /// <summary>
                /// Object used to iteratively return the members from the domain.
                /// </summary>
                private ICSEnumerator enumerator;

                /// <summary>
                /// Total number of records contained in the search.
                /// </summary>
                private int totalRecords;

                /// <summary>
                /// The cursor for the caller.
                /// </summary>
                private int currentRecord = 0;

                /// <summary>
                /// The last count of records returned.
                /// </summary>
                private int previousCount = 0;
                #endregion
		                #region Properties
                /// <summary>
                /// Indicates if the object has been disposed.
                /// </summary>
                public bool IsDisposed
                {
                        get { return disposed; }
                }

                /// <summary>
                /// Gets the context handle for this object.
                /// </summary>
                public string ContextHandle
                {
                        get { return contextHandle; }
                }

                /// <summary>
                /// Gets or sets the current record.
                /// </summary>
                public int CurrentRecord
                {
                        get { return currentRecord; }
                        set { currentRecord = value; }
                }

                /// <summary>
                /// Gets the domain ID for the domain that is being searched.
                /// </summary>
                public string DomainID
                {
                        get { return domainID; }
                }

                /// <summary>
                /// Gets or sets the last record count.
                /// </summary>
                public int LastCount
                {
                        get { return previousCount; }
                        set { previousCount = value; }
                }

                /// <summary>
                /// Gets the search iterator.
                /// </summary>
                public ICSEnumerator Enumerator
                {
                        get { return enumerator; }
                }

		               /// <summary>
                /// Gets the total number of records contained by this search.
                /// </summary>
                public int TotalRecords
                {
                        get { return totalRecords; }
                }
                #endregion

                #region Constructor
                /// <summary>
                /// Initializes an instance of an object.
                /// </summary>
                /// <param name="domainID">Identifier for the domain that is being searched.</param>
                /// <param name="enumerator">Search iterator.</param>
                /// <param name="totalRecords">The total number of records contained in the search.</param>
                public SearchState( string domainID, ICSEnumerator enumerator, int totalRecords )
                {
                        this.domainID = domainID;
                        this.enumerator = enumerator;
                        this.totalRecords = totalRecords;

                        lock ( searchTable )
                        {
                                searchTable.Add( contextHandle, this );
                        }
                }
                #endregion

                #region Private Methods
                /// <summary>
                /// Removes this SearchState object from the search table.
                /// </summary>
                private void RemoveSearchState()
                {
                        lock ( searchTable )
                        {
                                // Remove the search context from the table and dispose it.
                                searchTable.Remove( contextHandle );
                        }
                }
                #endregion
		               #region Public Methods
                /// <summary>
                /// Returns a search context object that contains the state information for an outstanding search.
                /// </summary>
                /// <param name="contextHandle">Context handle that refers to a specific search context object.</param>
                /// <returns>A SearchState object if a valid one exists, otherwise a null is returned.</returns>
                static public SearchState GetSearchState( string contextHandle )
                {
                        lock ( searchTable )
                        {
                                return searchTable[ contextHandle ] as SearchState;
                        }
                }
                #endregion

                #region IDisposable Members
                /// <summary>
                /// Allows for quick release of managed and unmanaged resources.
                /// Called by applications.
                /// </summary>
                public void Dispose()
                {
                        RemoveSearchState();
                        Dispose( true );
                        GC.SuppressFinalize( this );
                }

                /// <summary>
                /// Dispose( bool disposing ) executes in two distinct scenarios.
                /// If disposing equals true, the method has been called directly
                /// or indirectly by a user's code. Managed and unmanaged resources
                /// can be disposed.
                /// If disposing equals false, the method has been called by the
                /// runtime from inside the finalizer and you should not reference
                /// other objects. Only unmanaged resources can be disposed.
                /// </summary>
                /// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
                private void Dispose( bool disposing )
                {
                        // Check to see if Dispose has already been called.
                        if ( !disposed )
                        {
                                // Protect callers from accessing the freed members.
                                disposed = true;

                                // If disposing equals true, dispose all managed and unmanaged resources.
                                if ( disposing )
                                {
                                        // Dispose managed resources.
                                        enumerator.Dispose();
                                }
                        }
                }

		              /// <summary>
                /// Use C# destructor syntax for finalization code.
                /// This destructor will run only if the Dispose method does not get called.
                /// It gives your base class the opportunity to finalize.
                /// Do not provide destructors in types derived from this class.
                /// </summary>
                ~SearchState()
                {
                        Dispose( false );
                }
                #endregion
        }

}
