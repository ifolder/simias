<%@ Control Language="c#" AutoEventWireup="false" Codebehind="Footer.ascx.cs" Inherits="Novell.iFolderWeb.Admin.Footer" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<div class="footer">

	<asp:Label ID="LoggedInAs" Runat="server" CssClass="loggedinas" />
	
	<asp:Label ID="LoggedInName" Runat="server" CssClass="loggedinname" />
		
	<div class="controls">
		<asp:HyperLink
			ID="HelpButton"
			Runat="server"
			Target="ifolderHelp"
			CssClass="helplinkcss" />
		|
		<asp:LinkButton 
			ID="LogoutButton" 
			Runat="server" 
			CssClass="helplinkcss" 
			OnClick="OnLogoutButton_Click" /> 
	</div>		
</div>
