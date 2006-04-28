<%@ Page Language="C#" Codebehind="iFolders.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFoldersPage" %>
<%@ Register TagPrefix="iFolder" TagName="HeaderControl" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="HomeContextControl" Src="HomeContext.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MessageControl" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="QuotaControl" Src="Quota.ascx" %>
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
	
		function SetFocus()
		{
			document.getElementById("HomeContext_SearchPattern").select();
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>

</head>

<body>

<div id="container">
	
	<form runat="server">

		<iFolder:HeaderControl runat="server" />
		
		<iFolder:HomeContextControl id="HomeContext" runat="server" />
		
		<div id="nav">
	
			<iFolder:QuotaControl runat="server" />

		</div>
	
		<div id="content">
		
			<iFolder:MessageControl id="Message" runat="server" />
	
			<div class="main">
					
				<div class="actions">
					<div class="action">
						<asp:HyperLink ID="NewiFolderLink" NavigateUrl="iFolderNew.aspx" runat="server" />
					</div>
					<div class="sep">|</div>
					<div class="action">
						<span id="RemoveDisabled"><%= GetString("REMOVE") %></span>
						<asp:LinkButton ID="RemoveButton" style="display:none;" runat="server" />
					</div>
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
				
				<iFolder:PaggingControl id="iFolderPagging" runat="server" />
					
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
