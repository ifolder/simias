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
using System.Xml;
using System.Text;
using System.Reflection;
using System.Web.Services.Description;

/// <summary>
/// Generate WSDL File
/// </summary>
class GenerateWsdl
{
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static int Main(string[] args)
	{
		int result = 0;

		if (args.Length == 0)
		{
			Console.WriteLine("USAGE: GenerateWsdl.exe [Assembly] [Type] [URL] [File]");
			result = -1;;
		}
		else
		{
			try
			{
				Assembly assembly = Assembly.LoadFrom(args[0]);
				Type type = assembly.GetType(args[1]);

				ServiceDescriptionReflector reflector = new ServiceDescriptionReflector();
				reflector.Reflect(type, args[2]);

				XmlTextWriter writer = new XmlTextWriter(args[3], Encoding.UTF8);
				writer.Formatting = Formatting.Indented;
				reflector.ServiceDescriptions[0].Write(writer);
				writer.Close();
			}
			catch(Exception ex)
			{
				Console.Error.WriteLine(ex);
				result = -1;
			}
		}

		return result;
	}
}
