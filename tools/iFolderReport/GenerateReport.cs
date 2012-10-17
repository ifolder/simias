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

public class EnableUserLogin 
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

	private static readonly int count = Enum.GetNames( typeof( ColumnID ) ).Length;	
	static string disabledAtProperty = "IdentitySynchronization:DisabledAt";
	static int Main( string[] args)
	{
		GetReports();	
		return 0;
	}

	public static void GetReports( )
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

			        ICSList ifolders = store.GetCollectionsByType( "iFolder" );
			        foreach( ShallowNode sn in ifolders )
                                {
                                        object[] cells = new object[ count ];

                                        Collection ifolder = store.GetCollectionByID( sn.ID );
                                        Member owner = domain.GetMemberByID( ifolder.Owner.UserID );

					Console.WriteLine("                          *************" );
					Console.WriteLine("user name is :{0} and iFolder name is :{1}",owner.FN, ifolder.Name);
					Console.WriteLine("iFolder quota is :{0}",DiskSpaceQuota.GetLimit( ifolder ));
					Console.WriteLine("Users quota is :{0}",DiskSpaceQuota.Get( owner ).Limit);
					Console.WriteLine("                         ");
					
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
