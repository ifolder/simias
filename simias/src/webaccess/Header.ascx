<%@ Control Language="c#" Codebehind="Header.ascx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.Header"%>

<!-- header -->
<div id="header">

	<div id="controls">
		<span class="name">
			<%= GetString("HEADER.LOGGEDIN") %>
			<strong><asp:Literal ID="FullName" runat="server" /></strong>
		</span>
		<!--
		<asp:HyperLink ID="SettingsButton" CssClass="bannerLink" Target="ifolderSettings" runat="server" />
		|
		<asp:HyperLink ID="HelpButton" CssClass="bannerLink" Target="ifolderHelp" runat="server" />
		|
		-->
		<asp:LinkButton ID="LogoutButton" CssClass="bannerLink" runat="server" />
	</div>
	
</div>