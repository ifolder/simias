/**
 * $RCSfile: iFolder.js,v $
 * 
 * @fileoverview iFolder.iFolder Class
 */

/**
 * Client logger.
 */
var log = new Log(Log.FATAL, Log.popupLogger);

/**
 * @class The top-level iFolder class.
 * @namespace The iFolder namespace.
 */
var iFolder = 
{
	/**
	 * iFolder Version
	 */
	Version: "3.5",

	/**
	 * Perform load time (inline) checking and initialization.
	 */
	load: function()
	{
		log.info('iFolder loading...');

		if ((typeof Prototype=='undefined') ||
			parseFloat(Prototype.Version.split('.')[0] + '.' +
				Prototype.Version.split('.')[1]) < 1.4)
		{
			throw('iFolder requires the Prototype JavaScript framework version 1.4 or higher.');
		}

		if ((typeof Scriptaculous=='undefined') ||
			parseFloat(Scriptaculous.Version.split('.')[0] + '.' +
                       Scriptaculous.Version.split('.')[1]) < 1.5)
		{
			throw('iFolder requires the Scriptaculous JavaScript framework version 1.5 or higher.');
		}

		Event.observe(window, 'load', this.init.bindAsEventListener(this), false);
	},

	/**
	 * Perform post-load time checking and initialization.
	 * @param {Event} event The triggering event.
	 */
	init: function(event) 
	{
		// store
		this.store = new iFolder.Store(window.location);

		// bad connection
		if (this.store.sessionInfo == null)
		{
			window.location = 'Login.aspx?Action=Refresh';
			return;
		}

		// view
		this.view = new iFolder.MainView(this.store);
		this.view.render();

		// refresh
		this.store.refresh();
	}
};

// inline initialization
iFolder.load();

