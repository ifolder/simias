/**
 * $RCSfile: ControlsView.js,v $
 * 
 * @fileoverview iFolder.ControlsView Class
 */

/**
 * @class A class for the controls view.
 */
iFolder.ControlsView = Class.create();

/**
 * Construct a new iFolder.ControlsView object.
 * @constructor
 */
iFolder.ControlsView.prototype.initialize = function(store, parentNode)
{
	this.store = store;
	this.parentNode = parentNode;
};

/**
 * Render the view.
 */
iFolder.ControlsView.prototype.render = function()
{
	this.rootNode = Builder.node('div', { id: 'controls' },
	[
		Builder.node('span', { className: 'info' }, 'Logged in as ' + this.store.sessionInfo.fullName),
		Builder.node('span', { className: 'sep' }, '|'),
		Builder.node('span', { id: 'logout', className: 'action' }, 'Logout')
	]);

	// append
	this.parentNode.appendChild(this.rootNode);

	// events
	Event.observe($('logout'), 'click', this.logout.bindAsEventListener(this));
};

/**
 * Logout.
 */
iFolder.ControlsView.prototype.logout = function()
{
	window.location = 'Login.aspx?Action=Logout';
}
