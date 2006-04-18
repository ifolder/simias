<%@ Page Language="C#" Codebehind="Members.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.MembersPage" %>
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

<body id="sharing">

<div id="container">
	
	<form runat="server">

		<iFolder:Header runat="server" />
		
		<div id="nav">
	
			<div id="Actions" class="actions" runat="server">
				<div class="action">
					<asp:HyperLink ID="AddButton" runat="server" />
				</div>
				<div class="action">
					<span id="RemoveDisabled"><%= GetString("REMOVE") %></span>
					<asp:LinkButton ID="RemoveButton" style="display:none;" runat="server" />
				</div>
				<div class="action">
					<span id="ReadOnlyDisabled"><%= GetString("RIGHTS.READONLY") %></span>
					<asp:LinkButton ID="ReadOnlyButton" style="display:none;" runat="server" />
				</div>
				<div class="action">
					<span id="ReadWriteDisabled"><%= GetString("RIGHTS.READWRITE") %></span>
					<asp:LinkButton ID="ReadWriteButton" style="display:none;" runat="server" />
				</div>
				<div class="action">
					<span id="AdminDisabled"><%= GetString("RIGHTS.ADMIN") %></span>
					<asp:LinkButton ID="AdminButton" style="display:none;" runat="server" />
				</div>
				<div class="action">
					<span id="OwnerDisabled"><%= GetString("OWNER") %></span>
					<asp:LinkButton ID="OwnerButton" style="display:none;" runat="server" />
				</div>
			</div>
			
			<iFolder:Quota runat="server" />

		</div>
	
		<div id="content">
		
			<iFolder:Context id="iFolderContext" runat="server" />
	
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="main">
				
				<asp:DataGrid
					ID="MemberData"
					GridLines="none"
					AutoGenerateColumns="false"
					ShowHeader="false"
					CssClass="list"
					ItemStyle-CssClass="row"
					AlternatingItemStyle-CssClass="altrow"
					runat="server">
					
					<columns>
					
						<asp:BoundColumn DataField="ID" Visible="false" />
						
						<asp:TemplateColumn ItemStyle-CssClass="cb">
							<itemtemplate>
								<asp:CheckBox ID="Select" Visible='<%# Actions.Visible %>' Enabled='<%# !(bool)DataBinder.Eval(Container.DataItem, "IsOwner") %>'  onclick="SelectionUpdate(this)" runat="server" />
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="icon">
							<itemtemplate>
								<asp:Image ImageUrl='images/user.png' runat="server" />
							</itemtemplate>
						</asp:TemplateColumn>
						
						<asp:BoundColumn DataField="Name" ItemStyle-CssClass="name" />
						
						<asp:BoundColumn DataField="Rights" ItemStyle-CssClass="rights" />
						
					</columns>
				</asp:DataGrid>
					
				<iFolder:Pagging id="MemberPagging" runat="server" />
					
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
