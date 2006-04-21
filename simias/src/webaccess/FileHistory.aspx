<%@ Page Language="C#" Codebehind="FileHistory.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.FileHistoryPage" %>
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

<div id="container">
	
	<form runat="server">

		<iFolder:Header runat="server" />
		
		<div id="nav">
		</div>
	
		<div id="content">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="section">
				<%= GetString("FILEHISTORY") %>
			</div>
			
			<div class="main">
				
				<div class="path">
					<asp:Literal ID="EntryName" runat="server" />
				</div>

				<asp:DataGrid
					ID="HistoryData"
					GridLines="none"
					AutoGenerateColumns="false"
					ShowHeader="false"
					CssClass="list"
					ItemStyle-CssClass="row"
					AlternatingItemStyle-CssClass="altrow"
					runat="server">
					
					<columns>
						<asp:TemplateColumn ItemStyle-CssClass="icon">
							<itemtemplate>
								<asp:Image ImageUrl='<%# "images/change-" + DataBinder.Eval(Container.DataItem, "Image") + ".png" %>' runat="server" />
							</itemtemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn ItemStyle-CssClass="action">
							<itemtemplate>
								<%# DataBinder.Eval(Container.DataItem, "Type") + "&nbsp;" + GetString("BY") + "&nbsp;" + DataBinder.Eval(Container.DataItem, "UserFullName") %>
							</itemtemplate>
						</asp:TemplateColumn>
						<asp:BoundColumn DataField="Time" ItemStyle-CssClass="datetime" />
					</columns>
				</asp:DataGrid>
					
				<iFolder:Pagging id="HistoryPagging" runat="server" />
		
				<div class="actions">
					<asp:Button ID="CloseButton" runat="server" />
				</div>
	
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
