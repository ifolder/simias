<%@ Page Language="C#" Codebehind="History.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.HistoryPage" %>
<%@ Register TagPrefix="iFolder" TagName="HeaderControl" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="iFolderContextControl" Src="iFolderContext.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TabControl" Src="TabControl.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MessageControl" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="QuotaControl" Src="Quota.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="PaggingControl" Src="Pagging.ascx" %>
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
			//document.getElementById("iFolderContext_SearchPattern").select();
			// Only search page should have the search box, so disabling from other pages 
			document.getElementById("iFolderContext_SearchPattern").style.display = "none";
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>

</head>

<body id="history">

<div id="container">
	
	<form runat="server">

		<iFolder:HeaderControl id="Head" runat="server" />
		
		<iFolder:iFolderContextControl id="iFolderContext" runat="server" />
	
		<div id="nav">
	
			<iFolder:TabControl id="Tabs" runat="server" />
	
			<%--<iFolder:iFolderActionsControl runat="server" />--%>

			<iFolder:QuotaControl runat="server" />

		</div>
	
		<div id="content">

<%--	<iFolder:iFolderContextControl id="iFolderContext" runat="server" /> --%>
		
			<iFolder:MessageControl id="Message" runat="server" />
	
			<div class="main">

				
				<asp:DataGrid
					ID="HistoryData"
					GridLines="none"
					AutoGenerateColumns="false"
					ShowHeader="true"
					CssClass="list"
					ItemStyle-CssClass="row"
					AlternatingItemStyle-CssClass="altrow"
					runat="server">
				<HeaderStyle CssClass="ColumnHead">
                                 </HeaderStyle>				
					<columns>
						<asp:TemplateColumn ItemStyle-CssClass="icon">
							<itemtemplate>
								<asp:Image ImageUrl='<%# "images/change-" + DataBinder.Eval(Container.DataItem, "TypeImage") + ".png" %>' ToolTip='<%# DataBinder.Eval(Container.DataItem, "Name") %>' runat="server" />
							</itemtemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn ItemStyle-CssClass="icon">
							<itemtemplate>
								<asp:Image ImageUrl='<%# "images/change-" + DataBinder.Eval(Container.DataItem, "ActionImage") + ".png" %>' ToolTip='<%# GetToolTip(DataBinder.Eval(Container.DataItem, "Type")) %>' runat="server" />
							</itemtemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn ItemStyle-CssClass="name">
							<itemtemplate>
								<%# DataBinder.Eval(Container.DataItem, "ShortenedName") %>
								<span class="rights"><%# DataBinder.Eval(Container.DataItem, "NewRights") %></span>
							</itemtemplate>
						</asp:TemplateColumn>
						<asp:TemplateColumn ItemStyle-CssClass="action">
							<itemtemplate>
								<%# DataBinder.Eval(Container.DataItem, "Action") + "&nbsp;" + GetString("BY") + "&nbsp;" + DataBinder.Eval(Container.DataItem, "UserFullName") %>
							</itemtemplate>
						</asp:TemplateColumn>
						<asp:BoundColumn DataField="Time" ItemStyle-CssClass="datetime"/>
					</columns>
				</asp:DataGrid>
					
				<iFolder:PaggingControl id="HistoryPagging" runat="server" />
				
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
