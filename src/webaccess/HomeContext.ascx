<%@ Control Language="c#" Codebehind="HomeContext.ascx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.HomeContextControl"%>

<!-- context -->
<div id="context">
	<table><tr>
		
		<td class="home">
			<asp:HyperLink NavigateUrl="iFolders.aspx" runat="server">
				<asp:Image ImageUrl="images/go-home.png" runat="server" />
			</asp:HyperLink>
			<asp:HyperLink ID="HomeLink" NavigateUrl="iFolders.aspx" runat="server" />
		</td>
		
		<td class="filter"><%= GetString("SHOW") %>
			<asp:DropDownList ID="SearchCategory" AutoPostBack="true" runat="server" />
			<%= GetString("FILTER") %>
			<asp:TextBox ID="SearchPattern" AutoPostBack="true" CssClass="filterText" runat="server" />
		</td>
		
	</tr></table>
</div>
