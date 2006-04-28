<%@ Control Language="c#" AutoEventWireup="false" Codebehind="TabControl.ascx.cs" Inherits="Novell.iFolderApp.Web.TabControl" %>

<div class="tabs">
	<div id="browseTab" class="tab"><asp:HyperLink ID="BrowseLink" runat="server" /></div>
	<div id="searchTab" class="tab"><asp:HyperLink ID="SearchLink" runat="server" /></div>
	<div id="membersTab" class="tab"><asp:HyperLink ID="MembersLink" runat="server" /></div>
	<div id="historyTab" class="tab"><asp:HyperLink ID="HistoryLink" runat="server" /></div>
	<div id="detailsTab" class="tab"><asp:HyperLink ID="DetailsLink" runat="server" /></div>
</div>
			
