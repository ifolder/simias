#import "iFolderPrefsController.h"

@implementation iFolderPrefsController

- (void)awakeFromNib
{
	[self setShouldCascadeWindows:NO];
	[self setWindowFrameAutosaveName:@"iFolder Preferences"];
	
	[[self window] setContentSize:[generalView frame].size];
	[[self window] setContentView: generalView];
	[[self window] setTitle:@"iFolder Preferences: General"];
	
	[self setupToolbar];

	[toolbar setSelectedItemIdentifier:@"General"];

	// Setup the controls
}



- (void) updateSize:(NSSize)newSize
{
	NSRect oldFrameRect = [[self window] frame];
	NSRect oldViewRect =  [[[self window] contentView] frame];

	int toolbarSize = oldFrameRect.size.height - oldViewRect.size.height;
	int newY = oldFrameRect.origin.y + oldFrameRect.size.height - newSize.height - toolbarSize;

	[[self window] setFrame: NSMakeRect(oldFrameRect.origin.x, newY, newSize.width, newSize.height + toolbarSize) 
			display: YES animate: YES];
}


// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag
{
	return[toolbarItemDict objectForKey:itemIdentifier];
}


- (int)count
{
	return [toolbarItemArray count];
}



- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemArray;
}




- (NSArray *)toolbarSelectableItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemArray;
}





- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemArray;
}




- (void)generalPreferences:(NSToolbarItem *)item
{
	if([[self window] contentView] != generalView)
	{
		NSLog(@"Switching view to general Page");
		[[self window] setContentView: blankView];
		[self updateSize:[generalView frame].size];
		[[self window] setContentView: generalView];
		[[self window] setTitle:@"iFolder Preferences: General"];	
	}
}




- (void)accountPreferences:(NSToolbarItem *)item
{
	if([[self window] contentView] != accountsView)
	{
		NSLog(@"Switching view to accounts Page");
		[[self window] setContentView: blankView];
		[self updateSize:[accountsView frame].size];
		[[self window] setContentView: accountsView];
		[[self window] setTitle:@"iFolder Preferences: Accounts"];	
	}
}



- (void)setupToolbar
{
	toolbarItemDict = [[NSMutableDictionary alloc] init];
	toolbarItemArray = [[NSMutableArray alloc] init];

	NSToolbarItem *item=[[NSToolbarItem alloc] initWithItemIdentifier:@"General"];
	[item setPaletteLabel:@"General"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"General"]; // name for the item in the toolbar
	[item setToolTip:@"General Settings"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(generalPreferences:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItemDict setObject:item forKey:@"General"]; // add to toolbar list
	[toolbarItemArray addObject:@"General"];
	[item release];	

	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"Accounts"];
	[item setPaletteLabel:@"Accounts"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Accounts"]; // name for the item in the toolbar
	[item setToolTip:@"Accounts"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(accountPreferences:)];
	[item setImage:[NSImage imageNamed:@"prefs-accounts"]];
    [toolbarItemDict setObject:item forKey:@"Accounts"]; // add to toolbar list
	[toolbarItemArray addObject:@"Accounts"];
	[item release];
	

	toolbar = [[NSToolbar alloc] initWithIdentifier:@"iFolderPrefsToolbar"];
	[toolbar setDelegate:self];
	[toolbar setAllowsUserCustomization:NO];
	[toolbar setAutosavesConfiguration:NO];
	[[self window] setToolbar:toolbar];
}

@end
