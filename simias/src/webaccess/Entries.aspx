<%@ Page Language="C#" Codebehind="Entries.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.EntriesPage" %>
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

<body>

<form runat="server">

	<div id="container">
	
		<iFolder:Header runat="server" />
	
		<div id="context">
			<asp:HyperLink ID="HomeButton" NavigateUrl="iFolders.aspx" runat="server" />
			/
			<asp:Repeater ID="EntryPathList" runat="server">
				<itemtemplate>
					<asp:LinkButton CommandName='<%# DataBinder.Eval(Container.DataItem, "Path") %>' runat="server"> <%# DataBinder.Eval(Container.DataItem, "Name") %></asp:LinkButton> /
				</itemtemplate>
			</asp:Repeater>
			<asp:Literal ID="EntryPathLeaf" runat="server" />
		</div>
		
		<div id="main">
	
			<iFolder:Message id="MessageBox" runat="server" />

			<div id="entries" class="section">
		
				<div class="title"><%= GetString("FILES") %></div>
	
				<div class="content">
					
					<div class="actions">
						<table><tr><td>
						<span id="DeleteDisabled"><%= GetString("DELETE") %></span><asp:LinkButton ID="DeleteButton" style="display:none;" runat="server" />	
						</td><td class="search">
							<asp:TextBox ID="SearchPattern" CssClass="searchPattern" runat="server" onkeydown="return SubmitKeyDown(event, 'SearchButton');" />
							<asp:Button ID="SearchButton" CssClass="hide" runat="server" />
						</td></tr></table>
					</div>
					
					<asp:DataGrid
						ID="EntryData"
						GridLines="none"
						AutoGenerateColumns="false"
						ShowHeader="false"
						CssClass="entries"
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
									<asp:HyperLink NavigateUrl='<%# DataBinder.Eval(Container.DataItem, "Link") %>' runat="server"><asp:Image ImageUrl='<%# "images/16/" + DataBinder.Eval(Container.DataItem, "Image") %>' runat="server" /></asp:HyperLink>
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
							
							<asp:TemplateColumn ItemStyle-CssClass="details">
								<itemtemplate>
									<asp:HyperLink NavigateUrl='<%# "Entry.aspx?iFolder=" + DataBinder.Eval(Container.DataItem, "iFolderID") + "&Entry=" + DataBinder.Eval(Container.DataItem, "ID") %>' Visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "IsDirectory") %>' runat="server"><asp:Image ImageUrl="images/16/document-properties.png" runat="server" /></asp:HyperLink>
								</itemtemplate>
							</asp:TemplateColumn>
							
						</columns>
					</asp:DataGrid>
				
					<iFolder:Pagging id="EntryPagging" runat="server" />
				
				</div>
			
			</div>
	
			<div id="upload" class="section">
		
				<div class="title"><%= GetString("UPLOAD") %></div>
				
				<div class="content">
				
					<input id="UploadFile" type="file" runat="server" onKeyDown="return SubmitKeyDown(event, 'UploadButton');" />
					<br>
					<asp:Button ID="UploadButton" runat="server" />
				
				</div>
				
			</div>
			
			<div id="newfolder" class="section">
		
				<div class="title"><%= GetString("NEWFOLDER") %></div>
				
				<div class="content">
					
					<asp:TextBox ID="NewFolderName" runat="server" onkeydown="return SubmitKeyDown(event, 'NewFolderButton');" />
					<br>
					<asp:Button ID="NewFolderButton" runat="server" />
					
				</div>
				
			</div>
			
			<div id="ifolder" class="section">
		
				<div class="title"><%= GetString("IFOLDER") %></div>
			
				<div class="content">
					<table><tr>
						<td>
							<asp:HyperLink ID="iFolderImageButton" runat="server">
								<asp:Image ImageUrl="images/16/document-properties.png" runat="server" />
							</asp:HyperLink>
						</td>
						<td valign="top">
							<asp:HyperLink ID="iFolderButton" runat="server" />
						</td>
					</tr></table>
				</div>
				
			</div>
			
		</div>
		
		<iFolder:Footer runat="server" />

	</div>
	
</form>

</body>

</html>