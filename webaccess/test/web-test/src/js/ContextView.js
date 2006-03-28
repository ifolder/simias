/**
 * $RCSfile: ContextView.js,v $
 * 
 * @fileoverview iFolder.ContextView Class
 */

/**
 * @class A class for the controls view.
 */
iFolder.ContextView = Class.create();

/**
 * Construct a new iFolder.ContextView object.
 * @constructor
 */
iFolder.ContextView.prototype.initialize = function(store, parentNode)
{
	this.store = store;
	this.parentNode = parentNode;
};

/**
 * Render the view.
 */
iFolder.ContextView.prototype.render = function()
{
	// events
	Event.observe(this.store, 'startRefresh', this.update.bindAsEventListener(this));
};

/**
 * Update the view.
 */
iFolder.ContextView.prototype.update = function(event)
{
	// TODO
	Element.removeChildren(this.parentNode);

	this.rootNode = Builder.node('div', { className: 'list'});

	var context = this.store.context;
	var ifodlerId = context.ifodlerId;
	var path = context.path;
	var itemName, itemContext;
	
	if (context.isRoot())
	{
		itemName = 'iFolders';
		
		if (context.page == iFolder.Context.Page_NEW)
		{
			this.renderItem(itemName, new iFolder.Context());
			this.renderItem('New iFolder', context, true);
		}
		else
		{
			this.renderItem(itemName, context, true);
		}
	}
	else
	{
		this.renderItem('iFolders', new iFolder.Context());
	
		var start = 0, end = 0;
		
		while (end != -1)
		{
			end = path.indexOf('/', start);
	
            if (end != -1)
			{
				itemName = path.substring(start, end);
				itemContext = new iFolder.Context(ifodlerId, path.substring(0, end));
				this.renderItem(itemName, itemContext);
			}
			else
			{
				itemName = path.substring(start);

				if (context.page == iFolder.Context.PAGE_DETAILS)
				{
					itemName = itemName + ' Details';
				}
				else if (context.page == iFolder.Context.Page_NEW)
				{
					itemName = itemName + ' New Folder';
				}
				
				this.renderItem(itemName, context, true);
			}
	
			start = end + 1;
		}
	}

	// append
	this.parentNode.appendChild(this.rootNode);
};


/**
 * Render an item.
 */
iFolder.ContextView.prototype.renderItem = function(name, context, current)
{
	if (this.rootNode.firstChild)
	{
		this.rootNode.appendChild(Builder.node('span', { className: 'sep' }, ' > ' ));
	}
	
	var node = Builder.node('span', { className: 'item' }, name );
	node.context = context;

	if (!current)
	{
		Element.addClassName(node, 'action');
		Event.observe(node, 'click', this.click.bindAsEventListener(this), false);
	}

	this.rootNode.appendChild(node);
}

iFolder.ContextView.prototype.click = function(event)
{
	var item = Event.element(event);
	this.store.setContext(item.context);
};
