/**
 * $RCSfile: EventTarget.js,v $
 * 
 * @fileoverview ObjectEventTarget Class
 */

/**
 * @class A base class for JavaScript objects to become event targets.
 */
iFolder.EventTarget = Class.create();

/**
 * Add an event listener.
 */
iFolder.EventTarget.prototype.addEventListener = function(type, listener)
{
	if (!this.listeners)
	{
		this.listeners = new Array();
	}
	
	if (!this.listeners[type])
	{
		this.listeners[type] = new Array();
	}

	this.listeners[type].push(listener);
};


/**
 * Remove an event listener.
 */
iFolder.EventTarget.prototype.removeEventListener = function(type, listener)
{
	if (this.listeners[type])
	{
		for (var i = 0; i < this.listeners[type].length; i++)
		{
			if (this.listeners[type][i] == listener)
			{
				this.listeners[type][i] = null;
			}
		}
	}
};

/**
 * Dispatch an event.
 */
iFolder.EventTarget.prototype.dispatchEvent = function(type, data)
{
	if (this.listeners[type])
	{
		for (var i = 0; i < this.listeners[type].length; i++)
		{
			this.listeners[type][i].call(this, data);
		}
	}
};
