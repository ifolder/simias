<%@ Page Language="C#" Codebehind="iFolder.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFolderPage" %>
<%@ Register TagPrefix="iFolder" TagName="Header" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Message" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Pagging" Src="Pagging.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
<html>

<head>
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	
	<title><%= GetString("TITLE") %></title>
	
	<link rel="SHORTCUT ICON" href="images/ifolder.ico">

	<style type="text/css">
		@import url(css/ifolder.css);
	</style>

</head>

<body>

<form runat="server">

	<div id="container">
	
		<iFolder:Header runat="server" />
	
		<div id="context">
			<asp:HyperLink ID="HomeButton" NavigateUrl="iFolders.aspx" runat="server" />
			/
			<asp:Literal ID="iFolderContextName" runat="server" />
		</div>
		
		<div id="main">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div id="general" class="section">
	
				<div class="title"><%= GetString("IFOLDER") %></div>
				
				<div class="content">
					
					<table>
						<tr>
							<td class="label"><%= GetString("NAME") %>:</td>
							<td><asp:Literal ID="iFolderName" runat="server" /> ( <asp:HyperLink ID="BrowseButton" runat="server" /> )</td>
							<td class="seperator">&nbsp;</td>
							<td class="label"><%= GetString("SIZE") %>:</td>
							<td><asp:Literal ID="iFolderSize" runat="server" /></td>
						</tr>
						<tr>
							<td class="label"><%= GetString("OWNER") %>:</td>
							<td><asp:Literal ID="iFolderOwner" runat="server" /></td>
							<td class="seperator">&nbsp;</td>
							<td class="label"><%= GetString("FILES") %>:</td>
							<td><asp:Literal ID="iFolderFileCount" runat="server" /></td>
						</tr>
						<tr>
							<td class="label"><%= GetString("MEMBERS") %>:</td>
							<td><asp:Literal ID="iFolderMemberCount" runat="server" /></td>
							<td class="seperator">&nbsp;</td>
							<td class="label"><%= GetString("FOLDERS") %>:</td>
							<td><asp:Literal ID="iFolderFolderCount" runat="server" /></td>
						</tr>
						<tr>
							<td class="label"><%= GetString("DESCRIPTION") %>:</td>
							<td colspan="4"><asp:Literal ID="iFolderDescription" runat="server" /></td>
						</tr>
					</table>
					
				</div>
				
			</div>
			
			<div id="members" class="section">
	
				<div class="title"><%= GetString("SHAREDWITH") %></div>
				
				<div class="content">
					
					<div class="actions">
						<asp:HyperLink ID="ShareButton" runat="server" />
						|
						Remove
					</div>
				
					<asp:DataGrid
						ID="MemberData"
						GridLines="none"
						AutoGenerateColumns="false"
						ShowHeader="false"
						CssClass="entries"
						runat="server">
						
						<columns>
						
							<asp:TemplateColumn ItemStyle-CssClass="cb">
								<itemtemplate>
									<asp:CheckBox runat="server" />
								</itemtemplate>
							</asp:TemplateColumn>
							
							<asp:TemplateColumn ItemStyle-CssClass="icon">
								<itemtemplate>
									<asp:Image ImageUrl='images/16/text-x-generic.png' runat="server" />
								</itemtemplate>
							</asp:TemplateColumn>
							
							<asp:BoundColumn DataField="Name" ItemStyle-CssClass="name" />
							
							<asp:BoundColumn DataField="Rights" ItemStyle-CssClass="rights" />
							
						</columns>
					</asp:DataGrid>
						
					<iFolder:Pagging id="MemberPagging" runat="server" />
				
				</div>
				
			</div>
	
			<div id="history" class="section">
	
				<div class="title"><%= GetString("HISTORY") %></div>
				
				<div class="content">
					
					<asp:DataGrid
						ID="HistoryData"
						GridLines="none"
						AutoGenerateColumns="false"
						ShowHeader="false"
						CssClass="entries"
						runat="server">
						
						<columns>
							<asp:TemplateColumn ItemStyle-CssClass="icon">
								<itemtemplate>
									<asp:Image ImageUrl='<%# "images/16/change-" + DataBinder.Eval(Container.DataItem, "Image") + ".png" %>' ToolTip='<%# DataBinder.Eval(Container.DataItem, "EntryName") %>' runat="server" />
								</itemtemplate>
							</asp:TemplateColumn>
							<asp:BoundColumn DataField="Time" ItemStyle-CssClass="datetime" />
							<asp:TemplateColumn ItemStyle-CssClass="name">
								<itemtemplate>
									<%# DataBinder.Eval(Container.DataItem, "ShortEntryName") %>
								</itemtemplate>
							</asp:TemplateColumn>
							<asp:TemplateColumn ItemStyle-CssClass="action">
								<itemtemplate>
									<%# DataBinder.Eval(Container.DataItem, "Type") + "&nbsp;" + GetString("BY") + "&nbsp;" + DataBinder.Eval(Container.DataItem, "UserFullName") %>
								</itemtemplate>
							</asp:TemplateColumn>
						</columns>
					</asp:DataGrid>
						
					<iFolder:Pagging id="HistoryPagging" runat="server" />
	
				</div>
				
			</div>
	
		</div>
		
	</div>
	
</form>

</body>

</html>
