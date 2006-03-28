/**
 * $RCSfile: Utilities.js,v $
 * 
 * @fileoverview iFolder Class
 */

/**
 * @class Extend the Element class.
 * @extends Element
 */
Object.extend(Element,
{
	/**
	 * Removes all the children of the  given node.
	 * @param {Node} node The node object.
	 */
	removeChildren: function(node)
	{
		while (node.firstChild) node.removeChild(node.firstChild);
	}
});

/**
 * @class Extend the Event class.
 * @extends Event
 */
Object.extend(Event,
{
	/**
	 * Like Event.stop, but allows the default actions
	 * @param {Event} event The event object.
	 */
	stopBubble: function(event)
	{
		if (event.stopPropagation)
		{
			event.stopPropagation();
		}
		else
		{
			event.cancelBubble = true;
		}
	}
});

/**
 * @class Extend the Date class.
 * @extends Date
 */
Object.extend(Date,
{
	MONTHS: [ 'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec' ],

	/**
	 * Create a friendly date string.
	 */
	toFriendlyLocaleDateString: function(date)
	{
		var result;
		
		var today = new Date();

		if ((date.getDate() == today.getDate())
			&& (date.getMonth() == today.getMonth())
			&& (date.getFullYear() == today.getFullYear()))
		{
			result = 'today';
		}
		else if (date.getYear() == today.getYear())
		{
			result = date.getDate() + ' ' + this.MONTHS[date.getMonth()];
		}
		else
		{
			result = date.getDate() + ' ' + this.MONTHS[date.getMonth()] + ' ' + date.getFullYear();
		}

		return result;
	}
});
