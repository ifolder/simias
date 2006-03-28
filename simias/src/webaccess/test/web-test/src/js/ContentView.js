/**
 * $RCSfile: ContentView.js,v $
 * 
 * @fileoverview iFolder.ContentView Class
 */

/**
 * @class A class for the content view.
 */
iFolder.ContentView = Class.create();

/**
 * Construct a new iFolder.ContentView object.
 * @constructor
 */
iFolder.ContentView.prototype.initialize = function(store, parentNode)
{
	this.store = store;
	this.parentNode = parentNode;
};

/**
 * Render the view.
 */
iFolder.ContentView.prototype.render = function()
{
	// events
	Event.observe(this.store, 'startRefresh', this.clear.bindAsEventListener(this));
	Event.observe(this.store, 'finishRefresh', this.update.bindAsEventListener(this));
};

/**
 * Clear the view.
 */
iFolder.ContentView.prototype.clear = function(event)
{
	// TODO
	Element.removeChildren(this.parentNode);

	// loading message
	this.parentNode.appendChild(Builder.node('div', 'loading...'));
};

/**
 * Update the view.
 */
iFolder.ContentView.prototype.update = function(event)
{
	// TODO
	Element.removeChildren(this.parentNode);

	var context = this.store.context;

	// new view
	if (context.isRoot())
	{
		this.currentView = new iFolder.iFoldersView(this.store, this.parentNode);
		this.currentView.render();
	}
	else
	{
		this.currentView = new iFolder.EntriesView(this.store, this.parentNode);
		this.currentView.render();
	}
};

