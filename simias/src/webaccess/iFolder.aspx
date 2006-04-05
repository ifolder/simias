<%@ Page Language="C#" Codebehind="iFolder.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFolderPage" %>
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

			document.getElementById("RemoveButton").style.display = (count > 0) ? "" : "none";
			document.getElementById("ReadOnlyButton").style.display = (count > 0) ? "" : "none";
			document.getElementById("ReadWriteButton").style.display = (count > 0) ? "" : "none";
			document.getElementById("AdminButton").style.display = (count > 0) ? "" : "none";
			document.getElementById("OwnerButton").style.display = (count == 1) ? "" : "none";

			document.getElementById("RemoveDisabled").style.display = (count > 0) ? "none" : "";
			document.getElementById("ReadOnlyDisabled").style.display = (count > 0) ? "none" : "";
			document.getElementById("ReadWriteDisabled").style.display = (count > 0) ? "none" : "";
			document.getElementById("AdminDisabled").style.display = (count > 0) ? "none" : "";
			document.getElementById("OwnerDisabled").style.display = (count == 1) ? "none" : "";
		}
	
	</script>

</head>

<body>

<form runat="server">

	<div id="container">
	
		<iFolder:Header runat="server" />
	
		<div id="context">
			<asp:HyperLink ID="HomeButton" NavigateUrl="iFolders.aspx" runat="server" />
			/
			<asp:Literal ID="iFolderContextName" runat="server" />
		</div>
		
		<div id="main">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div id="general" class="section">
	
				<div class="title"><%= GetString("IFOLDER") %></div>
				
				<div class="content">
					
					<table>
						<tr>
							<td class="label"><%= GetString("NAME") %>:</td>
							<td><asp:HyperLink ID="iFolderButton" runat="server" /></td>
							<td class="seperator">&nbsp;</td>
							<td class="label"><%= GetString("SIZE") %>:</td>
							<td><asp:Literal ID="iFolderSize" runat="server" /></td>
						</tr>
						<tr>
							<td class="label"><%= GetString("OWNER") %>:</td>
							<td><asp:Literal ID="iFolderOwner" runat="server" /></td>
							<td class="seperator">&nbsp;</td>
							<td class="label"><%= GetString("FILES") %>:</td>
							<td><asp:Literal ID="iFolderFileCount" runat="server" /></td>
						</tr>
						<tr>
							<td class="label"><%= GetString("MEMBERS") %>:</td>
							<td><asp:Literal ID="iFolderMemberCount" runat="server" /></td>
							<td class="seperator">&nbsp;</td>
							<td class="label"><%= GetString("FOLDERS") %>:</td>
							<td><asp:Literal ID="iFolderFolderCount" runat="server" /></td>
						</tr>
						<tr>
							<td class="label"><%= GetString("DESCRIPTION") %>:</td>
							<td colspan="4"><asp:Literal ID="iFolderDescription" runat="server" /></td>
						</tr>
					</table>
					
				</div>
				
			</div>
			
			<div id="members" class="section">
	
				<div class="title"><%= GetString("SHAREDWITH") %></div>
				
				<div class="content">
					
					<div class="actions">
						<asp:HyperLink ID="AddButton" runat="server" />
						|
						<span id="RemoveDisabled"><%= GetString("REMOVE") %></span><asp:LinkButton ID="RemoveButton" style="display:none;" runat="server" />
						|
						<%= GetString("SETRIGHTS") %>:
						<span id="ReadOnlyDisabled"><%= GetString("RIGHTS.READONLY") %></span><asp:LinkButton ID="ReadOnlyButton" style="display:none;" runat="server" />,
						<span id="ReadWriteDisabled"><%= GetString("RIGHTS.READWRITE") %></span><asp:LinkButton ID="ReadWriteButton" style="display:none;" runat="server" />,
						<span id="AdminDisabled"><%= GetString("RIGHTS.ADMIN") %></span><asp:LinkButton ID="AdminButton" style="display:none;" runat="server" />,
						<span id="OwnerDisabled"><%= GetString("OWNER") %></span><asp:LinkButton ID="OwnerButton" style="display:none;" runat="server" />
					</div>
				
					<asp:DataGrid
						ID="MemberData"
						GridLines="none"
						AutoGenerateColumns="false"
						ShowHeader="false"
						CssClass="entries"
						runat="server">
						
						<columns>
						
							<asp:BoundColumn DataField="ID" Visible="false" />
							
							<asp:TemplateColumn ItemStyle-CssClass="cb">
								<itemtemplate>
									<asp:CheckBox ID="Select" Enabled='<%# !(bool)DataBinder.Eval(Container.DataItem, "IsOwner") %>'  onclick="SelectionUpdate(this)" runat="server" />
								</itemtemplate>
							</asp:TemplateColumn>
							
							<asp:TemplateColumn ItemStyle-CssClass="icon">
								<itemtemplate>
									<asp:Image ImageUrl='images/16/user.png' runat="server" />
								</itemtemplate>
							</asp:TemplateColumn>
							
							<asp:BoundColumn DataField="Name" ItemStyle-CssClass="name" />
							
							<asp:BoundColumn DataField="Rights" ItemStyle-CssClass="rights" />
							
						</columns>
					</asp:DataGrid>
						
					<iFolder:Pagging id="MemberPagging" runat="server" />
				
				</div>
				
			</div>
	
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
									<asp:Image ImageUrl='<%# "images/16/change-" + DataBinder.Eval(Container.DataItem, "Image") + ".png" %>' ToolTip='<%# DataBinder.Eval(Container.DataItem, "EntryName") %>' runat="server" />
								</itemtemplate>
							</asp:TemplateColumn>
							<asp:BoundColumn DataField="Time" ItemStyle-CssClass="datetime" />
							<asp:TemplateColumn ItemStyle-CssClass="name">
								<itemtemplate>
									<%# DataBinder.Eval(Container.DataItem, "ShortEntryName") %>
								</itemtemplate>
							</asp:TemplateColumn>
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
