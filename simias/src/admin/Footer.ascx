<%@ Control Language="c#" AutoEventWireup="false" Codebehind="Footer.ascx.cs" Inherits="Novell.iFolderWeb.Admin.Footer" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<div class="footer">

	<asp:Label ID="LoggedInAs" Runat="server" CssClass="loggedinas" />
	
	<asp:Label ID="LoggedInName" Runat="server" CssClass="loggedinname" />
	
	<asp:LinkButton 
		ID="LogoutButton" 
		Runat="server" 
		CssClass="logoutbutton" 
		OnClick="OnLogoutButton_Click" /> 
			
</div>
