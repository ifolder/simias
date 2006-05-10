<%@ Page Language="C#" Codebehind="ItemHistory.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.ItemHistoryPage" %>
<%@ Register TagPrefix="iFolder" TagName="HeaderControl" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MessageControl" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="PaggingControl" Src="Pagging.ascx" %>
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

		<iFolder:HeaderControl runat="server" />
		
		<div id="nav">
		</div>
	
		<div id="content">
		
			<iFolder:MessageControl id="Message" runat="server" />
	
			<div class="section">
				<%= GetString("HISTORY") %>
			</div>
			
			<div class="main">
				
				<div class="path">
					<asp:Image ID="ItemImage" runat="server" />
					<asp:Literal ID="ItemName" runat="server" />
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
								<asp:Image ImageUrl='<%# "images/change-" + DataBinder.Eval(Container.DataItem, "Image") + ".png" %>' ToolTip='<%# DataBinder.Eval(Container.DataItem, "Type") %>' runat="server" />
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
					
				<iFolder:PaggingControl id="HistoryPagging" runat="server" />
		
				<div class="buttons">
					<asp:Button ID="CloseButton" runat="server" />
				</div>
	
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
