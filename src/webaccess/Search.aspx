<%@ Page Language="C#" Codebehind="Search.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.SearchPage" %>
<%@ Register TagPrefix="iFolder" TagName="HeaderControl" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MessageControl" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="iFolderContextControl" Src="iFolderContext.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TabControl" Src="TabControl.ascx" %>
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
	
		function ConfirmDelete(f)
		{
			return confirm("<%= GetString("ENTRY.CONFIRMDELETE") %>");
		}
	
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

		function SelectionUpdate(cb)
		{
			// MONO work-around
			if (cb.nodeName != "INPUT")
			{
				cb = cb.firstChild;
			}
			
			var f = cb.form;
			var count = 0;
			
			for(i=0; i < f.elements.length; i++)
			{
				var e = f.elements[i];
				
				if ((e.type == "checkbox") && (e.checked))
				{
					count++;
				}
			}

			document.getElementById("DeleteButton").style.display = (count > 0) ? "" : "none";
			document.getElementById("DeleteDisabled").style.display = (count > 0) ? "none" : "";
		}
	
		function SetFocus()
		{
			document.getElementById("iFolderContext_SearchPattern").select();
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>

</head>

<body id="search">

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
		
			<iFolder:MessageControl id="Message" runat="server" />
	
			<div class="main">
				
				<div id="Actions" class="actions" runat="server">
					<div class="action">
						<span id="DeleteDisabled"><%= GetString("DELETE") %></span>
						<asp:LinkButton ID="DeleteButton" style="display:none;" runat="server" />	
					</div>
				</div>
			
				<asp:DataGrid
					ID="EntryData"
					GridLines="none"
					AutoGenerateColumns="false"
					ShowHeader="false"
					CssClass="list"
					ItemStyle-CssClass="row"
					AlternatingItemStyle-CssClass="altrow"
					runat="server">
					
					<columns>
						<asp:BoundColumn DataField="ID" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="cb">
							<itemtemplate>
								<asp:CheckBox ID="Select" onclick="SelectionUpdate(this)" runat="server" />
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="icon">
							<itemtemplate>
								<asp:HyperLink NavigateUrl='<%# DataBinder.Eval(Container.DataItem, "Link") %>' runat="server"><asp:Image ImageUrl='<%# "images/" + DataBinder.Eval(Container.DataItem, "Image") %>' runat="server" /></asp:HyperLink>
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="name">
							<itemtemplate>
								<asp:HyperLink NavigateUrl='<%# DataBinder.Eval(Container.DataItem, "Link") %>' runat="server"><%# DataBinder.Eval(Container.DataItem, "Name") %></asp:HyperLink>
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="date">
							<itemtemplate>
								<%# DataBinder.Eval(Container.DataItem, "LastModified") %>
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="size">
							<itemtemplate>
								<%# DataBinder.Eval(Container.DataItem, "Size") %>
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="history">
							<itemtemplate>
								<asp:HyperLink NavigateUrl='<%# "ItemHistory.aspx?iFolder=" + DataBinder.Eval(Container.DataItem, "iFolderID") + "&Item=" + DataBinder.Eval(Container.DataItem, "ID") + "&Type=File" %>' runat="server"><asp:Image ImageUrl="images/document-properties.png" runat="server" /></asp:HyperLink>
							</itemtemplate>
						</asp:TemplateColumn>
						
					</columns>
				</asp:DataGrid>
			
				<iFolder:PaggingControl id="EntryPagging" runat="server" />
					
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
