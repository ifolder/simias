/**
 * $RCSfile: Context.js,v $
 * 
 * @fileoverview iFolder.Context Class
 */

/**
 * @class A class that represents an iFolder context.
 */
iFolder.Context = Class.create();


/**
 * Separators
 */
iFolder.Context.IFOLDERID_SEP = ':';
iFolder.Context.PAGE_SEP = '|';

/**
 * Pages
 */
iFolder.Context.PAGE_LIST = '';
iFolder.Context.PAGE_DETAILS = 'details';
iFolder.Context.PAGE_NEW = 'new';

/**
 * Parse text to create a context.
 */
iFolder.Context.parse = function(text)
{
	var ifolderId = '';
	var path = '';
	var page = '';
	
	if (text && (text.length > 0))
	{
		var temp = text;
		var parts = null;
		
		// split off the ifolder id
		parts = temp.split(iFolder.Context.IFOLDERID_SEP, 2);

		if (parts.length > 1)
		{
			ifodlerId = parts[0];
			temp = parts[1];
		}

		// split off the page
		parts = temp.split('|', 2);

		if (parts.length > 1)
		{
			temp = parts[0];
			page = parts[1];
		}

		// save path
		path = temp;
	}

	return new iFolder.Context(iFolder.Context.PAGE_SEP, path, page);
};

/**
 * Construct a new iFolder.Context object.
 * @constructor
 */
iFolder.Context.prototype.initialize = function(ifodlerId, path, page)
{
	this.ifodlerId = ifodlerId ? ifodlerId : '';
	this.path = path ? path : '';
	this.page = page ? page : '';
}

/**
 * Create a duplicate context.
 */
iFolder.Context.prototype.clone = function()
{
	return new iFolder.Context(this.ifodlerId, this.path, this.page);
}

/**
 * Is this the root context?
 */
iFolder.Context.prototype.isRoot = function()
{
	return (this.path.length == 0);
}

/**
 * Is this the context point to the root of an iFolder?
 */
iFolder.Context.prototype.isiFolder = function()
{
	return (this.path.indexOf('/') == -1);
}

/**
 * Return a string representation of the context.
 */
iFolder.Context.prototype.toString = function()
{
	var result = this.path;

	// ifodlerId?
	if (this.ifodlerId.length > 0)
	{
		result = this.ifodlerId + ':' + result;
	}

	// page?
	if (this.page.length > 0)
	{
		result = result + '|' + this.page;
	}

	return result;
};

