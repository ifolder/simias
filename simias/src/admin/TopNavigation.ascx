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

<div class="detailtop">

	<asp:DataList 
		ID="BreadCrumbs" 
		Runat="server" 
		CssClass="breadcrumbs" 
		GridLines="None" 
		RepeatDirection="Horizontal" 
		ShowHeader="false">

		<SeparatorStyle CssClass="breadcrumbseparator" />		
		
		<SeparatorTemplate>
			<%= GetString( "BREADCRUMBSEPARATOR" ) %>
		</SeparatorTemplate>
		
		<ItemTemplate>
			<asp:HyperLink
				Runat="server"
				Target="_top" 
				CssClass="breadcrumblink"
				Text='<%# DataBinder.Eval( Container.DataItem, "CrumbField" ) %>'
				NavigateUrl='<%# DataBinder.Eval( Container.DataItem, "LinkField" ) %>' />
		</ItemTemplate>
		
	</asp:DataList>
	
</div>

<div id="ErrorPanel" runat="server" class="errorpanel">

	<asp:Label ID="ErrorMsg" Runat="server" CssClass="errormsg" />

</div>

