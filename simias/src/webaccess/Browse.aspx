<%@ Page Language="C#" Codebehind="Browse.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.BrowsePage" %>
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

<body id="browse">

<div id="container">
	
	<form runat="server">

		<iFolder:HeaderControl id="Head" runat="server" />
		
<!--		<iFolder:iFolderContextControl id="iFolderContext" runat="server" /> -->
	
		<div id="nav">
	
			<iFolder:TabControl id="Tabs" runat="server" />
	
			<iFolder:iFolderActionsControl runat="server" />

			<iFolder:QuotaControl runat="server" />

		</div>
	
		<div id="content">
		<iFolder:iFolderContextControl id="iFolderContext" runat="server" />
		
			<iFolder:MessageControl id="Message" runat="server" />
	
			<div class="main">
				
				<div class="path" style="display:none">
					<asp:Image ImageUrl="images/folder.png" runat="server" />
					<asp:Repeater ID="EntryPathList" runat="server">
						<itemtemplate>
							<asp:LinkButton CommandName='<%# DataBinder.Eval(Container.DataItem, "Path") %>' runat="server"> <%# DataBinder.Eval(Container.DataItem, "Name") %></asp:LinkButton> /
						</itemtemplate>
					</asp:Repeater>
					<asp:Literal ID="EntryPathLeaf" runat="server" />
				</div>
					
				<div id="Actions" class="actions" runat="server">
					<div class="action">
						<asp:HyperLink ID="NewFolderLink" runat="server" />
					</div>
					
					<div class="sep"></div>
						<asp:Label ID="FirstSingleStick" runat="server" /> 
					<div class="action">
						<asp:HyperLink ID="UploadFilesLink" runat="server" />
					</div>
					
					<div class="sep"></div>
						<asp:Label ID="SecondSingleStick" runat="server" /> 
					<div class="action">
						<!--<span id="DeleteDisabled"> <%= GetString("DELETE") %></span> -->
						<asp:Label ID="DeleteDisabled" runat="server" /> 
						<asp:LinkButton ID="DeleteButton" style="display:none;" runat="server" /> 	
					</div>
				</div>
				
                                <!-- added to show column heading style -->
                                <!-- column headings must line up with columns to be effective -->
                                <div class="ColumnHead">Name</div>

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
				
				<div class="labels"></div>	
					
					<asp:Label ID="PassPhraseLabel" Visible = "false" runat="server" />
					<asp:TextBox ID="PassPhraseText" TextMode="Password" Visible = "false"  runat="server" /><br><br>
					<asp:Button ID="OKButton" Visible = "false"  runat="server" />
					<asp:Button ID="CancelButton" Visible = "false"  runat="server" />
				
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
