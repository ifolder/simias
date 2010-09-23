/**
 * Create a multiple-file input control from a single file input control.
 *
 * Based on work by Stickman -- http://www.the-stickman.com 
 */

function bytesToSize(bytes) {
	var sizes = ['bytes', 'kb', 'MB', 'GB', 'TB', 'PB'];
	var e = Math.floor(Math.log(bytes)/Math.log(1024));
	return (bytes/Math.pow(1024, Math.floor(e))).toFixed(2)+" "+sizes[e];
};


function MultiFile(list, max)
{
	this.list = list;
	this.count = 0;
	this.id = 0;
	this.max = -1;
	this.button_text = "remove";
	
	if (max)
	{
		this.max = max;
	};
	
	/**
	 * Add a new file input element
	 */
	this.addElement = function(element)
	{
		if ((element.tagName == 'INPUT') && (element.type == 'file'))
		{
			element.name = 'file_' + this.id++;
			element.multi_file = this;

			// when a file is selected
			element.onchange = function()
			{
				// new file input
				var new_element = document.createElement('input');
				new_element.type = 'file';
				new_element.size = this.size;

				// add
				this.parentNode.insertBefore(new_element, this);
				this.multi_file.addElement(new_element);

				// update list
				this.multi_file.addListRow(this);

				// hide
				// note: we can't use display:none because Safari doesn't like it
				this.style.position = 'absolute';
				this.style.left = '-1000px';
			};

			if ((this.max != -1) && (this.count >= this.max))
			{
				element.disabled = true;
			};

			// current
			this.count++;
			this.current_element = element;
		};
	};

	/**
	 * Add a new row to the list of files
	 */
	this.addListRow = function(element)
	{
		var new_row = document.createElement('div');

		// remove button
		var new_row_button = document.createElement('a');
		new_row_button.href = '#';
		new_row_button.innerHTML = this.button_text;

		// references
		new_row.element = element;

		// remove function
		new_row_button.onclick= function()
		{
			// remove element
			this.parentNode.element.parentNode.removeChild(this.parentNode.element);

			// remove this row from the list
			this.parentNode.parentNode.removeChild(this.parentNode);

			// decrement counter
			this.parentNode.element.multi_file.count--;

			// re-enable input element (if it's disabled)
			this.parentNode.element.multi_file.current_element.disabled = false;

			// appease Safari
			return false;
		};

		// row value
		new_row.innerHTML = element.value + " (" + bytesToSize(element.files[0].fileSize) + ") ";

		// add button
		new_row.appendChild(new_row_button);

		// add
		this.list.appendChild(new_row);
	};
};