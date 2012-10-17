using System;
using System.IO;
using System.Text;
using System.Xml;


using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.LdapProvider;
using Simias.Storage.Provider;

public class EnableUserLogin 
{
	private static string SimiasDataPath = "/var/simias/data/simias/";
		
	static string disabledAtProperty = "IdentitySynchronization:DisabledAt";
	static int Main( string[] args)
	{
		bool replace = ( args.Length >= 1 && args[0] == "enable") ? true : false;
		EnableUserLogins( replace );	
		Console.WriteLine("Successfully changed !!!");
		return 0;
	}

        // open store and enable the login for all users. It will also delete 'DisabledAt' property for all users, if it was set earlier.
	//DisabledAt property means during e-dir sync, somehow server interpreted that user was deleted in e-dir, so disabled in iFolder and currently
	// it is under grace itnerval period. After grace interval, server will completely delete that user and orphan his iFolders.
        public static void EnableUserLogins( bool replace)
        {
		if( replace == false ) Console.WriteLine("You have opted to view and not delete..Use enable to modify also "); else Console.WriteLine("It will modify the members");	
                Store.Initialize(SimiasDataPath, true , -1);
                Store store = Store.GetStore();
                if( store == null)
                        Console.WriteLine("store could not be initialized.....");
                Domain domain = store.GetDomain(store.DefaultDomain);
		try{
			ICSList FullMemList = domain.GetMemberList();
			foreach (ShallowNode sn in FullMemList)
			{
				Member memObject = new Member(domain, sn);
				Simias.Storage.Property disabledAt = memObject.Properties.GetSingleProperty( disabledAtProperty );
				if( disabledAt != null )
				{
					if( replace)
						memObject.DeleteProperty = disabledAtProperty;

					if( domain.GetLoginpolicy( memObject.UserID ) == true )   //it returns true if user is disabled
					{
					// if login was disabled, then enable login
						if( replace == true )
							domain.SetLoginDisabled( memObject.UserID, false ); //false means it will delete the logindisabled property
						Console.WriteLine("Enabled the login for user :"+memObject.Name);
					}
					else Console.WriteLine("Login already enabled for user :"+memObject.Name);
					if(replace)
						domain.Commit( memObject );
				Console.WriteLine("Removed the disabledAt property for user :"+memObject.Name);
				}
			}
		 }
                catch(Exception ee)
                {
                        Console.WriteLine("Got exception: "+ee.Message);
                }
                finally{
                        Store.DeleteInstance();
                }

        }
}
