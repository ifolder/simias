<%@ Control Language="c#" AutoEventWireup="false" Codebehind="iFolderSearch.ascx.cs" Inherits="Novell.iFolderWeb.Admin.iFolderSearch" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<script language="javascript">

	function SubmitKeyDown(e, b)
	{
		var result = true;
		if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13))
		{
			document.getElementById(b).click();
			result = false;
		} 
		
		return result;
	}

</script>

<div class="searchnav">

	<asp:Label ID="NameLabel" Runat="server" CssClass="searchlistnamelabel" />

	<asp:DropDownList ID="SearchOpList" Runat="server" CssClass="searchlist">
	
		<asp:ListItem></asp:ListItem>
		<asp:ListItem></asp:ListItem>
		<asp:ListItem></asp:ListItem>
		<asp:ListItem></asp:ListItem>
	
	</asp:DropDownList>
	
	<input 
		ID="SearchNameTextBox" 
		Type="text" 
		Runat="server" 
		MaxLength="255" 
		Class="edittext" NAME="SearchName"/>
	
	<asp:Button 
		ID="SearchButton" 
		Runat="server"
		CssClass="ifolderbuttons" />
	
</div>

