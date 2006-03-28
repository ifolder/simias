/**
 * $RCSfile: EntriesView.js,v $
 * 
 * @fileoverview iFolder.EntriesView Class
 */

/**
 * @class A class for the entries view.
 */
iFolder.EntriesView = Class.create();

/**
 * Construct a new iFolder.EntriesView object.
 * @constructor
 */
iFolder.EntriesView.prototype.initialize = function(store, parentNode)
{
	this.store = store;
	this.parentNode = parentNode;
};

/**
 * Render the view.
 */
iFolder.EntriesView.prototype.render = function()
{
	this.rootNode = Builder.node('div', { id: 'entries' },
	[
		Builder.node('table', { className: 'entries' },
		[
			Builder.node('colgroup',
			[
				Builder.node('col', { className: 'cb' }),
				Builder.node('col', { className: 'icon' }),
				Builder.node('col', { className: 'name' }),
				Builder.node('col', { className: 'size' }),
				Builder.node('col', { className: 'details' })
			]),
			Builder.node('tbody')
		])
	]);

	// tbody tag from above
	var tbody = this.rootNode.firstChild.lastChild;

	// add entries
	var entries = this.store.data;

	for (var i in entries)
	{
		if (entries[i].type)
		{
			this.renderEntry(tbody, entries[i]);
		}
	}

	// append
	this.parentNode.appendChild(this.rootNode);
};

/**
 * Render the entry.
 */
iFolder.EntriesView.prototype.renderEntry = function(tbody, entry)
{
	var row = Builder.node('tr',
	[
		Builder.node('td',
		[
			Builder.node('input', { type: 'checkbox' })
		]),
		Builder.node('td', { className: entry.type }),
		Builder.node('td', entry.name),
		Builder.node('td', entry.size),
		Builder.node('td', { className: 'details' })
	]);

	// TODO: find better Safari solution
	row.childNodes[1].innerHTML = '&nbsp;';
	row.childNodes[4].innerHTML = '&nbsp;';
	
	// save context on row
	row.context = new iFolder.Context(entry.ifolderId, entry.path);
	
	tbody.appendChild(row);

	// action click
	Event.observe(row, 'click', this.action.bindAsEventListener(this));

	// selection click
	Event.observe(row.firstChild, 'click', this.selection.bindAsEventListener(this));

	// details click
	Event.observe(row.lastChild, 'click', this.details.bindAsEventListener(this));
};


/**
 * Action click on an entry.
 */
iFolder.EntriesView.prototype.action = function(event)
{
	var row = Event.findElement(event, 'tr');
	this.store.setContext(row.context);
};

/**
 * Selection click on an entry.
 */
iFolder.EntriesView.prototype.selection = function(event)
{
	Event.stopBubble(event);
};

/**
 * Details click on an entry.
 */
iFolder.EntriesView.prototype.details = function(event)
{
	var row = Event.findElement(event, 'tr');
	var context = row.context.clone();
	context.page = iFolder.Context.PAGE_DETAILS;
	this.store.setContext(context);

	Event.stop(event);
};

