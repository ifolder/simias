/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

#import <Cocoa/Cocoa.h>


@class LoginWindowController;
@class iFolder;
@class iFolderData;
@class SMFileSyncEvent;
@class SMCollectionSyncEvent;
@class SMNodeEvent;


@interface iFolderApplication : NSObject
{
	LoginWindowController	*loginWindowController;
	iFolderData				*ifolderdata;
	BOOL					runThreads;
}

//==========================================
// IBAction Methods
//==========================================
- (IBAction)showSyncLog:(id)sender;
- (IBAction)showAboutBox:(id)sender;
- (IBAction)showPrefs:(id)sender;
- (IBAction)showiFolderWindow:(id)sender;



//==========================================
// All other methods
//==========================================
- (void)showLoginWindow:(NSString *)domainID;
- (void)addLog:(NSString *)entry;
- (void)initializeSimiasEvents;
- (BOOL)authenticateToDomain:(NSString *)domainID withPassword:(NSString *)password;



//==========================================
// Thread Safe calls
//==========================================
- (void)addLogTS:(NSString *)entry;
- (void)showLoginWindowTS:(NSString *)domainID;


//==========================================
// NSApplication Delegates
//==========================================
- (void)applicationDidFinishLaunching:(NSNotification*)notification;
- (void)applicationWillTerminate:(NSNotification *)notification;


//==========================================
// Simias startup and shutdown methods
//==========================================
- (void)startSimiasThread:(id)arg;


//==========================================
// Simias Event thread methods
//==========================================
- (void)enableThreads:(id)arg;
- (void)simiasEventThread:(id)arg;
- (void)processNotifyEvents;
- (void)processFileSyncEvents;
- (void)handleFileSyncEvent:(SMFileSyncEvent *)fileSyncEvent;
- (void)processCollectionSyncEvents;
- (void)handleCollectionSyncEvent:(SMCollectionSyncEvent *)colSyncEvent;
- (void)processNodeEvents;
- (void)handleNodeEvent:(SMNodeEvent *)nodeEvent;


@end
