<%@ Page Language="C#" Codebehind="Details.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.DetailsPage" %>
<%@ Register TagPrefix="iFolder" TagName="HeaderControl" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="iFolderContextControl" Src="iFolderContext.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TabControl" Src="TabControl.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MessageControl" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="QuotaControl" Src="Quota.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="iFolderActionsControl" Src="iFolderActions.ascx" %>
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
	
		function SetFocus()
		{
			document.getElementById("iFolderContext_SearchPattern").select();
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>
	
</head>

<body id="details">

<div id="container">
	
	<form runat="server">

		<iFolder:HeaderControl runat="server" />
		
		<iFolder:iFolderContextControl id="iFolderContext" runat="server" />
	
		<div id="nav">
	
			<iFolder:TabControl runat="server" />
	
			<iFolder:iFolderActionsControl runat="server" />

			<iFolder:QuotaControl runat="server" />

		</div>
	
		<div id="content">
		
			<iFolder:MessageControl id="Message" runat="server" />
	
			<div class="main">
				
				<div>
					
					<div class="section"><%= GetString("PROPERTIES") %></div>
						
					<div id="PropertyActions" class="actions" runat="server">
						<div class="action">
							<asp:HyperLink ID="PropertyEditLink" runat="server" />
						</div>
					</div>
					
					<asp:DataGrid
						ID="PropertyData"
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
					
				<div visible="false" runat="server">
					
					<div class="section"><%= GetString("POLICY") %></div>
						
					<div id="PolicyActions" class="actions" runat="server">
						<div class="action">
							<asp:HyperLink ID="PolicyEditLink" runat="server" />
						</div>
					</div>
					
					<asp:DataGrid
						ID="PolicyData"
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
	
		</div>
	
	</form>

</div>

</body>

</html>
