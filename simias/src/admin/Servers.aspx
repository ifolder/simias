<%@ Page language="c#" Codebehind="Servers.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Server" %>
<%@ Register TagPrefix="iFolder" TagName="ListFooter" Src="ListFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="iFolderSearch" Src="iFolderSearch.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 
<html>

<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/Servers.css);
	</style>

</head>

<body id="server" runat="server">
	
<form runat="server">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
		
		<div class="nav">
		
			<div class="pagetitle">
				
				<%= GetString( "SERVERS" ) %>
			
			</div>
				
			<iFolder:iFolderSearch ID="ServerSearch" Runat="server" />
					
			<div class="serversnav">
			
				<table class="serverlistheader" cellpadding="0" cellspacing="0" border="0">
			
					<tr>
						<td class="typecolumn">
							<%= GetString( "TYPE" ) %>
						</td>
						
						<td class="namecolumn">
							<%= GetString( "NAME" ) %>
						</td>
						
						<td class="dnscolumn">
							<%= GetString( "DNSNAME" ) %>
						</td>
						
						<td class="publicuricolumn">
							<%= GetString( "PUBLICURI" ) %>
						</td>
						
						<td class="privateuricolumn">
							<%= GetString( "PRIVATEURI" ) %>
						</td>
						
						<td class="statuscolumn">
							<%= GetString( "STATUS" ) %>
						</td>
					</tr>
			
				</table>
			
				<asp:datagrid 
					id="ServerList" 
					runat="server" 
					AutoGenerateColumns="False" 
					PageSize="15" 
					CellPadding="0"
					CellSpacing="0" 
					GridLines="None" 
					ShowHeader="False" 
					CssClass="serverlist" 
					AlternatingItemStyle-CssClass="serverlistaltitem" 
					ItemStyle-CssClass="serverlistitem">
					
					<Columns>
					
						<asp:BoundColumn DataField="IDField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="serveritem1">
							<ItemTemplate>
								<asp:Image 
									ID="ServerListImage" 
									Runat="server" 
									ImageUrl='<%# GetServerImage( DataBinder.Eval( Container.DataItem, "TypeField" ) ) %>' 
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:HyperLinkColumn 
							ItemStyle-CssClass="serveritem2" 
							DataTextField="NameField" 
							DataNavigateUrlField="IDField"
							DataNavigateUrlFormatString="ServerDetails.aspx?id={0}" 
							Target="_top" />
							
						<asp:BoundColumn ItemStyle-CssClass="serveritem3" DataField="DnsField" />
						
						<asp:BoundColumn ItemStyle-CssClass="serveritem4" DataField="PublicUriField" />
						
						<asp:BoundColumn ItemStyle-CssClass="serveritem5" DataField="PrivateUriField" />
						
						<asp:BoundColumn ItemStyle-CssClass="serveritem6" DataField="StatusField" />
							
					</Columns>
					
				</asp:datagrid>
				
				<ifolder:ListFooter ID="ServerListFooter" Runat="server" />
				
			</div>
					
		</div>
		
	</div>
	
<!-- 	<ifolder:Footer id="footer" runat="server" /> -->
				
</form>
	
</body>

</html>
