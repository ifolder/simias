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
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 |***************************************************************************/
 
using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.0.0")]
	[assembly: SatelliteContractVersion("1.0.0.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion("1.0.0.0")]
	[assembly: SatelliteContractVersion("1.0.0.0")]
	[assembly: ComCompatibleVersion(1, 0, 0, 0)]
	[assembly: TypeLibVersion(1, 10)]
#endif

[assembly: AssemblyTitle("Novell.AddressBook.dll")]
[assembly: AssemblyDescription("Novell.AddressBook.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("iFolder development team")]
[assembly: AssemblyProduct("iFolder")]
[assembly: AssemblyCopyright("(c) 2004 Novell, Inc.")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(false)]
[assembly: AssemblyDefaultAlias("Novell.AddressBook.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers]
[assembly: ComVisible(false)]

// #if MONO
// [assembly: AssemblyDelaySign(true)]
// [assembly: AssemblyKeyFile("../ifolder-snakeoil.keys")]
// #endif
