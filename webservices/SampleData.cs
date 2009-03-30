/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL, 
* PROPRIETARY, AND TRADE SECRET INFORMATION.	NOVELL RESTRICTS THIS WORK TO 
* NOVELL EMPLOYEES WHO NEED THE WORK TO PERFORM THEIR ASSIGNMENTS AND TO 
* THIRD PARTIES AUTHORIZED BY NOVELL IN WRITING.  THIS WORK MAY NOT BE USED, 
* COPIED, DISTRIBUTED, DISCLOSED, ADAPTED, PERFORMED, DISPLAYED, COLLECTED,
* COMPILED, OR LINKED WITHOUT NOVELL'S PRIOR WRITTEN CONSENT.  USE OR 
* EXPLOITATION OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE 
* PERPETRATOR TO CRIMINAL AND  CIVIL LIABILITY.
*
* Novell is the copyright owner of this file.  Novell may have released an earlier version of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
*
*-----------------------------------------------------------------------------
*
*                 $Author: Rob
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
using System.Collections;
using System.Threading;

using Simias.Storage;
using Simias.Server;

namespace iFolder.WebService
{
	/// <summary>
	/// Sample Data Generator
	/// </summary>
	public class SampleData
	{
		private static string[] firstNames =
		{
			"James", "John", "Robert", "Michael", "William",
			"David", "Richard", "Charles", "Joseph", "Thomas",
			"Christopher", "Daniel", "Paul", "Mark", "Donald",
			"George", "Kenneth", "Steven", "Edward", "Brian",
			"Ronald", "Anthony", "Kevin", "Jason", "Matthew",
			"Gary", "Timothy", "Jose", "Larry", "Jeffrey",
			"Frank", "Scott", "Eric", "Stephen", "Andrew",
			"Raymond", "Gregory", "Joshua", "Jerry", "Dennis",
			"Walter", "Patrick", "Peter", "Harold", "Douglas",
			"Henry", "Carl", "Arthur", "Ryan", "Roger",
			"Mary", "Patricia", "Linda", "Barbara", "Elizabeth",
			"Jennifer", "Maria", "Susan", "Margaret", "Dorothy",
			"Lisa", "Nancy", "Karen", "Betty", "Helen",
			"Sandra", "Donna", "Carol", "Ruth", "Sharon",
			"Michelle", "Laura", "Sarah", "Kimberly", "Deborah",
			"Jessica", "Shirley", "Cynthia", "Angela", "Melissa",
			"Brenda", "Amy", "Anna", "Rebecca", "Virginia",
			"Kathleen", "Pamela", "Martha", "Debra", "Amanda",
			"Stephanie", "Carolyn", "Christine", "Marie", "Janet",
			"Catherine", "Frances", "Ann", "Joyce", "Diane"
		};

		private static string[] lastNames = 
		{
			"Smith", "Johnson", "Williams", "Jones", "Brown",
			"Davis", "Miller", "Wilson", "Moore", "Taylor",
			"Anderson", "Thomas", "Jackson", "White", "Harris",
			"Martin", "Thompson", "Garcia", "Martinez", "Robinson",
			"Clark", "Rodriguez", "Lewis", "Lee", "Walker",
			"Hall", "Allen", "Young", "Hernandez", "King",
			"Wright", "Lopez", "Hill", "Scott", "Green",
			"Adams", "Baker", "Gonzalez", "Nelson", "Carter",
			"Mitchell", "Perez", "Roberts", "Turner", "Phillips",
			"Campbell", "Parker", "Evans", "Edwards", "Collins",
			"Stewart", "Sanchez", "Morris", "Rogers", "Reed",
			"Cook", "Morgan", "Bell", "Murphy", "Bailey",
			"Rivera", "Cooper", "Richardson", "Cox", "Howard",
			"Ward", "Torres", "Peterson", "Gray", "Ramirez",
			"James", "Watson", "Brooks", "Kelly", "Sanders",
			"Price", "Bennett", "Wood", "Barnes", "Ross",
			"Henderson", "Coleman", "Jenkins", "Perry", "Powell",
			"Long", "Patterson", "Hughes", "Flores", "Washington",
			"Butler", "Simmons", "Foster", "Gonzales", "Bryant",
			"Alexander", "Russell", "Griffin", "Diaz", "Hayes"
		};

