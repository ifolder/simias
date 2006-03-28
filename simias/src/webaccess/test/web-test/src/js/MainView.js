/**
 * $RCSfile: MainView.js,v $
 * 
 * @fileoverview iFolder.MainView Class
 */

/**
 * @class A class for the main view.
 */
iFolder.MainView = Class.create();

/**
 * Construct a new iFolder.MainView object.
 * @constructor
 */
iFolder.MainView.prototype.initialize = function(store)
{
	this.store = store;
	this.parentNode = document.getElementsByTagName('body')[0];
};

/**
 * Render the view.
 */
iFolder.MainView.prototype.render = function()
{
	this.rootNode = Builder.node('div', { id: 'container' },
	[
		Builder.node('div', { id: 'header' }),
		Builder.node('div', { id: 'view' },
		[
			Builder.node('div', { id: 'context' }),
			Builder.node('div', { id: 'content' })
		]),
		Builder.node('div', { id: 'nav' })
	]);

	// append
	this.parentNode.appendChild(this.rootNode);

	// views
	this.controlsView = new iFolder.ControlsView(this.store, $('header'));
	this.controlsView.render();
	this.actionsView = new iFolder.ActionsView(this.store, $('nav'));
	this.actionsView.render();
	this.contextView = new iFolder.ContextView(this.store, $('context'));
	this.contextView.render();
	this.contentView = new iFolder.ContentView(this.store, $('content'));
	this.contentView.render();
};

