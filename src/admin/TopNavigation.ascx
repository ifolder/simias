<%@ Control Language="c#" AutoEventWireup="false" Codebehind="TopNavigation.ascx.cs" Inherits="Novell.iFolderWeb.Admin.TopNavigation" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<div id="top">

	<div id="logo"><img src="images/if3sa-logo.png"></div>
	
	<ul id="navcontainer">
	
		<li class="users">
			<a id="UserLink" runat="server"><%= GetString( "USERS") %></a>
		</li>
		
		<li class="ifolders">
			<a id="iFolderLink" runat="server"><%= GetString( "IFOLDERS" ) %></a>
		</li>
		
		<li class="system">
			<a id="SystemLink" runat="server"><%= GetString( "SYSTEM" ) %></a>
		</li>
		
		<li class="server">
			<a id="ServerLink" runat="server"><%= GetString( "SERVERS" ) %></a>
		</li>
		
	</ul>
	
</div>

<div id="detailtop">
	Bread Crumb List
</div>