		private static string[] ifolderNames =
		{
			"Montgomery", "Juneau", "Phoenix", "Little Rock", "Sacramento",
			"Denver", "Hartford", "Dover", "Tallahassee", "Atlanta",
			"Honolulu", "Boise", "Springfield", "Indianapolis", "Des Moines",
			"Topeka", "Frankfort", "Baton Rouge", "Augusta", "Annapolis",
			"Boston", "Lansing", "St. Paul", "Jackson", "Jefferson City",
			"Helena", "Lincoln", "Carson City", "Concord", "Trenton",
			"Santa Fe", "Albany", "Raleigh", "Bismarck", "Columbus",
			"Oklahoma City", "Salem", "Harrisburg", "Providence", "Columbia",
			"Pierre", "Nashville", "Austin", "Salt Lake City", "Montpelier",
			"Richmond", "Olympia", "Charleston", "Madison", "Cheyenne"
		};

		private SampleData()
		{
		}

		/// <summary>
		/// Generate Sample Data
		/// </summary>
		public static void Generate()
		{
			// support only non-external user providers
			IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
			UserProviderCaps caps = provider.GetCapabilities();
			if (caps.ExternalSync) throw new ApplicationException("External User Providers Are Not Supported");
			if (!caps.CanCreate) throw new ApplicationException("Provider Does Not Support User Creation");

			// spin thread
			Thread t = new Thread(new ThreadStart(Start));
			t.IsBackground = true;
			t.Start();
		}

		/// <summary>
		/// Start Generating Data
		/// </summary>
		public static void Start()
		{
			// users
			ArrayList users = CreateUsers();

			// ifolders
			ArrayList ifolders = CreateiFolders(users);
		}

		/// <summary>
		/// Create iFolders
		/// </summary>
		/// <param name="users"></param>
		/// <returns></returns>
		private static ArrayList CreateiFolders(ArrayList users)
		{
			ArrayList ifolders = new ArrayList();

			// personal ifolders
			foreach(string user in users)
			{
				string id = CreateiFolder("Personal", user);
				if (id != null) ifolders.Add(id);
			}

			// shared ifolders
			ArrayList temp = (ArrayList)users.Clone();
			temp.Sort();
			Stack stack = new Stack(temp);

			int count = (users.Count / ifolderNames.Length) - 2;

			foreach(string name in ifolderNames)
			{
				string id = CreateiFolder(name, (string)stack.Pop());
				if (id != null) ifolders.Add(id);

				for(int i=0; i < count; i++)
				{
					ShareiFolder(id, (string)stack.Pop());
				}
			}

			return ifolders;
		}

		/// <summary>
		/// Create iFolder
		/// </summary>
		/// <param name="name"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		private static string CreateiFolder(string name, string owner)
		{
			string id = null;

			try
			{
				//iFolder ifolder = iFolder.CreateiFolder(name, owner, name, owner);
				//id = ifolder.ID;
			}
			catch
			{
				// ignore
			}

			return id;
		}
		
		/// <summary>
		/// Share iFolder
		/// </summary>
		/// <param name="id"></param>
		/// <param name="member"></param>
		private static void ShareiFolder(string id, string member)
		{
			try
			{
				iFolderUser.AddMember(id, member, Rights.ReadOnly, null);
			}
			catch
			{
				// ignore
			}
		}
		
		/// <summary>
		/// Create Users
		/// </summary>
		private static ArrayList CreateUsers()
		{
			ArrayList users = new ArrayList();
			int i = 0;
			string id;

			foreach(string last in lastNames)
			{
				foreach(string first in firstNames)
				{
					string username = String.Format("{0}{1}{2}", Char.ToLower(first[0]), last.ToLower(), ++i);
					id = CreateUser(username, first, last);

					if (id != null) users.Add(id);
				}
			}

			// default user
			id = CreateUser("jdoe", "John", "Doe");
			if (id != null) users.Add(id);

			return users;
		}

		/// <summary>
		/// Create User
		/// </summary>
		/// <param name="username"></param>
		/// <param name="first"></param>
		/// <param name="last"></param>
		/// <returns></returns>
		private static string CreateUser(string username, string first, string last)
		{
			string id = null;

			try
			{
				Simias.Server.User user = new Simias.Server.User(username);
				user.FirstName = first;
				user.LastName = last;
				user.FullName = String.Format("{0} {1}", first, last);
			
				Simias.Server.RegistrationInfo info = user.Create("password");
				id = info.UserGuid;
			}
			catch
			{
				// ignore
			}

			return id;
		}
	}
}
