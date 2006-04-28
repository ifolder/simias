<%@ Control Language="c#" Codebehind="iFolderContext.ascx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFolderContextControl"%>

<!-- context -->
<div id="context">
	<table><tr>
		
		<td class="home">
			<asp:HyperLink NavigateUrl="iFolders.aspx" runat="server">
				<asp:Image ImageUrl="images/go-home.png" runat="server" />
			</asp:HyperLink>
			<asp:HyperLink ID="HomeLink" NavigateUrl="iFolders.aspx" runat="server" />
		</td>
		
		<td class="sep">:</td>
		
		<td class="ifolder">
			<asp:HyperLink ID="iFolderImageLink" runat="server">
				<asp:Image ImageUrl="images/ifolder.png" runat="server" />
			</asp:HyperLink>
			<asp:HyperLink ID="iFolderLink" NavigateUrl="iFolders.aspx" runat="server" />
		</td>
		
		<td class="search">
			<asp:TextBox ID="SearchPattern" AutoPostBack="true" CssClass="searchText" runat="server" />
		</td>
		
	</tr></table>
</div>
