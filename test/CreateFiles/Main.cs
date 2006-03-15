// project created on 3/15/2006 at 9:15 AM
using System;
using System.IO;


class MainClass
{
	private int maxSize = 4096;
	private int numberFiles = 1;
	private string directory = null;
	private bool random = false;
	private bool verbose = false;
	private bool showHelp = false;
	
	private void ParseCommandLine( string[] args )
	{
		for( int i = 0; i < args.Length; i++ )
		{
			switch( args[ i ].ToLower() )
			{
			
				case "--filesize":
				{
					if ( i < args.Length )
					{
						maxSize = Convert.ToInt32( args[ ++i ] );
					}
					break;
				}
				
				case "--number":
				{
					if ( i < args.Length )
					{
						numberFiles = Convert.ToInt32( args[ ++i ] );
					}
					break;
				}
				
				case "--dir":
				{
					if ( i < args.Length )
					{
						if ( args[ i + 1 ] != "" && args[ i + 1 ].StartsWith( "--" ) == false )
						{
							directory = args[ ++i ];
						}
					}
					break;
				}
				
				case "--random":
				{
					this.random = true;
					break;
				}
				
				case "--help":
				{
					this.showHelp = true;
					break;
				}
			}
		}
	}
	
	private void ShowUseage()
	{
		Console.WriteLine( "Usage: CreateFiles --number <number of files to create> --dir <directory> --filesize <size> --random" );
		Console.WriteLine( "  --dir  destination directory where to create files" );
		Console.WriteLine( "  --filesize   destination directory where to create files" );
		Console.WriteLine( "  --random file size up to and including filesize" );
	}
	
	public MainClass()
	{
		directory = Environment.CurrentDirectory;
	}
	
	public static void Main(string[] args)
	{
		string fullPathAndFile;
		int fileSize;
		
		MainClass main = new MainClass();
		
		if ( args.Length > 0 )
		{
			main.ParseCommandLine( args );
			
			if ( main.showHelp == true )
			{
				main.ShowUseage();
				return;
			}
		}
		
		Random randomSize = new Random( DateTime.Now.Millisecond );
		
		Console.WriteLine( "Creating {0} file(s)" );
		
		for ( int i = 0; i < main.numberFiles; i++ )
		{
			fullPathAndFile = main.directory + Path.DirectorySeparatorChar.ToString() + Guid.NewGuid();
			
			if ( main.random == false )
			{
				fileSize = main.maxSize;
			}
			else
			{
				fileSize = randomSize.Next();
				while( fileSize > main.maxSize )
				{
					fileSize = randomSize.Next();
				}
			}
			
			if ( main.verbose == true )
			{
				Console.WriteLine( "creating: " + fullPathAndFile );
				Console.WriteLine( "file size: " + fileSize );
			}
			
			char[] buffer = new char[ fileSize ];
			System.IO.StreamWriter sw = File.CreateText( fullPathAndFile );
			sw.Write( buffer );
			sw.Flush();
			sw.Close();
			buffer = null;
		}
	}
}