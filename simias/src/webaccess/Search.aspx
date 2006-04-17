<%@ Page Language="C#" Codebehind="Search.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.SearchPage" %>
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
			document.getElementById("SearchPattern").select();
		}
		
		// on load
		window.onload = SetFocus;

	</script>

</head>

<body id="search">

<div id="container">
	
	<form runat="server">

		<iFolder:Header runat="server" />
		
		<div id="nav">
	
			<div class="actions">
				<div class="action">
					<span id="DeleteDisabled"><%= GetString("DELETE") %></span>
					<asp:LinkButton ID="DeleteButton" style="display:none;" runat="server" />	
				</div>
			</div>
			
			<iFolder:Quota runat="server" />

		</div>
	
		<div id="content">
		
			<iFolder:Context id="iFolderContext" runat="server" />
	
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="main">
				
				<div class="search">
					<asp:TextBox ID="SearchPattern" CssClass="searchText" runat="server" onkeydown="return SubmitKeyDown(event, 'SearchButton');" />
					<asp:Button ID="SearchButton" CssClass="hide" runat="server" />
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
								<asp:HyperLink NavigateUrl='<%# "FileHistory.aspx?iFolder=" + DataBinder.Eval(Container.DataItem, "iFolderID") + "&Entry=" + DataBinder.Eval(Container.DataItem, "ID") %>' Visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "IsDirectory") %>' runat="server"><asp:Image ImageUrl="images/document-properties.png" runat="server" /></asp:HyperLink>
							</itemtemplate>
						</asp:TemplateColumn>
						
					</columns>
				</asp:DataGrid>
			
				<iFolder:Pagging id="EntryPagging" runat="server" />
					
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>