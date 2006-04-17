<%@ Control Language="c#" Codebehind="Header.ascx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.Header"%>

<!-- header -->
<div id="header">
	
	<div class="panel">
	
		<div class="identity">
			<%= GetString("HEADER.LOGGEDIN") %>
			<strong><asp:Literal ID="FullName" runat="server" /></strong>
		</div>
		
		<div class="controls">
			<!--
			<asp:HyperLink ID="SettingsButton" CssClass="bannerLink" Target="ifolderSettings" runat="server" />
			|
			<asp:HyperLink ID="HelpButton" CssClass="bannerLink" Target="ifolderHelp" runat="server" />
			|
			-->
			<asp:LinkButton ID="LogoutButton" CssClass="bannerLink" runat="server" />
		</div>
	
	</div>

</div>