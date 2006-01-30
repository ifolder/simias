using System;
using System.IO;

using Simias;
using Simias.Client;
using Simias.Storage;

namespace OrphanAdopt
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Orphan
	{
		private static bool commit = false;
		private static bool quiet = false;

		/// <summary>
		/// Parses the command line parameters and environment variables to get
		/// the configuration for Simias.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		/// <returns>True if successful.</returns>
		private static bool ParseConfigurationParameters( string[] args )
		{
			bool status = true;

			for ( int i = 0; i < args.Length; ++i )
			{
				switch ( args[ i ].ToLower() )
				{
					case "--commit":
					{
						commit = true;
						break;
					}

					case "--quiet":
					{
						quiet = true;
						break;
					}

					case "--help":
					{
						ShowUseage();
						status = false;
						break;
					}

					default:
					{
						// Unknown command line option.
						Console.Error.WriteLine( "{0} is an invalid command line option.", args[ i ] );
						status = false;
						break;
					}
				}
			}

			return status;
		}

		private static void ShowUseage()
		{
			Console.WriteLine();
			Console.WriteLine( "OrphanAdopt is a test application to restore orphaned collections back to" );
			Console.WriteLine( "their orignal owner, if the owner member still exists in the domain." );
			Console.WriteLine();
			Console.WriteLine( "Command line arguments:" );
			Console.WriteLine( "    --commit:" );
			Console.WriteLine( "        This parameter is optional." );
			Console.WriteLine( "        Specifies to write the changes to the store. If this parameter" );
			Console.WriteLine( "        is not specified, the changes will be output to the console but" );
			Console.WriteLine( "        not committed." );
			Console.WriteLine();
			Console.WriteLine( "    --quiet:" );
			Console.WriteLine( "        Don't display GUIDS." );
			Console.WriteLine();
			Console.WriteLine( "    --help:" );
			Console.WriteLine( "        Displays this help." );
			Console.WriteLine();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if ( ParseConfigurationParameters( args ) )
			{
				try
				{
					// Initialize the store.
					Store store = Store.GetStore();

					// Get the default domain.
					Domain domain = store.GetDomain( store.DefaultDomain );
					if ( domain != null )
					{
						// Get all of the collections that have been orphaned.
						ICSList orphanList = store.GetCollectionsByProperty( new Property( "OrphanedOwner", "cn" ), SearchOp.Begins );
						if ( orphanList.Count > 0 )
						{
							foreach( ShallowNode sn in orphanList )
							{
								// Convert the ShallowNode to a Collection object.
								Collection collection = new Collection( store, sn );
							
								// Get the orphaned property.
								Property p = collection.Properties.GetSingleProperty( "OrphanedOwner" );
								if ( p != null )
								{
									// Look up this user in the domain.
									ICSList memberList = domain.Search( "DN", p.Value as String, SearchOp.Equal );
									if ( memberList.Count > 0 )
									{
										foreach( ShallowNode msn in memberList )
										{
											// Convert the ShallowNode to a member.
											Member domainMember = new Member( domain, msn );

											// Get the owner of the collection.
											Member ownerMember = collection.Owner;

											// Set the proper rights for this collection.
											Access.Rights rights = collection.IsBaseType( collection, NodeTypes.POBoxType ) ? Access.Rights.ReadWrite : Access.Rights.Admin;

											if ( quiet )
											{
												Console.WriteLine( "Changing collection {0} owner from {1} to {2}.", collection.Name, ownerMember.Name, domainMember.Name );
											}
											else
											{
												Console.WriteLine( "Changing collection {0}:{1} owner from {2}:{3} to {4}:{5}.", collection.Name, collection.ID, ownerMember.Name, ownerMember.UserID, domainMember.Name, domainMember.UserID );
											}

											// Commit the changes.
											if ( commit )
											{
												// Remove the orphaned property.
												collection.Properties.DeleteSingleProperty( "OrphanedOwner" );
												collection.Commit();

												// Add this new member to the collection.
												Member member = new Member( domainMember.Name, domainMember.UserID, rights );
												Node[] nodes = collection.ChangeOwner( member, Access.Rights.Deny );
												collection.Commit( nodes );
											}
										}
									}
									else
									{
										Console.Error.WriteLine( "Error: Could not find user {0} in the domain.", p );
									}
								}
								else
								{
									Console.Error.WriteLine( "Error: Could not find OrphanedOwner property for collection {0}:{1}", collection.Name, collection.ID );
								}
							}
						}
						else
						{
							Console.Error.WriteLine( "No orphaned collections to adopt." );
						}
					}
					else
					{
						Console.Error.WriteLine( "Error: Cannot get the default domain for the store." );
					}
				}
				catch ( Exception ex )
				{
					Console.Error.WriteLine( "Error: Exception {0}.", ex.Message );
					Console.Error.WriteLine( "       Stack Trace: {0}", ex.StackTrace );
				}
			}
		}
	}
}
