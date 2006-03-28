<%@ Page Language="C#" Codebehind="iFolders.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFoldersPage" %>
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

	<script type="text/javascript">

		function SubmitKeyDown(e, b)
		{
			var result = true;
			
			if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13))
			{
				document.getElementById(b).click();
				result = false;
			} 
			
			return result;
		}
	
		function SetFocus()
		{
			document.getElementById("SearchPattern").select();
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>

</head>

<body>

<form runat="server">

	<div id="container">
	
		<iFolder:Header runat="server" />
	
		<div id="context">
			<%= GetString("HOME") %>
		</div>
		
		<div id="main">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div id="ifolders" class="section">
	
				<div class="title"><%= GetString("IFOLDERS") %></div>
	
				<div class="content">
					
					<div class="actions">
						<table><tr><td>
						<asp:HyperLink ID="NewiFolderLink" NavigateUrl="Share.aspx" runat="server" />
						|
						Delete	
						</td><td class="search">
							<asp:TextBox ID="SearchPattern" CssClass="searchPattern" runat="server" onkeydown="return SubmitKeyDown(event, 'SearchButton');" />
							<asp:Button ID="SearchButton" CssClass="hide" runat="server" />
						</td></tr></table>
					</div>
					
					<asp:DataGrid
						ID="iFolderData"
						GridLines="none"
						AutoGenerateColumns="false"
						ShowHeader="false"
						CssClass="entries"
						runat="server">
						
						<columns>
							<asp:BoundColumn DataField="ID" Visible="False" />
							
							<asp:TemplateColumn ItemStyle-CssClass="cb">
								<itemtemplate>
									<asp:CheckBox runat="server" />
								</itemtemplate>
							</asp:TemplateColumn>
							
							<asp:TemplateColumn ItemStyle-CssClass="icon">
								<itemtemplate>
									<asp:HyperLink NavigateUrl='<%# "Entries.aspx?iFolder=" + DataBinder.Eval(Container.DataItem, "ID") %>' runat="server">
										<asp:Image ImageUrl='<%# "images/16/" + DataBinder.Eval(Container.DataItem, "Image") %>' runat="server" />
									</asp:HyperLink>
								</itemtemplate>
							</asp:TemplateColumn>
							
							<asp:TemplateColumn ItemStyle-CssClass="name">
								<itemtemplate>
									<asp:HyperLink NavigateUrl='<%# "Entries.aspx?iFolder=" + DataBinder.Eval(Container.DataItem, "ID") %>' runat="server">
										<%# DataBinder.Eval(Container.DataItem, "Name") %>
									</asp:HyperLink>
								</itemtemplate>
							</asp:TemplateColumn>
							
							<asp:TemplateColumn ItemStyle-CssClass="date">
								<itemtemplate>
									<%# DataBinder.Eval(Container.DataItem, "LastModified") %>
								</itemtemplate>
							</asp:TemplateColumn>
							
							<asp:TemplateColumn ItemStyle-CssClass="details">
								<itemtemplate>
									<asp:HyperLink NavigateUrl='<%# "iFolder.aspx?iFolder=" + DataBinder.Eval(Container.DataItem, "ID") %>' runat="server">
										<asp:Image ImageUrl="images/16/document-properties.png" ToolTip='<%# GetString("DETAILS") %>' runat="server" />
									</asp:HyperLink>
								</itemtemplate>
							</asp:TemplateColumn>
							
						</columns>
					</asp:DataGrid>
						
					<iFolder:Pagging id="iFolderPagging" runat="server" />
				
				</div>
								
			</div>
	
		</div>
		
	</div>
	
</form>

</body>

</html>
