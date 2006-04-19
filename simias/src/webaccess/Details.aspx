<%@ Page Language="C#" Codebehind="Details.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.DetailsPage" %>
<%@ Register TagPrefix="iFolder" TagName="Header" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Message" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Context" Src="Context.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Quota" Src="Quota.ascx" %>
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

<body id="details">

<div id="container">
	
	<form runat="server">

		<iFolder:Header runat="server" />
		
		<div id="nav">
	
			<div id="Actions" class="actions" runat="server">
				<div class="action">
					<asp:HyperLink ID="iFolderEditLink" runat="server" />
				</div>
			</div>
			
			<iFolder:Quota runat="server" />

		</div>
	
		<div id="content">
		
			<iFolder:Context id="iFolderContext" runat="server" />
	
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="main">
			
				<asp:DataGrid
					ID="DetailData"
					GridLines="none"
					AutoGenerateColumns="false"
					ShowHeader="false"
					CssClass="list"
					ItemStyle-CssClass="row"
					AlternatingItemStyle-CssClass="altrow"
					runat="server">
					
					<columns>
						<asp:TemplateColumn ItemStyle-CssClass="label">
							<itemtemplate>
								<%# DataBinder.Eval(Container.DataItem, "Label") %>:&nbsp;
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="value">
							<itemtemplate>
								<%# DataBinder.Eval(Container.DataItem, "Value") %>
							</itemtemplate>
						</asp:TemplateColumn>
						
					</columns>
				</asp:DataGrid>

			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
