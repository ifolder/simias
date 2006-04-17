<%@ Control Language="c#" AutoEventWireup="false" Codebehind="Context.ascx.cs" Inherits="Novell.iFolderApp.Web.Context" %>

<div id="context">
	<div class="home"><asp:HyperLink ID="HomeLink" NavigateUrl="iFolders.aspx" runat="server" /></div>
	<div class="sep">&frasl;</div>
	<div class="page"><asp:Literal ID="iFolderNameLiteral" runat="server" /></div>
</div>

<div class="tabs">
	<div id="browseTab" class="tab"><asp:HyperLink ID="BrowseLink" runat="server" /></div>
	<div id="searchTab" class="tab"><asp:HyperLink ID="SearchLink" runat="server" /></div>
	<div id="detailsTab" class="tab"><asp:HyperLink ID="DetailsLink" runat="server" /></div>
	<div id="sharingTab" class="tab"><asp:HyperLink ID="SharingLink" runat="server" /></div>
	<div id="historyTab" class="tab"><asp:HyperLink ID="HistoryLink" runat="server" /></div>
</div>
			
