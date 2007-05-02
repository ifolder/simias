<%@ Control Language="c#" AutoEventWireup="false" Codebehind="MemberSearch.ascx.cs" Inherits="Novell.iFolderWeb.Admin.MemberSearch" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

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

	<asp:DropDownList ID="SearchAttributeList" Runat="server" CssClass="searchlist">

		<asp:ListItem></asp:ListItem>
		<asp:ListItem></asp:ListItem>
		<asp:ListItem></asp:ListItem>

	</asp:DropDownList>
	
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
		Class="edittext" />
	
	<asp:Button 
		ID="SearchButton" 
		Runat="server"
		CssClass="ifolderbuttons" />
	
</div>

