<%@ Page Language="C#" Codebehind="iFolders.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFoldersPage" %>
<%@ Register TagPrefix="iFolder" TagName="Header" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Message" Src="Message.ascx" %>
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
	
		function ConfirmRemove(f)
		{
			return confirm("<%= GetString("IFOLDER.CONFIRMREMOVES") %>");
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

			document.getElementById("RemoveButton").style.display = (count > 0) ? "" : "none";
			document.getElementById("RemoveDisabled").style.display = (count > 0) ? "none" : "";
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

		function SetFocus()
		{
			document.getElementById("SearchPattern").select();
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>

</head>

<body>

<div id="container">
	
	<form runat="server">

		<iFolder:Header runat="server" />
		
		<div id="nav">
	
			<div class="actions">
				<div class="action">
					<asp:HyperLink ID="NewiFolderLink" NavigateUrl="iFolderNew.aspx" runat="server" />
				</div>
				<div class="action">
					<span id="RemoveDisabled"><%= GetString("REMOVE") %></span>
					<asp:LinkButton ID="RemoveButton" style="display:none;" runat="server" />
				</div>
			</div>
			
			<iFolder:Quota runat="server" />

		</div>
	
		<div id="content">
		
			<div id="context">
				<div class="home"><%= GetString("IFOLDERS") %></div>
			</div>
					
			<div id="Tabs" class="AllPage tabs" runat="server">
				<div id="allTab" class="tab"><asp:LinkButton ID="AllButton" runat="server" /></div>
				<div id="newTab" class="tab"><asp:LinkButton ID="NewButton" runat="server" /></div>
				<div id="ownerTab" class="tab"><asp:LinkButton ID="OwnerButton" runat="server" /></div>
				<div id="sharedTab" class="tab"><asp:LinkButton ID="SharedButton" runat="server" /></div>
			</div>
			
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="main">
				
				<div class="filter">
					<asp:TextBox ID="SearchPattern" CssClass="filterText" runat="server" onkeydown="return SubmitKeyDown(event, 'SearchButton');" />
					<asp:Button ID="SearchButton" CssClass="" runat="server" />
				</div>
			
				<asp:DataGrid
					ID="iFolderData"
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
								<asp:CheckBox ID="Select" Enabled='<%# !(bool) DataBinder.Eval(Container.DataItem, "Owner") %>' onclick="SelectionUpdate(this)" runat="server" />
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="icon">
							<itemtemplate>
								<asp:HyperLink NavigateUrl='<%# "Browse.aspx?iFolder=" + DataBinder.Eval(Container.DataItem, "ID") %>' runat="server">
									<asp:Image ImageUrl='<%# "images/" + DataBinder.Eval(Container.DataItem, "Image") %>' runat="server" />
								</asp:HyperLink>
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="name">
							<itemtemplate>
								<asp:HyperLink NavigateUrl='<%# "Browse.aspx?iFolder=" + DataBinder.Eval(Container.DataItem, "ID") %>' runat="server">
									<%# DataBinder.Eval(Container.DataItem, "Name") %>
								</asp:HyperLink>
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="date">
							<itemtemplate>
								<%# DataBinder.Eval(Container.DataItem, "LastModified") %>
							</itemtemplate>
						</asp:TemplateColumn>
						
					</columns>
				</asp:DataGrid>
				
				<iFolder:Pagging id="iFolderPagging" runat="server" />
					
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
