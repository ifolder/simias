/**
 * $RCSfile: Store.js,v $
 * 
 * @fileoverview iFolder.Store Class
 */

/**
 * @class A class to store the iFolder model.
 * @extends iFolder.EventTarget
 */
iFolder.Store = Class.create();

Object.extend(iFolder.Store.prototype, iFolder.EventTarget.prototype);

/**
 * Construct a new iFolder.Store object.
 * @constructor
 */
iFolder.Store.prototype.initialize = function(location)
{
	this.context = new iFolder.Context('');
	this.sessionInfo = null;
	
	try
	{
		var service = new iFolderJsonService();
		var result = service.getSessionInfo();
		this.sessionInfo = result.rows[0];
	}
	catch (e)
	{
		// ignore
	}
};

/**
 * Initiate a refresh of the iFolder.Store.
 */
iFolder.Store.prototype.refresh = function()
{
	this.dispatchEvent('startRefresh');

	log.info('iFolder store updating...');
	
	var service = new iFolderJsonService();

	switch (this.context.page)
	{
		case iFolder.Context.PAGE_DETAILS:
			if (this.context.isiFolder())
			{
				service.getiFolderDetails(this.context.ifodlerId, this._refresh_callback.bind(this));
			}
			else
			{
				service.getEntryDetails(this.context.ifodlerId, this.context.path, this._refresh_callback.bind(this));
			}
			break;

		case iFolder.Context.PAGE_NEW:
			if (context.isiFolder())
			{
			}
			else
			{
			}
   			break;
	
		case iFolder.Context.PAGE_LIST:
		default:
			if (this.context.isRoot())
			{
				service.getiFolders(this._refresh_callback.bind(this));
			}
			else
			{
				service.getEntries(this.context.ifodlerId, this.context.path, this._refresh_callback.bind(this));
			}
			break;
	}
};

/**
 * Callback from a request to refresh the iFolder.Store.
 */
iFolder.Store.prototype._refresh_callback = function(response)
{
	if (response.result && response.result.rows)
	{
		this.data = response.result.rows;
	}
	else
	{
		this.data = new Array();
	}

	log.info( 'iFolder store refreshed with ' + this.data.length + ' entries' );

	this.dispatchEvent('finishRefresh');
};

/**
 * Create a new iFolder.
 */
iFolder.Store.prototype.createiFolder = function(name, description)
{
	var service = new iFolderJsonService();

	service.createiFolder(name, description, this._createiFolder_callback.bind(this));
};

/**
 * Callback from a request to create a new iFolder.
 */
iFolder.Store.prototype._createiFolder_callback = function(response)
{
	this.refresh();
}

/**
 * Create a new folder.
 */
iFolder.Store.prototype.createFolder = function(name)
{
	var service = new iFolderJsonService();

	service.createFolder(this.context, name, this._createFolder_callback.bind(this));
};

/**
 * Callback from a request to create a new folder.
 */
iFolder.Store.prototype._createFolder_callback = function(response)
{
	this.refresh();
}

/**
 * Set the current context of the iFolderStore.
 * @param context The new context.
 */
iFolder.Store.prototype.setContext = function(context)
{
	this.context = context;

	// TODO: ?
	//window.location.hash = this.context;
	
	this.refresh();
};

