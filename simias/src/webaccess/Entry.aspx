<%@ Page Language="C#" Codebehind="Entry.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.EntryPage" %>
<%@ Register TagPrefix="iFolder" TagName="Header" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Message" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Pagging" Src="Pagging.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
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
			<asp:Literal ID="iFolderName" runat="server" />
		</div>
		
		<div id="main">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
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
									<asp:Image ImageUrl='<%# "images/16/change-" + DataBinder.Eval(Container.DataItem, "Image") + ".png" %>' runat="server" />
								</itemtemplate>
							</asp:TemplateColumn>
							<asp:BoundColumn DataField="Time" ItemStyle-CssClass="datetime" />
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
		
		<iFolder:Footer runat="server" />

	</div>
	
</form>

</body>

</html>
