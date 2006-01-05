// project created on 1/4/2006 at 10:23 AM
using System;

using MdbHandle = System.IntPtr;

class MainClass
{
	public static void Main(string[] args)
	{
		Console.WriteLine( "Starting up" );
		
		Simias.MdbSync.EnumUsers enumUsers = null;
		Simias.MdbSync.Mdb mdb = null;
		Simias.MdbSync.User user = null;
		
		mdb = new Simias.MdbSync.Mdb( "\\Tree\\Context\\admin", "hula" );
		Console.WriteLine( "MDB Handle: " + mdb.Handle.ToString() );
		
		// new up banderso
		user = new Simias.MdbSync.User( mdb.Handle, "\\Tree\\Context\\banderso" );
		
		enumUsers = new Simias.MdbSync.EnumUsers( mdb.Handle, "\\Tree\\Context\\", false );
		enumUsers.Reset();
		while( enumUsers.MoveNext() == true )
		{
			user = enumUsers.Current as Simias.MdbSync.User;
			Console.WriteLine( user );
		}
	}
}