<%@ Page Language="C#" Codebehind="Share.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.SharePage" %>
<%@ Register TagPrefix="iFolder" TagName="Header" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Message" Src="Message.ascx" %>
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

<body id="share">

<div id="container">
	
	<form runat="server">

		<iFolder:Header runat="server" />
		
		<div id="nav">
		</div>
	
		<div id="content">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="section">
				<%= GetString("SHARE") %>
			</div>
			
			<div class="main">
				
				<div class="path">
					<asp:Literal ID="iFolderName" runat="server" />
				</div>

				<div class="search">
					<asp:DropDownList ID="SearchPropertyList" runat="server" />
					<asp:TextBox ID="SearchPattern" CssClass="searchText" runat="server" onkeydown="return SubmitKeyDown(event, 'SearchButton');" />
					<asp:Button ID="SearchButton" CssClass="" runat="server" />
				</div>
				
				<table class="columns"><tr><td>
				
					<div id="share-users">
			
						<div class="sub-section"><%= GetString("AVAILABLEUSERS") %></div>
						
						<asp:DataGrid
							ID="UserData"
							GridLines="none"
							AutoGenerateColumns="false"
							ShowHeader="false"
							CssClass="list"
							ItemStyle-CssClass="row"
							AlternatingItemStyle-CssClass="altrow"
							runat="server">
							
							<columns>
								<asp:TemplateColumn ItemStyle-CssClass="icon">
									<itemtemplate>
										<asp:LinkButton CommandName="Add" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
											<asp:Image ImageUrl='images/list-add.png' runat="server" />
										</asp:LinkButton>
										<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
											<asp:Image ImageUrl='images/list-add-disabled.png' runat="server" />
										</div>
									</itemtemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn ItemStyle-CssClass="icon">
									<itemtemplate>
										<asp:LinkButton CommandName="Add" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
											<asp:Image ImageUrl='images/user.png' runat="server" />
										</asp:LinkButton>
										<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
											<asp:Image ImageUrl='images/user-disabled.png' runat="server" />
										</div>
									</itemtemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn ItemStyle-CssClass="name">
									<itemtemplate>
										<asp:LinkButton CommandName="Add" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
											<%# DataBinder.Eval(Container.DataItem, "FullName") %>
										</asp:LinkButton>
										<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
											<%# DataBinder.Eval(Container.DataItem, "FullName") %>
										</div>
									</itemtemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn ItemStyle-CssClass="name">
									<itemtemplate>
										&nbsp;
									</itemtemplate>
								</asp:TemplateColumn>
							</columns>
						</asp:DataGrid>
							
						<iFolder:Pagging id="UserPagging" runat="server" />
		
					</div>
					
				</td><td>
				
					<div id="share-members">
			
						<div class="sub-section"><%= GetString("SHAREWITH") %></div>
						
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
								<asp:TemplateColumn ItemStyle-CssClass="icon">
									<itemtemplate>
										<asp:LinkButton CommandName="Remove" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
											<asp:Image ImageUrl='images/list-remove.png' runat="server" />
										</asp:LinkButton>
										<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
											<asp:Image ImageUrl='images/list-remove-disabled.png' runat="server" />
										</div>
									</itemtemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn ItemStyle-CssClass="icon">
									<itemtemplate>
										<asp:LinkButton CommandName="Remove" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
											<asp:Image ImageUrl='images/user.png' runat="server" />
										</asp:LinkButton>
										<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
											<asp:Image ImageUrl='images/user-disabled.png' runat="server" />
										</div>
									</itemtemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn ItemStyle-CssClass="name">
									<itemtemplate>
										<asp:LinkButton CommandName="Remove" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
											<%# DataBinder.Eval(Container.DataItem, "FullName") %>
										</asp:LinkButton>
										<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
											<%# DataBinder.Eval(Container.DataItem, "FullName") %>
										</div>
									</itemtemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn ItemStyle-CssClass="name">
									<itemtemplate>
										&nbsp;
									</itemtemplate>
								</asp:TemplateColumn>
							</columns>
						</asp:DataGrid>
							
						<iFolder:Pagging id="MemberPagging" runat="server" />
		
					</div>
					
				</td></tr></table>
		
				<div class="buttons">
					<asp:Button ID="ShareButton" runat="server" />
					<asp:Button ID="CancelButton" runat="server" />
				</div>
				
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
